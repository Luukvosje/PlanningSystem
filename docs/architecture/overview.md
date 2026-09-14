# System overview

What the system *is*. How you write code in it lives in [guidelines/](../guidelines/); this
document describes the parts, their dependencies, and the path one request takes.

## Projects and dependencies

```
Planning.Api ──────────► Planning.Application ──────────► Planning.Domain
     │                            ▲                              ▲
     └──────────────────► Planning.Infrastructure ───────────────┘
                                  │
                                  └──► implements the ports declared in Application
```

| Project | Responsible for | May depend on |
|---|---|---|
| `Planning.Domain` | entities, enums, invariants, repository interfaces | **nothing** |
| `Planning.Application` | DTOs, application services, validators, `Result<T>`, ports | Domain |
| `Planning.Infrastructure` | EF Core, repository implementations, PostgreSQL, SMTP, BCrypt, logo storage | Domain, Application |
| `Planning.Api` | controllers, middleware, JWT, policies, Swagger | Application, Infrastructure |
| `Planning.Tests` | xUnit + NSubstitute, no external dependencies | Domain, Application |
| `Planning.Web` | Nuxt 4 frontend | the API through the generated client |

The extra arrow from Infrastructure to Application is deliberate: that is where the ports live
(`IEmailSender`, `IPasswordHasher`, `IOrganizationLogoStorage`, `ICurrentUserContext`) which
Infrastructure and Api implement. It is the only permitted extra edge.

**No CQRS, no MediatR, no event sourcing.** One service per feature, registered in
`Planning.Application/DependencyInjection.cs`. That is an explicit choice, not an omission.

## The path of one request

An employee opens the week planning:

1. **Browser → Nuxt.** `middleware/auth.global.ts` validates the session against
   `/api/auth/me` (with a silent refresh) before any routing decision; then
   `module.global.ts` checks whether the route belongs to a module this user has.
2. **Query composable.** `usePlanningRange()` asks TanStack Vue Query for the data, with a key
   from `utils/queryKeys.ts` and `enabled` guarded on session state.
3. **`apiClient.ts`.** Sets the base URL, the `Authorization: Bearer` token, the
   `X-Organization-Id` header and `Accept-Language`. On a 401 it refreshes once and retries —
   with deduplication, because two concurrent refreshes would spend the same single-use
   refresh token.
4. **Caddy → API container.** In production everything is one origin: `/api/*` and `/img/*` go
   to the API, the rest to Nuxt. That is why production has no CORS preflight at all.
5. **Middleware chain.** `UseForwardedHeaders` → `GlobalExceptionMiddleware` → localisation →
   rate limiter → CORS → static files → `UseAuthentication` → `OrganizationContextMiddleware`
   → `UseAuthorization` → controller.
6. **Controller.** Validates the request with FluentValidation and calls one service method.
7. **Application service.** Resolves the organization from `ICurrentUserContext`, loads the
   entity, checks ownership, calls the domain method, persists through the repository and maps
   to a response DTO. See [multi-tenancy.md](multi-tenancy.md).
8. **Repository → PostgreSQL.** LINQ over `ApplicationDbContext`; the repository calls
   `SaveChangesAsync` itself.
9. **Back up.** `ResultExtensions` turns `Result<T>` into an `IActionResult`; the error code
   decides the status.

## Identity and tenant

Short version: the JWT carries only `accountId`. *Which* organization you are is decided per
request by `OrganizationContextMiddleware`, based on your memberships. The full story is in
[multi-tenancy.md](multi-tenancy.md) — the most important page in this folder.

## Modules

Planning, Klant and Beheer are enabled per organization *and* per user. Backend and frontend
each own half of that gate; see [modules.md](modules.md).

## Production topology

Four containers on one VPS, `docker compose` in `/opt/planning`:

```
internet ──443──► caddy ──┬── /api/*, /img/*  ──► api  :8080 ──► db :5432
                          └── everything else ──► web  :3000
```

- **Only Caddy publishes ports** (80, 443, 443/udp). API, frontend and database talk over the
  internal network and are unreachable from outside — which is also why the API may serve
  plain HTTP and skip its own https redirect.
- **Nuxt runs server-side**, unlike a static build: SSR talks to `http://api:8080`, the browser
  to `https://<domain>`.
- **The API migrates itself at startup.** Safe because exactly one API instance runs; a second
  would race it.
- **Images are tagged by commit SHA**, not only `latest`. That is what makes a rollback one
  line in `.env` — see [runbooks/deploy-and-rollback.md](../runbooks/deploy-and-rollback.md).
- **Persistent data**: `db-data` (PostgreSQL), `logo-data` (uploaded organization logos — the
  only user data outside the database) and `caddy-data` (the issued certificates; losing it
  means re-requesting them and hitting Let's Encrypt rate limits).

## Environments

| | Development | Test | Production |
|---|---|---|---|
| Database | local Postgres via `docker-compose.dev.yml` | same | container `db` |
| Migrations | manual `dotnet ef database update` | **not** automatic | automatic at boot |
| Seed data | no | yes, `TestDataSeeder` | no |
| Swagger | yes | yes | no |
| Email | to the console (`LogOnlyEmailSender`) | same | SMTP |
| Exception detail in responses | yes | no | no |
| Secrets from | `dotnet user-secrets` | user-secrets | environment variables |
