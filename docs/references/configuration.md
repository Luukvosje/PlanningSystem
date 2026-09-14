# Configuration

Every setting, where it comes from and what happens when it is missing.

**No secret lives in an `appsettings*.json`.** Locally they come from `dotnet user-secrets`
(`Planning.Api` owns the secret store); in production from environment variables through
`deploy/.env`. The .NET separator: `Jwt:Key` in a JSON file is `Jwt__Key` as an environment
variable.

## Backend

| Key | Env var | Default | Missing → |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | — | **startup failure** |
| `Jwt:Key` | `Jwt__Key` | — | **startup failure** |
| `Jwt:Issuer` | `Jwt__Issuer` | `Planning.Api` | tokens fail validation |
| `Jwt:Audience` | `Jwt__Audience` | `Planning.Api` | tokens fail validation |
| `Jwt:ExpirationMinutes` | `Jwt__ExpirationMinutes` | 60 | — |
| `Jwt:RefreshTokenExpirationDays` | `Jwt__RefreshTokenExpirationDays` | 14 | — |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0`, `__1`, … | empty | **startup failure** |
| `Email:SmtpHost` | `Email__SmtpHost` | empty | mail goes to the log |
| `Email:SmtpPort` | `Email__SmtpPort` | 587 | — |
| `Email:Username` / `Password` | `Email__Username` / `Email__Password` | empty | — |
| `Email:FromAddress` | `Email__FromAddress` | empty | mail goes to the log |
| `Email:FromName` | `Email__FromName` | `Planning` | — |
| `Email:AppBaseUrl` | `Email__AppBaseUrl` | empty | links in mail point nowhere |

### Three things that are not obvious

**An empty CORS list is a fatal startup error, not a lenient default.** `Program.cs` throws
explicitly. A silently origin-less policy looks like a broken frontend in production and
invites somebody to "fix" it with `AllowAnyOrigin`.

**Mail falls back to the log.** `IEmailSender` resolves to `SmtpEmailSender` once both
`SmtpHost` and `FromAddress` are set (`EmailOptions.IsConfigured`), otherwise to
`LogOnlyEmailSender`. Locally that means no mail credentials are needed and a real customer
cannot be mailed by accident — the reset link simply appears in the console.

**Changing `Jwt:Key` signs everybody out.** That is also how you revoke all sessions at once.

## Local setup

```bash
docker compose -f docker-compose.dev.yml up -d
dotnet user-secrets set "Jwt:Key" "<32+ random characters>" --project Planning.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=PlanningDb;Username=planning;Password=planning_dev" \
  --project Planning.Api
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
dotnet run --project Planning.Api
```

The dev database listens on `127.0.0.1` only, which is why those credentials sit literally in
`docker-compose.dev.yml`.

## Environments

`ASPNETCORE_ENVIRONMENT` drives three things:

| | Development | Test | Production |
|---|---|---|---|
| Swagger at `/swagger` | yes | yes | no |
| Migrations at boot | yes | **no** | yes |
| Seed data (`TestDataSeeder`) | no | yes | no |
| Exception detail in the 500 body | yes | no | no |

Starting Test:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Test"; dotnet run --project Planning.Api --no-launch-profile
```

## Frontend

`Planning.Web/nuxt.config.ts`, `runtimeConfig`:

| Key | Env var | Default | For |
|---|---|---|---|
| `apiBaseUrl` | `NUXT_API_BASE_URL` | `http://localhost:5264` | server-side (SSR) |
| `public.apiBaseUrl` | `NUXT_PUBLIC_API_BASE_URL` | `http://localhost:5264` | in the browser |

Those two differ in production: SSR talks straight to the API container
(`http://api:8080`), the browser goes through Caddy on the public domain. Client-side calls
need an absolute URL — with an empty base the request hits the Nuxt app itself.

Cookies (`stores/auth.ts`): `planning_access_token`, `planning_refresh_token`,
`planning_organization_id`, each 14 days, `sameSite: 'lax'`. Plus `planning_locale` for the
language choice. They are deliberately **not** httpOnly, which means any XSS is a full account
takeover.

## Production

Everything lives in `deploy/.env` on the VPS, built from `deploy/.env.example`. That file is
never committed and sits at `chmod 600`.

Extra keys that exist only there:

| Var | For |
|---|---|
| `APP_DOMAIN` | the single public domain; feeds Caddy, CORS and `Email__AppBaseUrl` |
| `ACME_EMAIL` | where Let's Encrypt sends expiry warnings |
| `GITHUB_REPOSITORY_OWNER` | owner of the ghcr.io images |
| `IMAGE_TAG` | the commit SHA currently running; rolling back means changing this line |
| `POSTGRES_USER` / `PASSWORD` / `DB` | database |
| `RESTIC_PASSWORD` / `RESTIC_REPOSITORY` | encrypted off-site backup |
| `BACKUP_RETENTION_DAYS` | local dumps in `/opt/planning/backups` (7) |

**`RESTIC_PASSWORD` must also live somewhere off this server.** Without that passphrase the
backups cannot be restored — and a server you have lost takes it with it.

## GitHub secrets

For `.github/workflows/deploy.yml`:

| Secret | For |
|---|---|
| `VPS_HOST`, `VPS_USER` | where to |
| `VPS_SSH_KEY` | private deploy key |
| `VPS_SSH_HOST_KEY` | pinned host key — a swapped-out server fails the deploy instead of silently being accepted as a new identity |

`GITHUB_TOKEN` is built in and used as a short-lived registry login, so the VPS never stores a
registry credential.
