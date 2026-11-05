#!/bin/bash
# Démarre les images de l'API et du front comme sur le NAS (réseau Docker, nginx devant l'API)
# et vérifie qu'elles répondent, avant toute publication.
#
# Usage : test-images.sh <image api> <image web> <version attendue>
set -euo pipefail
API_IMAGE="$1"
WEB_IMAGE="$2"
VERSION="$3"
NET=carnet-test

cleanup() {
  local rc=$?
  if [ "$rc" -ne 0 ]; then
    echo "::group::Journaux des conteneurs"
    docker logs carnet-test-api 2>&1 | tail -n 50 || true
    docker logs carnet-test-web 2>&1 | tail -n 50 || true
    echo "::endgroup::"
  fi
  docker rm -f carnet-test-api carnet-test-web >/dev/null 2>&1 || true
  docker network rm "$NET" >/dev/null 2>&1 || true
}
trap cleanup EXIT

wait_for() {
  for _ in $(seq 1 30); do
    if curl -fsS "$1"; then return 0; fi
    sleep 2
  done
  return 1
}

docker network create "$NET" >/dev/null
docker run -d --name carnet-test-api --network "$NET" --network-alias api \
  -e ASPNETCORE_ENVIRONMENT=Production -e App__Domain=localhost -e DataProtection__KeysPath=/tmp/keys \
  "$API_IMAGE" >/dev/null
docker run -d --name carnet-test-web --network "$NET" -p 127.0.0.1:8380:8080 "$WEB_IMAGE" >/dev/null

# /api/version plutôt que /api/health : la santé dépendra de la base, absente de ce test
reponse="$(wait_for http://127.0.0.1:8380/api/version)" \
  || { echo "::error::L'API ne répond pas derrière nginx"; exit 1; }
echo "API par le front : $reponse"
grep -q "\"$VERSION\"" <<<"$reponse" \
  || { echo "::error::L'API ne renvoie pas la version $VERSION"; exit 1; }
curl -fsS http://127.0.0.1:8380/ | grep -q "<app-root" \
  || { echo "::error::Le front ne renvoie pas l'appli Angular"; exit 1; }
echo "Front : OK"
