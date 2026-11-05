#!/bin/sh
# Sauvegarde quotidienne de la base, à planifier dans DSM :
#   Panneau de configuration > Planificateur de tâches > Créer > Tâche planifiée > Script défini par l'utilisateur
#   Utilisateur : root ; commande : sh /volume1/docker/carnet-de-salle/backup.sh
# Les fichiers .bak arrivent dans DATA_DIR/backups : ajoute ce dossier à Hyper Backup
# pour en garder une copie hors du NAS.
set -eu
DEPLOY_DIR="$(cd "$(dirname "$0")" && pwd)"
set -a; . "$DEPLOY_DIR/.env"; set +a
DB="${DB_NAME:-CarnetDeSalle}"
FILE="/var/opt/mssql/backup/${DB}_$(date +%Y-%m-%d_%H%M).bak"

docker exec carnet-de-salle-db-1 /opt/mssql-tools18/bin/sqlcmd -C -b -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
  -Q "BACKUP DATABASE [$DB] TO DISK = N'$FILE' WITH INIT, CHECKSUM, NAME = N'$DB quotidienne'"

# Garde 14 jours de sauvegardes
find "$DATA_DIR/backups" -name "${DB}_*.bak" -mtime +14 -delete
echo "Sauvegarde OK : $FILE"
