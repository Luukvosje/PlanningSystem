# CLAUDE.md

Guidance for Claude Code (claude.ai/code) when working in this repository.

## Project

Planning is a multi-tenant planning SaaS for small businesses: create a week's planning,
assign people to customers, confirm shifts, handle availability and absence requests. One
codebase, per-organization modules. .NET 10 backend in Clean Architecture (no CQRS) plus a
Nuxt 4 frontend.

Product context — who this is for, what is deliberately out of scope, and the current
quarter's goal — lives in `my-company.md`, included at the bottom of this file. Read it
before making a scope decision.

## Repository layout

```
Planning.Api/             Controllers, middleware, JWT, authorization policies, Swagger
Planning.Application/     DTOs, application services, FluentValidation validators, Result<T>
Planning.Domain/          Entities, enums, repository interfaces (no EF Core, no framework refs)
Planning.Infrastructure/  EF Core, repository implementations, PostgreSQL, SMTP, BCrypt
Planning.Tests/           xUnit + NSubstitute, unit tests only
Planning.Web/             Nuxt 4 / Vue 3 frontend (pnpm)
deploy/                   Production docker-compose, Caddyfile, backup script, VPS runbook
docs/                     Documentation — see docs/README.md
```

`Planning.slnx` at the root spans the five .NET projects. `Planning.Web` is not in the
solution; it has its own toolchain.

## Commands

### Backend (.NET 10, from the repo root)

```bash
dotnet restore Planning.slnx
dotnet build Planning.slnx
dotnet run --project Planning.Api          # http://localhost:5264, Swagger at /swagger

dotnet test Planning.Tests                 # the whole suite; no external dependencies
dotnet test Planning.Tests --filter FullyQualifiedName~PlanningServiceTests
dotnet test Planning.Tests --filter FullyQualifiedName~PlanningServiceTests.GetById_hides_a_record_from_another_organization
```

`dotnet test Planning.slnx` and `dotnet test Planning.Tests` are the same thing today —
`Planning.Tests` is the only test project, and it needs no database and no configuration.

Local secrets come from `dotnet user-secrets` (`Planning.Api` owns the secret store):

```bash
dotnet user-secrets set "Jwt:Key" "<32+ random chars>" --project Planning.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=PlanningDb;Username=planning;Password=planning_dev" --project Planning.Api
```

### Database (PostgreSQL)

```bash
docker compose -f docker-compose.dev.yml up -d     # local dev database on 127.0.0.1:5432
dotnet ef migrations add <Name> --project Planning.Infrastructure --startup-project Planning.Api
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
```

**The API migrates itself on boot** in every environment except `Test` (`Program.cs`). A
migration merged to `main` is applied by the next deploy without anyone running a command —
so a destructive migration ships silently. Ask before adding one that drops or retypes a
column, and before running `database update` against anything but the local database.

### Frontend (`Planning.Web`, pnpm 10.33.0 / Node 22)

```bash
pnpm install                     # from Planning.Web
pnpm dev                         # http://localhost:3000
pnpm lint                        # eslint .
pnpm lint:fix
pnpm test                        # vitest run
pnpm exec vitest run test/timelineMath.spec.ts
pnpm build
pnpm generate:api                # orval, against a running API's swagger.json
```

`pnpm generate:api` reads `http://localhost:5264/swagger/v1/swagger.json` (override with
`OPENAPI_URL`) and rewrites `app/generated/**`. Start the API first, and never hand-edit
anything under `app/generated/`.

### CI and deployment

`.github/workflows/ci.yml` runs on every PR and on pushes to `test`: backend build + test,
frontend lint + test + build. `.github/workflows/deploy.yml` runs the same checks on a push
to `main` and then ships images to ghcr.io and onto the VPS. **`main` is production.** The
first-deploy runbook is [docs/runbooks/first-deploy.md](docs/runbooks/first-deploy.md).

## Architecture

```
Planning.Api → Planning.Application → Planning.Domain ← Planning.Infrastructure
```

`Planning.Infrastructure` also references `Planning.Application`, because it implements the
ports declared there (`IEmailSender`, `IPasswordHasher`, `IOrganizationLogoStorage`). That is
the only extra edge. `Planning.Domain` references nothing.

- **Application services, not CQRS.** One service per feature area
  (`PlanningService`, `CustomerService`, …), registered in
  `Planning.Application/DependencyInjection.cs`. No MediatR, no commands/queries, no event
  sourcing — do not introduce them.
- **Repository interfaces live in Domain** (`I*Repository`), implementations in
  Infrastructure next to their `*Configuration.cs`. Repositories call `SaveChangesAsync`
  themselves; there is no unit-of-work.
- **Controllers are thin**: validate → one service call → `Result`/`Result<T>` translated to
  an `IActionResult` by `Planning.Api/Extensions/ResultExtensions.cs`.
- **Modules** (`AppModule.Planning` / `Klant` / `Beheer`) are enabled per organization and
  per user. `ModuleAuthorizationHandler` resolves them for the `Require*Module` policies;
  `Planning.Web/app/utils/modules.ts` mirrors the route side.
- **Frontend state**: TanStack Vue Query for server state (`composables/queries/*`), Pinia
  for the session only (`stores/auth.ts`), all HTTP through the orval-generated client
  behind `composables/api/use*Api.ts`.

## Documentation

`docs/` is organised by the kind of question it answers — see [docs/README.md](docs/README.md).

| Map | Beantwoordt |
|---|---|
| [docs/decisions/](docs/decisions/) | waarom is dit zo — genomen keuzes |
| [docs/architecture/](docs/architecture/) | hoe zit het in elkaar — tenancy, modules, productie |
| [docs/guidelines/](docs/guidelines/) | hoe schrijf ik hier code |
| [docs/references/](docs/references/) | configuratie, policies, foutcodes |
| [docs/runbooks/](docs/runbooks/) | live zetten, deployen, terugrollen |
| [docs/help/](docs/help/) | voor de klant, Nederlands |
| [docs/plans/](docs/plans/) | werk dat nog niet bestaat |

- **Decisions go in [docs/decisions/](docs/decisions/)** — one file per decision, one row in
  its README. If you are about to write "we besloten om…" anywhere else, write it there
  instead. **Do not work around a decision; revise it.** That index is injected at every
  session start by a `SessionStart` hook in `.claude/settings.json`, so keep it short.
- When the code overtakes a decision, add a `## Stand van zaken (datum)` section rather than
  rewriting what was decided.
- Open questions that are not yet decided stay in their plan document under
  [docs/plans/](docs/plans/) until there is a choice, then move to `docs/decisions/`.
- Documentation is Dutch where it is about the product or the business, English where it is
  about the code. `docs/help/` is always Dutch. Code, commits, identifiers and log messages
  are always English.

## Conventions

The detailed, code-anchored conventions live in two documents. They are **not** included
here — together they are ~30 KB and a frontend session has no use for the backend half.
Load the one you need:

| Working on | Skill | Document |
|---|---|---|
| `Planning.Api` / `Application` / `Domain` / `Infrastructure` / `Tests` | `backend-conventions` | [docs/guidelines/api.md](docs/guidelines/api.md) |
| `Planning.Web` | `frontend-conventions` | [docs/guidelines/frontend.md](docs/guidelines/frontend.md) |
| Visual language, page anatomy, shared components | `frontend-conventions` | [docs/guidelines/design.md](docs/guidelines/design.md) |

Read the relevant one **before** writing code in that area.

There are also task skills for procedures the documents do not spell out step by step:
`tenant-endpoint`, `db-schema-change`, `form-card`, `query-slice`. Prefer one of those over
re-deriving the procedure.

### The non-negotiables, in short

These are the rules most often got wrong, so they stay here rather than behind a skill.

**Multi-tenancy — the #1 correctness and security concern**

Every tenant-owned entity derives from `TenantEntity`. Repositories fetch by id **without**
filtering on organization, so the application service is the only enforcement point. Any new
query or mutation on a tenant entity must:

1. Resolve the organization from `ICurrentUserContext` (server-trusted: JWT claim +
   `OrganizationContextMiddleware`) — **never** from a client-supplied `organizationId`.
   `TenantServiceBase.TryGetOrganizationId` is the way.
2. After loading by id, check `Owns(entity)` before returning or mutating. Return
   `NOT_FOUND`, not `FORBIDDEN` — confirming another tenant's id exists is already a leak.
3. Validate every foreign reference (customer id, assigned user id) against the same
   organization before persisting.

A query that skips the ownership check is a bug, not a style nit.

**Backend**

- **`Result` / `Result<T>` for expected failures, exceptions for invariants.** Services
  return `Result.Failure(message, errorCode)`; domain entities throw `ArgumentException`,
  which `TenantServiceBase.TranslateDomainErrorsAsync` converts to `VALIDATION_ERROR`.
  Anything else is genuinely unexpected and belongs to `GlobalExceptionMiddleware`.
- **Error codes are `SCREAMING_SNAKE_CASE`** and must exist in the `switch` in
  `ResultExtensions.MapFailure`, or they fall through to a 400. Prefer the constants on
  `Failures`.
- **Every request DTO gets a FluentValidation validator**, run by
  `ApiControllerBase.ValidateAndExecuteAsync`. No hand-rolled null/range checks in a
  controller.
- **Every endpoint declares its policies explicitly** — a module policy on the controller
  (`RequirePlanningModule` / `RequireKlantModule` / `RequireBeheerModule`) and an action
  policy on anything mutating (`CanManagePlanning` / `RequireOwnerOrAdmin`). A new mutating
  endpoint without one is a red flag; ask before shipping it.
- **Async all the way, `CancellationToken` threaded through** every layer. Controllers pass
  `HttpContext.RequestAborted`.
- Business rules live in the domain entity (private setters, static `Create`, methods like
  `Update`/`Move`/`Confirm`). Do not re-implement an invariant check in the service.

**Frontend**

- **All HTTP goes through the orval client** (`app/generated/**`) wrapped in
  `composables/api/use*Api.ts`. No `$fetch` or `useFetch` against the API anywhere else.
- **Editable entity fields live in a form inside a card**, declared once in
  `composables/edit/use*Edit.ts` and rendered by `FormEditableSection`. No
  read-only-then-click-Edit step, no edit modal. Fields the API cannot update stay read-only
  (`FormDisplay`). Create flows use `composables/create/use*Create.ts` + a modal.
- **A `useForm(...)` definition never lives in a `.vue` file** — it goes in
  `composables/forms/`, `composables/edit/` or `composables/create/`.
- **Nothing is deleted without asking first.** The one-liner from
  `composables/useDeleteConfirm.ts`, or a component's own `UiConfirmModal`. A bare
  `await api.delete(id)` behind a trash icon is a bug.
- **`strict: true` and `@typescript-eslint/no-explicit-any: 'error'`.** Fix the type; never
  route around it with `any` or `as any`.
- **Every user-facing string through `$t()` / `t()`**, with entries in both
  `i18n/locales/nl.json` and `i18n/locales/en.json`. `nl` is the default locale.
- Session state lives in `stores/auth.ts` with `useCookie`. Do not add a second, parallel
  auth mechanism (localStorage, a plugin, a second store) — extend the store.
- Respect `eslint.config.mjs` rather than reformatting around it: tabs in Vue templates,
  single quotes, always-multiline trailing commas, one attribute per line. Run the linter
  instead of guessing.

**Security — apply to every change, not just "security work"**

- Tenant isolation, as above. This is how a bug becomes a data leak here.
- **Secrets never land in `appsettings*.json` or any committed file.** `dotnet user-secrets`
  locally, environment variables in production (`Jwt__Key`, `ConnectionStrings__DefaultConnection`,
  `Cors__AllowedOrigins__0`). Document the variable name, never the value.
- **Auth tokens sit in JS-readable cookies** (`useCookie`, not httpOnly) — any XSS is a full
  account takeover. Treat `v-html`, `innerHTML` and dynamically built URLs carrying user
  content as high severity.
- **Validate on both sides.** The zod schemas in `app/schemas/` are UX; the FluentValidation
  validator on the API is the security boundary.
- **EF Core queries stay parameterized** (LINQ / DbContext APIs). No string-concatenated SQL.
- **Never log secrets, tokens or PII-bearing bodies.** `GlobalExceptionMiddleware` only
  leaks exception detail in Development — keep it that way.
- Do not weaken CORS, JWT validation or the exception middleware to make local debugging
  easier. If you do it temporarily, revert before committing.

## Scope

These guidelines cover the five .NET projects and `Planning.Web`. `docs/prd.md` describes
the product; `my-company.md` describes what is and is not in the current quarter. Anything
outside the four MVP points in `my-company.md` is **not now** — say so instead of building it.

## Verification

Do not run tests, lint, typecheck or builds by default. Decide per change:

**Tests** — run the narrowest relevant test file when you changed logic, control flow or a
public interface. Never the full suite unless asked, or unless you touched something shared.

**Lint / format** — only on files you actually edited, never repo-wide. Do not fix
pre-existing violations in a file you touched for another reason; mention them instead.

**Build** — when you changed a signature, a DI registration, a project reference or a
package. A backend change that compiles is the minimum bar for "done".

**Always skip all of the above** for docs, comments, markdown, config and renames.

Beyond that: something is done when it has been built and run, not when it "should work".
If part of the task is not finished, say which part and why.

## Bedrijfscontext

@my-company.md
