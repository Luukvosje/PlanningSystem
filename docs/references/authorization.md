# Autorisatie

Rollen, policies en welk endpoint wat eist. De achterliggende mechaniek staat in
[architecture/modules.md](../architecture/modules.md) en
[architecture/multi-tenancy.md](../architecture/multi-tenancy.md).

## Rollen

`Planning.Domain/Enums/UserRole.cs`. De rol hangt aan het **lidmaatschap** (`User`), niet aan
het account — dezelfde persoon kan Owner zijn bij het ene bedrijf en Employee bij het andere.

| Rol | Bedoeld voor |
|---|---|
| `Owner` | de eigenaar; kan niet worden gedeactiveerd of van rol veranderd via de UI |
| `Admin` | beheert organisatie, gebruikers en modules |
| `Planner` | maakt en wijzigt de planning, beheert klanten |
| `Employee` | ziet zijn eigen week, dient verzoeken in |

Frontend-equivalenten staan in `Planning.Web/app/utils/userRole.ts`
(`canManageOrganization`, `canManagePlanning`, `canManageCustomers`, `canEditUserRole`,
`canEditUserStatus`). Gebruik die, nooit een losse rolvergelijking in een component.

## Policies

Gedeclareerd in `Planning.Api/Program.cs`.

| Policy | Slaagt wanneer |
|---|---|
| `RequireOwnerOrAdmin` | rol is Owner of Admin |
| `CanManagePlanning` | rol is Owner, Admin of Planner |
| `RequirePlanningModule` | organisatie én gebruiker hebben de Planning-module |
| `RequireKlantModule` | idem voor Klant |
| `RequireBeheerModule` | idem voor Beheer (organisatieniveau staat altijd aan) |

Meerdere `[Authorize]`-attributen op één actie stapelen als **EN**.

## Endpoints

Module-policy staat op de controller, actie-policy op de actie.

### `api/auth` — geen module-policy

| Methode | Route | Eist |
|---|---|---|
| POST | `/register` | anoniem, rate limit `Auth` |
| POST | `/login` | anoniem, rate limit `Auth` |
| POST | `/refresh` | anoniem |
| POST | `/forgot-password` | anoniem, rate limit `PasswordReset` |
| POST | `/reset-password` | anoniem, rate limit `PasswordReset` |
| GET | `/me` | ingelogd |
| PUT | `/me` | ingelogd |

### `api/organizations` — `[Authorize]`

| Methode | Route | Eist |
|---|---|---|
| POST | `/` | ingelogd |
| GET | `/current`, `/mine`, `/{id}` | ingelogd |
| PUT | `/current` | `RequireOwnerOrAdmin` |
| PUT | `/current/planning-settings` | `RequireOwnerOrAdmin` |
| PUT | `/current/modules` | `RequireOwnerOrAdmin` |
| GET | `/current/logo` | ingelogd |
| POST | `/current/logo` | `RequireOwnerOrAdmin` |

### `api/users` — `[Authorize]`

| Methode | Route | Eist |
|---|---|---|
| GET | `/`, `/{id}` | ingelogd |
| PUT | `/{id}/role` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| PUT | `/{id}/status` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| PUT | `/{id}/approval` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| PUT | `/{id}/modules` | `RequireOwnerOrAdmin` |

### `api/invites` — `[Authorize]`

| Methode | Route | Eist |
|---|---|---|
| POST | `/` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| POST | `/accept` | ingelogd |
| GET | `/{code}` | ingelogd |

### `api/customers` — `RequireKlantModule`

| Methode | Route | Eist |
|---|---|---|
| GET | `/`, `/{id}` | module |
| POST, PUT, DELETE | | `CanManagePlanning` |

### `api/planning` — `RequirePlanningModule`

| Methode | Route | Eist |
|---|---|---|
| GET | `/`, `/{id}` | module |
| POST, PUT, DELETE | | `CanManagePlanning` |
| PATCH | `/{id}/move`, `/{id}/confirm` | `CanManagePlanning` |
| POST | `/{id}/duplicate` | `CanManagePlanning` |

### `api/availability` — `RequirePlanningModule`

| Methode | Route | Eist |
|---|---|---|
| GET | `/rules`, `/rules/for-planning` | module |
| POST, PUT, DELETE | `/rules`, `/rules/{id}` | module — géén actie-policy |

### `api/requests` — `RequirePlanningModule`

| Methode | Route | Eist |
|---|---|---|
| GET | `/` | module |
| POST | `/approve`, `/reject` | `CanManagePlanning` |

## Let op

**`api/availability` heeft als enige mutaties zonder actie-policy.** Dat is bewust: een
medewerker beheert zijn eigen beschikbaarheid, dus `CanManagePlanning` zou juist verkeerd
zijn. De grens ligt daar in de service, niet in een policy — die controleert of je aan je
eigen regels zit of over die van een ander. Verander je daar iets, kijk dan eerst in
`AvailabilityRuleService`.

**`GET /api/users` is bereikbaar voor elk ingelogd lid** van de organisatie. Dat is nodig
omdat de planning namen van collega's toont. Het is een keuze, geen omissie.

## Rate limiting

Alleen op de anonieme auth-endpoints, per IP (`Program.cs`):

| Policy | Limiet | Waarom |
|---|---|---|
| `PasswordReset` | 5 per 15 minuten | verstuurde mail kost geld en landt in andermans inbox |
| `Auth` | 20 per 5 minuten | credential stuffing op login, accountenumeratie op register |

Het echte client-IP klopt omdat `UseForwardedHeaders` vóór de rate limiter draait.

Een nieuw anoniem endpoint heeft een rate limit nodig én een zin die uitlegt waarom het
anoniem is.

## JWT

`JwtTokenService` zet `sub`, `accountId` en `email` in het token — **niet** de organisatie.
Welke organisatie je in dit request bent, bepaalt `OrganizationContextMiddleware`, die
`userId`, `organizationId` en de rol als claims toevoegt. Zie
[architecture/multi-tenancy.md](../architecture/multi-tenancy.md).

Levensduur: `Jwt:ExpirationMinutes` (60), refresh token `Jwt:RefreshTokenExpirationDays` (14).
Refresh tokens roteren en zijn eenmalig.
