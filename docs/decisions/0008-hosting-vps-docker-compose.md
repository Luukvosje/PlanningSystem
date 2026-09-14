# 0008 — Hosting: a VPS with Docker Compose

- **Date:** 2026-08-22
- **Status:** final, built
- **Touches:** `deploy/`, `.github/workflows/deploy.yml`, `Planning.Api/Dockerfile`, `Planning.Web/Dockerfile`

## Decision

MVP point 4 (live with real data) becomes a VPS with Docker Compose: API, database and Nuxt as
containers on one machine, with Caddy in front for TLS.

## Why

Cheap and portable, no lock-in to one cloud.

## Alternative considered

Azure App Service plus Azure SQL: less to administer and managed backups, but more expensive
per month.

## Consequences you carry yourself

Backups, TLS renewal and OS updates are your own work. They have to be scheduled *before* a
customer depends on them — see [runbooks/first-deploy.md](../runbooks/first-deploy.md) §9 and
`deploy/backup.sh`.

## Where this stands (2026-09-14)

Built and running. Two things in the original text no longer hold:

- The decision said **SQL Server**. The database became PostgreSQL 17 (commit `3460539`,
  2026-09-01) — Npgsql in `Planning.Infrastructure`, `postgres:17-alpine` in both compose
  files.
- The decision said "nothing built yet, no Dockerfile, no CI". It is all there now: two
  Dockerfiles, `deploy/docker-compose.yml`, `deploy/Caddyfile`, `deploy/backup.sh`, and two
  workflows (`ci.yml` on every PR, `deploy.yml` from `main` to the VPS).

The topology is described in [architecture/overview.md](../architecture/overview.md).
