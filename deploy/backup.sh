#!/bin/sh
# Sauvegarde quotidienne de la base, à planifier dans DSM :
#   Panneau de configuration > Planificateur de tâches > Créer > Tâche planifiée > Script défini par l'utilisateur
#   Utilisateur : root ; commande : sh /volume1/docker/carnet-de-salle/backup.sh
# Les fichiers .bak arrivent dans DATA_DIR/backups : ajoute ce dossier à Hyper Backup
# pour en garder une copie hors du NAS.
set -eu
export PATH="/usr/local/bin:/usr/local/sbin:/usr/bin:/usr/sbin:/bin:/sbin:${PATH:-}"
DEPLOY_DIR="$(cd "$(dirname "$0")" && pwd)"
set -a
# .env est propre au NAS et absent du dépôt
# shellcheck disable=SC1091
. "$DEPLOY_DIR/.env"
set +a
DB="${DB_NAME:-CarnetDeSalle}"
FILE="/var/opt/mssql/backup/${DB}_$(date +%Y-%m-%d_%H%M).bak"

# dc.sh : docker compose v2 installé par update.sh (le paquet Docker de DSM 7.1 n'a que la v1)
bash "$DEPLOY_DIR/dc.sh" exec -T -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" db \
  /opt/mssql-tools18/bin/sqlcmd -C -b -S localhost -U sa \
  -Q "BACKUP DATABASE [$DB] TO DISK = N'$FILE' WITH INIT, CHECKSUM, NAME = N'$DB quotidienne'"

# Garde 14 jours de sauvegardes
find "$DATA_DIR/backups" -name "${DB}_*.bak" -mtime +14 -delete
echo "Sauvegarde OK : $FILE"
