#!/bin/bash
# Démarre les images de l'API et du front comme sur le NAS, et vérifie qu'elles répondent.
#
# Le NAS tourne sous DSM 7.1 avec le paquet Docker de Synology (Docker 20.10.3). Son profil seccomp
# refuse l'appel système clone3 (EPERM) au lieu de répondre qu'il ne le connaît pas (ENOSYS) : la glibc
# 2.34 et plus ne se rabat alors pas sur clone et ne peut plus créer de threads, si bien que les images
# Ubuntu ou Debian récentes ne démarrent pas. seccomp-dsm71.json reproduit exactement ce refus.
#
# Usage : test-images.sh <image api> <image web> <version attendue>
set -euo pipefail
API_IMAGE="$1"
WEB_IMAGE="$2"
VERSION="$3"
DIR="$(cd "$(dirname "$0")" && pwd)"
SECCOMP=(--security-opt "seccomp=$DIR/seccomp-dsm71.json")
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

echo "::group::Contrôle : le profil reproduit bien le refus de clone3"
THREAD='import threading; t = threading.Thread(target=print, args=("thread créé",)); t.start(); t.join()'
docker pull --quiet python:3.13-slim >/dev/null
docker pull --quiet python:3.13-alpine >/dev/null
# glibc (Debian) : la création du thread doit échouer
out="$(docker run --rm "${SECCOMP[@]}" python:3.13-slim python -c "$THREAD" 2>&1 || true)"
echo "glibc : $out"
grep -qi "start new thread" <<<"$out" \
  || { echo "::error::Le profil seccomp ne reproduit plus le comportement de Docker 20.10.3"; exit 1; }
# musl (Alpine) : doit marcher
echo "musl : $(docker run --rm "${SECCOMP[@]}" python:3.13-alpine python -c "$THREAD")"
echo "::endgroup::"

docker network create "$NET" >/dev/null
docker run -d --name carnet-test-api --network "$NET" --network-alias api "${SECCOMP[@]}" \
  -e ASPNETCORE_ENVIRONMENT=Production -e App__Domain=localhost -e DataProtection__KeysPath=/tmp/keys \
  "$API_IMAGE" >/dev/null
docker run -d --name carnet-test-web --network "$NET" -p 127.0.0.1:8380:8080 "${SECCOMP[@]}" \
  "$WEB_IMAGE" >/dev/null

# /api/version plutôt que /api/health : la santé dépendra de la base, absente de ce test
echo "API par le front : $(wait_for http://127.0.0.1:8380/api/version)"
curl -fsS http://127.0.0.1:8380/api/version | grep -q "\"$VERSION\"" \
  || { echo "::error::L'API ne renvoie pas la version $VERSION"; exit 1; }
curl -fsS http://127.0.0.1:8380/ | grep -q "<app-root" \
  || { echo "::error::Le front ne renvoie pas l'appli Angular"; exit 1; }
echo "Front : OK"
echo "Les images démarrent avec le seccomp de Docker 20.10.3 (DSM 7.1)."
