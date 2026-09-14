# Configuratie

Elke instelling, waar hij vandaan komt en wat er gebeurt als hij ontbreekt.

**Geen enkel geheim staat in een `appsettings*.json`.** Lokaal komen ze uit
`dotnet user-secrets` (`Planning.Api` bezit de secret store), in productie uit environment
variables via `deploy/.env`. De .NET-scheidingstekens: `Jwt:Key` in een JSON-bestand is
`Jwt__Key` als environment variable.

## Backend

| Sleutel | Env var | Default | Ontbreekt → |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | — | **startfout** |
| `Jwt:Key` | `Jwt__Key` | — | **startfout** |
| `Jwt:Issuer` | `Jwt__Issuer` | `Planning.Api` | tokens valideren niet |
| `Jwt:Audience` | `Jwt__Audience` | `Planning.Api` | tokens valideren niet |
| `Jwt:ExpirationMinutes` | `Jwt__ExpirationMinutes` | 60 | — |
| `Jwt:RefreshTokenExpirationDays` | `Jwt__RefreshTokenExpirationDays` | 14 | — |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0`, `__1`, … | leeg | **startfout** |
| `Email:SmtpHost` | `Email__SmtpHost` | leeg | mail gaat naar de log |
| `Email:SmtpPort` | `Email__SmtpPort` | 587 | — |
| `Email:Username` / `Password` | `Email__Username` / `Email__Password` | leeg | — |
| `Email:FromAddress` | `Email__FromAddress` | leeg | mail gaat naar de log |
| `Email:FromName` | `Email__FromName` | `Planning` | — |
| `Email:AppBaseUrl` | `Email__AppBaseUrl` | leeg | links in mail wijzen nergens heen |

### Drie dingen die niet vanzelf spreken

**Een lege CORS-lijst is een fatale startfout, geen soepele default.** `Program.cs` gooit
expliciet. Een stilzwijgend origin-loze policy ziet er in productie uit als een kapotte
frontend en nodigt iemand uit om het te "repareren" met `AllowAnyOrigin`.

**Mail valt terug op de log.** `IEmailSender` wordt `SmtpEmailSender` zodra `SmtpHost` én
`FromAddress` gevuld zijn (`EmailOptions.IsConfigured`), anders `LogOnlyEmailSender`. Lokaal
heb je dus geen mailcredentials nodig, en je kunt geen echte klant per ongeluk mailen — de
resetlink staat gewoon in de console.

**`Jwt:Key` wijzigen logt iedereen uit.** Dat is tegelijk de manier om alle sessies in één
keer in te trekken.

## Lokaal opzetten

```bash
docker compose -f docker-compose.dev.yml up -d
dotnet user-secrets set "Jwt:Key" "<32+ willekeurige tekens>" --project Planning.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=PlanningDb;Username=planning;Password=planning_dev" \
  --project Planning.Api
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
dotnet run --project Planning.Api
```

De dev-database luistert alleen op `127.0.0.1`, daarom staan die credentials letterlijk in
`docker-compose.dev.yml`.

## Omgevingen

`ASPNETCORE_ENVIRONMENT` stuurt drie dingen aan:

| | Development | Test | Production |
|---|---|---|---|
| Swagger op `/swagger` | ja | ja | nee |
| Migraties bij boot | ja | **nee** | ja |
| Seed-data (`TestDataSeeder`) | nee | ja | nee |
| Exception-detail in de 500-body | ja | nee | nee |

Test starten:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Test"; dotnet run --project Planning.Api --no-launch-profile
```

## Frontend

`Planning.Web/nuxt.config.ts`, `runtimeConfig`:

| Sleutel | Env var | Default | Waarvoor |
|---|---|---|---|
| `apiBaseUrl` | `NUXT_API_BASE_URL` | `http://localhost:5264` | server-side (SSR) |
| `public.apiBaseUrl` | `NUXT_PUBLIC_API_BASE_URL` | `http://localhost:5264` | in de browser |

Die twee verschillen in productie: SSR praat rechtstreeks met de API-container
(`http://api:8080`), de browser gaat via Caddy over het publieke domein. Client-side calls
hebben een absolute URL nodig — met een lege base raakt het request de Nuxt-app zelf.

Cookies (`stores/auth.ts`): `planning_access_token`, `planning_refresh_token`,
`planning_organization_id`, elk 14 dagen, `sameSite: 'lax'`. Plus `planning_locale` voor de
taalkeuze. Ze zijn bewust **niet** httpOnly, wat betekent dat elke XSS een volledige
accountovername is.

## Productie

Alles staat in `deploy/.env` op de VPS, gebouwd uit `deploy/.env.example`. Dat bestand wordt
nooit gecommit en staat op `chmod 600`.

Extra sleutels die alleen daar bestaan:

| Var | Waarvoor |
|---|---|
| `APP_DOMAIN` | het enige publieke domein; vult Caddy, CORS en `Email__AppBaseUrl` |
| `ACME_EMAIL` | waar Let's Encrypt verloopwaarschuwingen heen stuurt |
| `GITHUB_REPOSITORY_OWNER` | eigenaar van de ghcr.io-images |
| `IMAGE_TAG` | de commit-SHA die nu draait; terugrollen is deze regel wijzigen |
| `POSTGRES_USER` / `PASSWORD` / `DB` | database |
| `RESTIC_PASSWORD` / `RESTIC_REPOSITORY` | versleutelde off-site back-up |
| `BACKUP_RETENTION_DAYS` | lokale dumps in `/opt/planning/backups` (7) |

**`RESTIC_PASSWORD` moet ook ergens buiten deze server liggen.** Zonder die passphrase zijn de
back-ups niet te herstellen — en een server die je kwijt bent neemt hem mee.

## GitHub-secrets

Voor `.github/workflows/deploy.yml`:

| Secret | Waarvoor |
|---|---|
| `VPS_HOST`, `VPS_USER` | waar heen |
| `VPS_SSH_KEY` | private deploy-sleutel |
| `VPS_SSH_HOST_KEY` | vastgepinde host key — een omgewisselde server laat de deploy falen in plaats van stilletjes een nieuwe identiteit te accepteren |

`GITHUB_TOKEN` is ingebouwd en wordt als kortlevende registry-login gebruikt, zodat de VPS
nooit een registry-credential opslaat.
