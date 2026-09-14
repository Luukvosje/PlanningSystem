# Deploying and rolling back

For the day-to-day. Setting things up the first time is in [first-deploy.md](first-deploy.md).

## How a deploy runs

**`main` is production. Nothing else deploys.**

A push to `main` (or `workflow_dispatch`) starts `.github/workflows/deploy.yml`, in four
phases:

1. **verify** — `dotnet restore/build/test` on `Planning.slnx`, then
   `pnpm install --frozen-lockfile && lint && test && build` in `Planning.Web`. Exactly the
   same checks `ci.yml` runs on a PR.
2. **build** — two images in parallel (`planningsystem-api`, `planningsystem-web`) to ghcr.io,
   tagged with the **commit SHA** and `latest`. The SHA is the real tag; that is what reduces a
   rollback to one line.
3. **deploy** — ssh to the VPS, update `IMAGE_TAG` in `/opt/planning/.env` (the previous value
   is written to `.previous_tag`), `docker compose pull api web` and `up -d --remove-orphans`.
4. **ready check** — thirty attempts, five seconds apart, against
   `http://127.0.0.1:8080/health/ready` inside the api container. That endpoint includes the
   database, so it only passes once migrations have applied and the API can actually serve.
   On failure the workflow prints the last 60 lines of API log and the rollback instructions.

Two deploys cannot touch each other (`concurrency: deploy-production`) and are never cancelled
halfway (`cancel-in-progress: false`) — an interrupted deploy leaves the stack on mismatched
image versions.

## Migrations come along by themselves

The API applies pending migrations at startup. So there is no separate step — but also no
brake: **a destructive migration on `main` is executed by the next deploy without anybody
typing a command.** See the `db-schema-change` skill.

This only works because exactly one API instance runs. With a second they race the same
migration, and this has to become a separate one-shot job before the app starts.

## Rolling back

On the VPS, in `/opt/planning`:

```bash
cat .previous_tag
```

```bash
sed -i "s|^IMAGE_TAG=.*|IMAGE_TAG=<sha>|" .env && docker compose pull api web && docker compose up -d
```

Then check:

```bash
docker compose ps && docker compose exec -T api curl -fsS http://127.0.0.1:8080/health/ready
```

**What rolling back does *not* do: roll back the database.** Images go back, the schema does
not. If the broken deploy carried a migration that dropped or retyped columns, the old code now
runs against the new schema — and that usually ends badly. In that case the order is: restore
the database from backup first (see [first-deploy.md](first-deploy.md) §9), and only then put
the image back.

That is why a destructive migration deserves a conversation, not just a review.

## Quickly seeing what is wrong

```bash
docker compose ps
```

```bash
docker compose logs --tail 100 api
```

```bash
docker compose exec -T api curl -fsS http://127.0.0.1:8080/health/ready; echo " <- exit $?"
```

Two health endpoints, deliberately separate:

- **`/health/live`** — is this process up? No database. This is what the container healthcheck
  and the proxy poll, so a database hiccup does not restart a healthy API.
- **`/health/ready`** — includes the database. This is what the deploy waits for.

Neither is publicly reachable; Caddy deliberately does not route `/health/*`. Anything
reachable is something you have to keep safe.

## The containers

| Container | Ports | Data that persists |
|---|---|---|
| `caddy` | 80, 443, 443/udp — **the only one publishing** | `caddy-data` (certificates) |
| `web` | internal 3000 | — |
| `api` | internal 8080 | `logo-data` (organization logos) |
| `db` | internal 5432 | `db-data` |

Losing `caddy-data` means requesting new certificates and running into Let's Encrypt rate
limits. `logo-data` is the only user data outside the database — a backup has to cover both.

## When the ready check keeps failing

In order of likelihood:

1. **A migration is failing.** `docker compose logs api` shows the EF error. The API does not
   finish starting without a successful migration.
2. **The database is unhealthy.** `docker compose ps` shows `db` as unhealthy; `pg_isready` in
   the healthcheck fails. Check disk space (`df -h`).
3. **A missing environment variable.** `Jwt__Key`, `ConnectionStrings__DefaultConnection` and
   an empty `Cors__AllowedOrigins` are all three a fatal startup error, not a warning. See
   [references/configuration.md](../references/configuration.md).
4. **The wrong image tag.** `grep IMAGE_TAG .env` and compare with the SHA in the workflow.
