#!/bin/bash
# docker compose pour Carnet de salle, à utiliser à la place de « docker compose » ou « docker-compose »
# sur le NAS : même version que update.sh (bin/docker-compose, installé au premier déploiement) et
# images de la version en ligne (.deployed). Le docker-compose v1 du paquet Docker de DSM 7.1 ne sait
# pas lire docker-compose.yml.
#
# Exemples, dans DEPLOY_DIR (/volume1/docker/carnet-de-salle) :
#   bash dc.sh ps
#   bash dc.sh logs --tail 50 api
#   bash dc.sh restart api
set -euo pipefail
export PATH="/usr/local/bin:/usr/local/sbin:/usr/bin:/usr/sbin:/bin:/sbin:${PATH:-}"
cd "$(dirname "$0")"

[ -x bin/docker-compose ] || { echo "bin/docker-compose absent : lancer d'abord « bash update.sh --force »" >&2; exit 1; }

# Sans cela, docker compose prendrait IMAGE_TAG du .env (vide = latest), pas forcément la version en ligne
if [ -z "${IMAGE_TAG:-}" ] && [ -s .deployed ]; then
  IMAGE_TAG="$(cat .deployed)"
  export IMAGE_TAG
fi

exec bin/docker-compose "$@"
