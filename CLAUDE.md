# Planning SaaS — Guidance for Claude Code

Multi-tenant Planning SaaS. .NET 10 (Clean Architecture) backend + Nuxt/Vue 3 frontend.
Read this before making changes. If a rule here conflicts with something you observe in
the code, the code that most recently touched the same area wins — but flag the conflict
instead of silently picking one.

## Architecture — don't break the layering

```
Planning.Api           Controllers, middleware, JWT, authorization policies
Planning.Application   DTOs, application services, FluentValidation validators, Result<T>
Planning.Domain        Entities, enums, repository interfaces (no EF Core, no framework refs)
Planning.Infrastructure EF Core, repository implementations, SQL Server
Planning.Web           Nuxt/Vue 3 frontend
```

Flow: `Api → Application → Domain ← Infrastructure`. Concretely:

- Domain must not reference Application, Infrastructure, or Api. No EF Core attributes on
  domain entities — mapping lives in `Planning.Infrastructure/*/`*Configuration.cs`.
- Repository **interfaces** live in Domain (`I*Repository`); **implementations** live in
  Infrastructure. Don't add a repository interface directly in Infrastructure.
- Controllers stay thin: validate → call one application service method → translate
  `Result`/`Result<T>` to an `IActionResult` via `ResultExtensions`. No business logic,
  no direct repository/DbContext access in a controller.
- Business rules and invariants belong in the Domain entity (private setters, static
  `Create(...)` factory, methods like `Update`/`Move`/`Confirm` that throw
  `ArgumentException` on violation — see `PlanningRecord`). Don't re-implement invariant
  checks in the Application service; call the domain method and translate the exception.
- Application services orchestrate: load entities, check tenant ownership, call domain
  methods, call repositories, map to response DTOs. No CQRS/MediatR/event sourcing —
  don't introduce them.

## Multi-tenancy is the #1 correctness/security concern

Every tenant-owned entity derives from `TenantEntity` (`OrganizationId`, `CreatedAtUtc`,
`UpdatedAtUtc`). Repositories fetch by id **without** filtering by organization, so the
application service is the enforcement point. Any new query/mutation on a tenant entity
must:

1. Resolve the organization from `ICurrentUserContext` (server-trusted, derived from the
   JWT claim + `OrganizationContextMiddleware`) — **never** from a client-supplied
   `organizationId` in the request body/query for authorization decisions.
2. After loading an entity by id, verify it belongs to the current organization before
   returning or mutating it (see `BelongsToCurrentOrganization` in `PlanningService`).
   Return `NOT_FOUND`, not `FORBIDDEN` — don't confirm existence of another tenant's data.
3. Validate any foreign reference (customer id, assigned user id, etc.) also belongs to
   the same organization before persisting.

When you add a new entity/endpoint, replicate this pattern. If you find a query that
skips the ownership check, treat it as a bug, not a style nit.

## Backend conventions

- **Result pattern, not exceptions, for expected failures.** Application services return
  `Result` / `Result<T>` (`Success`, `Failure(message, errorCode)`). Reserve thrown
  exceptions for domain invariant violations (caught as `ArgumentException` at the
  service boundary) and truly unexpected failures (caught by `GlobalExceptionMiddleware`).
- **FluentValidation** validates request DTOs. Every new request type gets a validator
  registered in DI; controllers run it via `ApiControllerBase.ValidateAndExecuteAsync`.
  Don't hand-roll null/range checks in the controller.
- **Authorization policies, not bare `[Authorize]`.** Endpoints declare a module policy
  (e.g. `RequirePlanningModule`) at the controller level and an action policy (e.g.
  `CanManagePlanning`) on mutating actions. A new mutating endpoint without an explicit
  policy is a red flag — ask before shipping it.
- Keep DTOs immutable records where the existing code already uses them; match the
  surrounding file's style rather than introducing a new pattern in one file.
- Async all the way through; always accept and pass `CancellationToken`.

## Frontend conventions (Planning.Web)

- API access goes through the **orval-generated client** (`app/generated/**`) wrapped by
  a `composables/api/use*Api.ts` composable, combined with TanStack Vue Query
  (`composables/queries/*`) for server state. This is the target pattern — prefer it for
  new code.
- `utils/planningClient.ts` is a legacy hand-written client with manual
  PascalCase/camelCase normalization (`normalizeRecord`). That dual-casing is a symptom
  of an API contract mismatch, not something to imitate — don't copy this pattern into
  new modules. If you touch planning API code, prefer migrating toward the generated
  client over extending the manual normalizer.
- TypeScript `strict: true`, and ESLint enforces `no-explicit-any: 'error'` — don't use
  `any` or `as any` to route around a type error; fix the type.
- Respect the existing ESLint config (`eslint.config.mjs`) instead of reformatting around
  it — tabs for Vue template indentation, single quotes, always-multiline trailing
  commas, one attribute per line in templates. Run the linter rather than guessing.
- Auth/session state lives in `stores/auth.ts` (Pinia) using `useCookie` for
  `planning_access_token` / `planning_refresh_token` / `planning_organization_id`. Route
  gating lives in `middleware/*.global.ts`. Don't add a second, parallel auth-state
  mechanism (e.g. localStorage) — extend the store.

## Security checklist (apply to every change, not just "security work")

- **Tenant isolation**: see the multi-tenancy section above — this is the most common way
  a bug becomes a data leak in this codebase.
- **Secrets never go in `appsettings.json`/`appsettings.*.json` or any file that's
  committed.** Use `dotnet user-secrets`, environment variables, or a secret manager for
  `Jwt:Key`, connection strings with credentials, and any API key. If you need a local
  value for development, put it in a git-ignored file and document the variable name in
  README, not the value.
- **Never log secrets, tokens, or full request/response bodies containing PII.**
  `GlobalExceptionMiddleware` already avoids leaking exception details outside
  Development — keep that behavior when touching error handling.
- **Auth tokens are stored in a JS-readable cookie (`useCookie`, not httpOnly)** — that
  means any XSS is a full account takeover. Treat unsanitized rendering of user-provided
  content (`v-html`, `innerHTML`, dynamically constructed URLs) as high severity. Prefer
  plain interpolation; if `v-html` is ever necessary, sanitize first and say so in review.
- **Validate on both sides.** Client-side validation (schemas in `app/schemas/*`) is UX
  only — the FluentValidation validator on the API is the actual security boundary. Don't
  skip the server-side validator because the client already checks it.
- **EF Core queries stay parameterized** (LINQ / `DbContext` APIs) — never build SQL via
  string concatenation with user input. If raw SQL is ever unavoidable, use parameters.
- **New endpoints need an explicit authorization policy** (module + action). Don't ship
  an endpoint that's reachable by any authenticated user unless that's genuinely intended
  and stated as such.
- **Don't weaken CORS, JWT validation, or the exception middleware "no detail leak in
  prod" behavior** to make local debugging easier, without reverting before commit.

## Workflow

- Build: `dotnet build` from the repo root or a specific `*.csproj`/`Planning.slnx`.
- Migrations: `dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api`.
  Ask before running this against anything but a local/dev database, and before adding a
  new migration that alters existing columns (data loss risk).
- Frontend dev server / lint / build run from `Planning.Web` via the existing npm scripts.
- Don't add new top-level abstractions (mediator, CQRS, generic repository base beyond
  what exists) without discussing it first — this codebase deliberately keeps that layer
  simple.

## Bedrijfscontext

De kern van het bedrijf, het doel voor dit kwartaal en de focus van deze week staan in
`my-company.md`. Lees het mee voordat je scope-keuzes maakt.

@my-company.md
