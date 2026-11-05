#!/bin/bash
# Mise à jour automatique de Carnet de salle sur le NAS.
#
# C'est le NAS qui va chercher les nouvelles versions : aucun runner GitHub, aucun port entrant,
# et le dépôt peut être public sans que GitHub puisse piloter le NAS.
#
# À planifier dans DSM : Panneau de configuration > Planificateur de tâches > Créer >
#   Tâche planifiée > Script défini par l'utilisateur, utilisateur root, toutes les 5 minutes :
#     bash /volume1/docker/carnet-de-salle/update.sh
#
# Fonctionnement :
#   1. version visée : IMAGE_TAG du .env si elle est fixée (ex. sha-1a2b3c4, pour figer ou revenir
#      en arrière), sinon le commit de la dernière publication réussie sur main (workflow « Images ») ;
#   2. déjà en ligne : le script s'arrête sans rien toucher ;
#   3. sinon : télécharge les images et les fichiers de déploiement de ce commit, prépare la base,
#      applique les migrations, redémarre l'API et le front ;
#   4. vérifie /api/health ; en cas d'échec, remet la version précédente et ne réessaie plus
#      cette version (fichier .failed).
#
# Commandes utiles (dans DEPLOY_DIR) :
#   bash update.sh --force    redéploie la version visée même si elle est déjà en ligne
#   touch .pause              suspend les mises à jour (rm .pause pour reprendre)
#   tail -n 50 logs/update.log
set -euo pipefail
export PATH="/usr/local/bin:/usr/local/sbin:/usr/bin:/usr/sbin:/bin:/sbin:${PATH:-}"

REPO="JeanLaurain/carnet-de-salle"
WORKFLOW="images.yml"
IMAGES="ghcr.io/jeanlaurain/carnet-de-salle"

DEPLOY_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$DEPLOY_DIR"
mkdir -p logs sql
LOG="$DEPLOY_DIR/logs/update.log"
log() { echo "$(date '+%F %T') $*" | tee -a "$LOG"; }

FORCE=false
[ "${1:-}" = "--force" ] && FORCE=true
[ -f .pause ] && ! $FORCE && exit 0

# Un seul passage à la fois (verrou abandonné au bout de 30 min)
if ! mkdir .update.lock 2>/dev/null; then
  if [ -n "$(find .update.lock -maxdepth 0 -mmin +30 2>/dev/null)" ]; then rmdir .update.lock; mkdir .update.lock; else exit 0; fi
fi
trap 'rmdir "$DEPLOY_DIR/.update.lock" 2>/dev/null || true' EXIT

# Garde le journal sous 2 000 lignes
if [ -f "$LOG" ] && [ "$(wc -l < "$LOG")" -gt 2000 ]; then tail -n 1000 "$LOG" > "$LOG.tmp" && mv "$LOG.tmp" "$LOG"; fi

dc() { if docker compose version >/dev/null 2>&1; then docker compose "$@"; else docker-compose "$@"; fi; }

[ -f .env ] || { log "ERREUR : $DEPLOY_DIR/.env manquant (modèle : deploy/.env.example)"; exit 1; }
unset GITHUB_TOKEN                # seulement celui du .env
set -a; . ./.env; set +a

# Jeton facultatif : seulement si le dépôt ou les images sont privés
AUTH=()
if [ -n "${GITHUB_TOKEN:-}" ]; then
  AUTH=(-H "Authorization: Bearer $GITHUB_TOKEN")
  echo "$GITHUB_TOKEN" | docker login ghcr.io -u "${GITHUB_USER:-JeanLaurain}" --password-stdin >/dev/null 2>&1 \
    || { log "ERREUR : connexion à ghcr.io refusée (GITHUB_TOKEN)"; exit 1; }
fi

# 1. Version visée
if [ -n "${IMAGE_TAG:-}" ] && [ "$IMAGE_TAG" != "latest" ]; then
  TAG="$IMAGE_TAG"
  REV="$(curl -fsSL ${AUTH[@]+"${AUTH[@]}"} "https://api.github.com/repos/$REPO/commits/${TAG#sha-}" 2>/dev/null \
    | grep -o '"sha": *"[0-9a-f]\{40\}"' | head -n 1 | grep -o '[0-9a-f]\{40\}' || true)"
  [ -n "$REV" ] || { log "ERREUR : commit introuvable pour IMAGE_TAG=$TAG"; exit 1; }
else
  REV="$(curl -fsSL ${AUTH[@]+"${AUTH[@]}"} "https://api.github.com/repos/$REPO/actions/workflows/$WORKFLOW/runs?branch=main&status=success&per_page=1" 2>/dev/null \
    | grep -o '"head_sha": *"[0-9a-f]\{40\}"' | head -n 1 | grep -o '[0-9a-f]\{40\}' || true)"
  [ -n "$REV" ] || exit 0   # GitHub injoignable ou aucune publication : on réessaiera au prochain passage
  TAG="sha-${REV:0:7}"
fi

# 2. Déjà en ligne, ou version déjà en échec
DEPLOYED="$(cat .deployed 2>/dev/null || true)"
if ! $FORCE; then
  [ "$DEPLOYED" = "$TAG" ] && exit 0
  [ "$(cat .failed 2>/dev/null || true)" = "$TAG" ] && exit 0
fi
log "Déploiement de $TAG (commit ${REV:0:7}, en ligne : ${DEPLOYED:-aucune})"

# 3. Fichiers de déploiement du même commit, puis images
fetch() {
  curl -fsSL ${AUTH[@]+"${AUTH[@]}"} "https://raw.githubusercontent.com/$REPO/$REV/$1" -o "$2.tmp" && mv "$2.tmp" "$2"
}
fetch deploy/docker-compose.yml docker-compose.yml
fetch deploy/sql/init-db.sql sql/init-db.sql
fetch deploy/backup.sh backup.sh
fetch deploy/update.sh update.sh   # le script en cours garde l'ancienne version ouverte : sans risque

export IMAGE_TAG="$TAG"
dc pull --quiet api web db

dc up -d --wait db
dc exec -T -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" db /opt/mssql-tools18/bin/sqlcmd -C -b -S localhost -U sa \
  -v DbName="${DB_NAME:-CarnetDeSalle}" AppUser="${DB_APP_USER:-carnet_app}" AppPassword="$DB_APP_PASSWORD" \
  -i /opt/carnet/sql/init-db.sql >/dev/null
dc --profile tools run --rm migrate
dc up -d --remove-orphans api web

# 4. Vérification, sinon retour à la version précédente
for _ in $(seq 1 30); do
  if dc exec -T web wget -qO- http://127.0.0.1:8080/api/health >/dev/null 2>&1; then
    echo "$TAG" > .deployed
    rm -f .failed
    docker image prune -f >/dev/null 2>&1 || true
    log "OK : $TAG en ligne"
    exit 0
  fi
  sleep 2
done

log "ÉCHEC : /api/health ne répond pas avec $TAG"
dc logs --tail 40 api web >> "$LOG" 2>&1 || true
echo "$TAG" > .failed
if [ -n "$DEPLOYED" ]; then
  export IMAGE_TAG="$DEPLOYED"
  dc up -d api web && log "Retour à $DEPLOYED (les migrations déjà appliquées restent en place)"
fi
exit 1
