# Systeemoverzicht

Wat het systeem *is*. Hoe je er code in schrijft staat in [guidelines/](../guidelines/); dit
document beschrijft de onderdelen, hun afhankelijkheden en de weg die één request aflegt.

## Projecten en afhankelijkheden

```
Planning.Api ──────────► Planning.Application ──────────► Planning.Domain
     │                            ▲                              ▲
     └──────────────────► Planning.Infrastructure ───────────────┘
                                  │
                                  └──► implementeert de ports uit Application
```

| Project | Verantwoordelijk voor | Mag afhangen van |
|---|---|---|
| `Planning.Domain` | entiteiten, enums, invarianten, repository-interfaces | **niets** |
| `Planning.Application` | DTOs, application services, validators, `Result<T>`, ports | Domain |
| `Planning.Infrastructure` | EF Core, repository-implementaties, PostgreSQL, SMTP, BCrypt, logo-opslag | Domain, Application |
| `Planning.Api` | controllers, middleware, JWT, policies, Swagger | Application, Infrastructure |
| `Planning.Tests` | xUnit + NSubstitute, geen externe afhankelijkheden | Domain, Application |
| `Planning.Web` | Nuxt 4 frontend | de API via de gegenereerde client |

De extra pijl van Infrastructure naar Application is bewust: daar staan de ports
(`IEmailSender`, `IPasswordHasher`, `IOrganizationLogoStorage`, `ICurrentUserContext`) die
Infrastructure en Api invullen. Dat is de enige toegestane extra rand.

**Geen CQRS, geen MediatR, geen event sourcing.** Eén service per feature, geregistreerd in
`Planning.Application/DependencyInjection.cs`. Dat is een expliciete keuze, geen omissie.

## De weg van één request

Een medewerker opent de weekplanning:

1. **Browser → Nuxt.** `middleware/auth.global.ts` valideert de sessie tegen `/api/auth/me`
   (met stille refresh) vóór elke routebeslissing, daarna gaat `module.global.ts` na of de
   route bij een module hoort die deze gebruiker heeft.
2. **Query-composable.** `usePlanningRange()` vraagt TanStack Vue Query om de data, met een
   key uit `utils/queryKeys.ts` en `enabled` op sessiestatus.
3. **`apiClient.ts`.** Zet de base URL, het `Authorization: Bearer`-token, de
   `X-Organization-Id`-header en `Accept-Language`. Bij een 401 doet hij één refresh en
   herhaalt hij het request — met deduplicatie, want twee gelijktijdige refreshes zouden
   hetzelfde eenmalige refresh-token uitgeven.
4. **Caddy → API-container.** In productie is alles één origin: `/api/*` en `/img/*` gaan
   naar de API, de rest naar Nuxt. Daarom is er in productie geen CORS-preflight.
5. **Middleware-keten.** `UseForwardedHeaders` → `GlobalExceptionMiddleware` →
   localisatie → rate limiter → CORS → statische bestanden → `UseAuthentication` →
   `OrganizationContextMiddleware` → `UseAuthorization` → controller.
6. **Controller.** Valideert het request met FluentValidation en roept één servicemethode aan.
7. **Application service.** Haalt de organisatie uit `ICurrentUserContext`, laadt de entiteit,
   controleert eigendom, roept de domeinmethode aan, slaat op via de repository en mapt naar
   een response-DTO. Zie [multi-tenancy.md](multi-tenancy.md).
8. **Repository → PostgreSQL.** LINQ over `ApplicationDbContext`; de repository roept zelf
   `SaveChangesAsync` aan.
9. **Terug omhoog.** `Result<T>` wordt door `ResultExtensions` een `IActionResult`; de
   foutcode bepaalt de statuscode.

## Identiteit en tenant

Kort: het JWT draagt alleen `accountId`. Wélke organisatie je bent, wordt per request bepaald
door `OrganizationContextMiddleware` op basis van je lidmaatschappen. Het volledige verhaal
staat in [multi-tenancy.md](multi-tenancy.md) — dat is de belangrijkste pagina in deze map.

## Modules

Planning, Klant en Beheer worden per organisatie én per gebruiker aangezet. Backend en
frontend hebben elk hun eigen helft van die poort; zie [modules.md](modules.md).

## Productie-topologie

Vier containers op één VPS, `docker compose` in `/opt/planning`:

```
internet ──443──► caddy ──┬── /api/*, /img/*  ──► api  :8080 ──► db :5432
                          └── al het overige  ──► web  :3000
```

- **Alleen Caddy publiceert poorten** (80, 443, 443/udp). API, frontend en database praten over
  het interne netwerk en zijn van buiten onbereikbaar — daarom mag de API platte HTTP serveren
  en zijn eigen https-redirect overslaan.
- **Nuxt draait server-side**, anders dan een statische build: SSR praat met `http://api:8080`,
  de browser met `https://<domein>`.
- **De API migreert zichzelf bij het opstarten.** Veilig omdat er precies één API-instantie
  draait; een tweede zou hierop racen.
- **Images zijn getagd op commit-SHA**, niet alleen `latest`. Daarom is terugrollen één regel
  in `.env` — zie [runbooks/deploy-en-rollback.md](../runbooks/deploy-en-rollback.md).
- **Persistente data**: `db-data` (PostgreSQL), `logo-data` (geüploade organisatielogo's — de
  enige gebruikersdata buiten de database) en `caddy-data` (de uitgegeven certificaten; kwijt
  betekent opnieuw aanvragen en tegen Let's Encrypt-limieten aanlopen).

## Omgevingen

| | Development | Test | Production |
|---|---|---|---|
| Database | lokale Postgres via `docker-compose.dev.yml` | idem | container `db` |
| Migraties | handmatig `dotnet ef database update` | **niet** automatisch | automatisch bij boot |
| Seed-data | nee | ja, `TestDataSeeder` | nee |
| Swagger | ja | ja | nee |
| E-mail | naar de console (`LogOnlyEmailSender`) | idem | SMTP |
| Foutdetails in responses | ja | nee | nee |
| Secrets uit | `dotnet user-secrets` | user-secrets | environment variables |
