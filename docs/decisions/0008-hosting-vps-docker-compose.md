# 0008 — Hosting: VPS met Docker Compose

- **Datum:** 2026-08-22
- **Status:** definitief, gebouwd
- **Raakt:** `deploy/`, `.github/workflows/deploy.yml`, `Planning.Api/Dockerfile`, `Planning.Web/Dockerfile`

## Besluit

MVP-punt 4 (live met echte data) wordt een VPS met Docker Compose: API, database en Nuxt als
containers op één machine, met Caddy ervoor voor TLS.

## Waarom

Goedkoop en portabel, geen lock-in bij één cloud.

## Alternatief overwogen

Azure App Service plus Azure SQL: minder eigen beheer en managed backups, maar duurder per
maand.

## Consequenties die je zelf moet dragen

Backups, TLS-vernieuwing en OS-updates zijn eigen werk. Die moeten ingepland zijn vóórdat er
een klant op staat — zie [runbooks/first-deploy.md](../runbooks/first-deploy.md) §9 en
`deploy/backup.sh`.

## Stand van zaken (2026-09-14)

Gebouwd en draaiend. Twee dingen uit de oorspronkelijke tekst kloppen niet meer:

- Het besluit noemde **SQL Server**. De database is PostgreSQL 17 geworden (commit `3460539`,
  2026-09-01) — Npgsql in `Planning.Infrastructure`, `postgres:17-alpine` in beide
  compose-bestanden.
- Het besluit zei "nog niets van gebouwd, geen Dockerfile, geen CI". Alles staat er nu:
  twee Dockerfiles, `deploy/docker-compose.yml`, `deploy/Caddyfile`, `deploy/backup.sh`,
  en twee workflows (`ci.yml` op elke PR, `deploy.yml` van `main` naar de VPS).

Details van de topologie staan in [architecture/overview.md](../architecture/overview.md).
