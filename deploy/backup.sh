#!/usr/bin/env bash
#
# Nightly backup of everything that cannot be rebuilt from the repository: the database and the
# uploaded organization logos. Run from cron:
#   0 3 * * * /opt/planning/backup.sh >> /var/log/planning-backup.log 2>&1
#
# A backup that has never been restored is a guess. docs/runbooks/first-deploy.md has the restore drill;
# do it once before the first customer, not after the first incident.

set -euo pipefail

STACK_DIR="/opt/planning"
BACKUP_DIR="${STACK_DIR}/backups"

cd "${STACK_DIR}"

# shellcheck disable=SC1091
set -a
source "${STACK_DIR}/.env"
set +a

RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-7}"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
DUMP_FILE="${BACKUP_DIR}/planning-${TIMESTAMP}.sql.gz"

mkdir -p "${BACKUP_DIR}"

echo "[$(date -u +%FT%TZ)] Starting backup."

# --clean --if-exists makes the dump restorable over an existing database rather than only into
# an empty one, which is the situation you are actually in when restoring.
docker compose exec -T db pg_dump \
    --username "${POSTGRES_USER}" \
    --dbname "${POSTGRES_DB}" \
    --clean \
    --if-exists \
    | gzip > "${DUMP_FILE}"

# A dump that gzip cannot read is not a backup. Catch it now, while the source is still around.
if ! gzip -t "${DUMP_FILE}"; then
    echo "ERROR: ${DUMP_FILE} is corrupt. Keeping it for inspection and failing."
    exit 1
fi

DUMP_SIZE="$(du -h "${DUMP_FILE}" | cut -f1)"
echo "Database dumped to ${DUMP_FILE} (${DUMP_SIZE})."

# Uploaded logos live in a Docker volume, not in the database.
LOGO_ARCHIVE="${BACKUP_DIR}/logos-${TIMESTAMP}.tar.gz"
docker run --rm \
    -v planning_logo-data:/data:ro \
    -v "${BACKUP_DIR}:/backup" \
    alpine:3 \
    tar czf "/backup/$(basename "${LOGO_ARCHIVE}")" -C /data .
echo "Logos archived to ${LOGO_ARCHIVE}."

# Off-site copy. Local disk snapshots do not survive the server they live on.
if [ -n "${RESTIC_REPOSITORY:-}" ]; then
    export RESTIC_REPOSITORY RESTIC_PASSWORD
    restic backup "${BACKUP_DIR}" --tag planning
    restic forget --tag planning --keep-daily 7 --keep-weekly 4 --keep-monthly 6 --prune
    echo "Off-site copy complete."
else
    echo "WARNING: RESTIC_REPOSITORY is empty - backups exist only on this server."
    echo "         A failed disk or a deleted VPS takes them with it."
fi

# Local retention. The off-site repository keeps the longer history.
find "${BACKUP_DIR}" -name 'planning-*.sql.gz' -mtime "+${RETENTION_DAYS}" -delete
find "${BACKUP_DIR}" -name 'logos-*.tar.gz' -mtime "+${RETENTION_DAYS}" -delete

echo "[$(date -u +%FT%TZ)] Backup finished."
