# Backend conventions

For the five .NET projects. Read this before writing C# here. `CLAUDE.md` holds the
non-negotiables; this document holds the detail and the file paths.

Where this document and the code disagree, the code that most recently touched the same area
wins — but say so instead of silently picking one, and fix the loser.

## The shape of a feature

Every feature is the same seven files, one folder per project, folder named after the
feature. Customers is the smallest complete example:

```
Planning.Domain/Customers/Customer.cs                    entity, invariants
Planning.Domain/Customers/ICustomerRepository.cs         repository interface
Planning.Application/Customers/CustomerDtos.cs           request + response records
Planning.Application/Customers/ICustomerService.cs       service interface
Planning.Application/Customers/CustomerService.cs        orchestration + tenant checks
Planning.Application/Customers/CustomerValidators.cs     FluentValidation
Planning.Application/Customers/CustomerMapper.cs         entity → response
Planning.Infrastructure/Customers/CustomerRepository.cs      EF Core implementation
Planning.Infrastructure/Customers/CustomerConfiguration.cs   IEntityTypeConfiguration
Planning.Api/Controllers/CustomersController.cs          thin controller
```

Plus two registrations: the service in `Planning.Application/DependencyInjection.cs`, the
repository in `Planning.Infrastructure/DependencyInjection.cs`. Validators are picked up
automatically (`AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>`).

Do not invent an eighth file type. No `*Handler`, no `*Command`, no `*Facade`, no generic
repository base.

## Planning.Domain

References nothing — not EF Core, not ASP.NET, not `Microsoft.Extensions.*`. If you need an
attribute from a framework to express something, you are in the wrong project; express it in
`*Configuration.cs` instead.

**Entities** derive from `BaseEntity` (`Id`, `CreatedAtUtc`, `UpdatedAtUtc`, `Touch`) or from
`TenantEntity`, which adds `OrganizationId`. Anything owned by an organization derives from
`TenantEntity` — that is what makes it visible to the tenant rules below.

The shape, from `Planning.Domain/Planning/PlanningRecord.cs`:

- **All setters private**, a private parameterless constructor for EF Core, a private full
  constructor, and a static `Create(...)` factory.
- **Invariants are enforced in the entity**, by private `Validate*` methods that throw
  `ArgumentException`. `Create` and every mutator (`Update`, `Move`, `Confirm`,
  `ChangeStatus`) call them.
- **The clock is a parameter.** Every method that changes state takes `DateTime utcNow` and
  calls `Touch(utcNow)`. No `DateTime.UtcNow` inside the domain — that is what makes the
  tests deterministic.
- **Domain constants live on the entity**: `PlanningRecord.DefaultColor`,
  `PlanningRecord.MinimumDuration`. Not in the service, not in the validator.
- Derived state is a computed property (`IsOpenShift => AssignedUserId is null`), not a
  stored column.

**Repository interfaces** are `I<Entity>Repository` in the same folder. They are per-entity
and deliberately narrow: `GetByIdAsync`, `GetByOrganizationIdAsync`, `AddAsync`,
`UpdateAsync`, `DeleteAsync`, plus whatever that feature actually needs. Every method takes
`CancellationToken cancellationToken = default`.

**`GetByIdAsync` does not filter on organization.** That is deliberate and it is the single
most important thing to know about this codebase: the ownership check lives in the
application service, so a repository method that quietly filtered would hide a missing check
rather than fix one. See "Multi-tenancy" below.

**Enums** live in `Planning.Domain/Enums/`, one per file, and are serialized by name
(`JsonStringEnumConverter` in `Program.cs`) — so renaming an enum member is a breaking API
change and a frontend change.

## Planning.Application

### Services

One service per feature, `I<Feature>Service` + `<Feature>Service`. A service that touches
tenant-owned data derives from `TenantServiceBase` and takes `ICurrentUserContext` in its
constructor.

A service method:

1. Resolves the organization (`TryGetOrganizationId`) or loads the entity by id.
2. Checks ownership (`Owns(entity)`).
3. Validates foreign references against the same organization.
4. Calls the domain method inside `TranslateDomainErrorsAsync`.
5. Calls the repository.
6. Maps to a response DTO and returns `Result<T>.Success(...)`.

It does **not** re-check invariants the domain already enforces, and it does **not** throw
for expected failures.

### Result

`Planning.Application/Common/Result.cs`. `Result` for "no payload", `Result<T>` for one.
`Result.Failure(message, errorCode)` for everything the caller could reasonably hit.

Error codes are `SCREAMING_SNAKE_CASE`. The canonical ones are constants on `Failures`
(`Planning.Application/Common/TenantServiceBase.cs`): `NO_ORGANIZATION`, `NOT_FOUND`,
`FORBIDDEN`, `VALIDATION_ERROR`. Prefer the helpers (`Failures.NotFoundFor<T>("Customer")`)
over writing the string.

A new error code must be added to the `switch` in
`Planning.Api/Extensions/ResultExtensions.cs` — one that is not there falls through to a
400, which is almost never what you meant. The codes that map to something other than 400:
`NOT_FOUND` → 404, `UNAUTHORIZED` → 401, `CONFLICT` → 409, `FORBIDDEN` → 403,
`MODULE_DISABLED` → 403, `EXPIRED` → 410.

Failure messages are **English, static strings**. The backend has no i18n: the frontend
translates them by exact match in `Planning.Web/app/utils/backendMessages.ts`. So a message
with interpolated data cannot be translated — either keep it static, or accept that it stays
English. Change an existing message and you silently break its translation; update
`backendMessages.ts` in the same commit.

### DTOs

`sealed record` with positional parameters, in `<Feature>Dtos.cs`. Requests are
`Create<X>Request` / `Update<X>Request`, responses `<X>Response`.

When create and update validate the same fields, declare an `I<X>RequestFields` interface,
have both records implement it, and write one generic validator — see
`CustomerDtos.cs` + `CustomerValidators.cs`. Two hand-copied validators drift; `ValidatorParityTests`
exists because they already did once.

Nullability matters beyond C#: `Program.cs` turns C# nullable annotations into OpenAPI
`required`, which orval turns into TypeScript types. A `string?` where you meant `string`
becomes `string | null` in the frontend and every caller has to handle it.

### Validators

FluentValidation, one per request record, in `<Feature>Validators.cs`. They cover shape:
required, length, range, format, ordering. They do **not** cover "does this id belong to my
organization" — that is a database question and belongs in the service.

FluentValidation's *default* messages are localized by the backend (`UseRequestLocalization`
in `Program.cs`, driven by `Accept-Language`). Custom `.WithMessage(...)` text is not — it
needs an entry in `backendMessages.ts` like any other backend string.

### Mappers

`internal static class <Feature>Mapper` with `ToResponse(entity)`. No AutoMapper.

### Ports

Interfaces the Application layer needs but cannot implement live in
`Planning.Application/Common/`: `IEmailSender`, `IPasswordHasher`, `IJwtTokenService`,
`IOrganizationLogoStorage`, `ICurrentUserContext`. Infrastructure and Api implement them.
This is why `Planning.Infrastructure` references `Planning.Application`.

## Multi-tenancy

The rule in full, because this is where a bug becomes a data leak.

**The organization is server-derived, always.** `OrganizationContextMiddleware` takes the
`accountId` claim from the JWT, resolves it to a `User` membership — honouring the
`X-Organization-Id` header only when that membership exists and is active — and adds
`userId`, `organizationId` and a role claim. `CurrentUserContext` reads those claims.
Nothing else is trusted. An `organizationId` in a request body is data, not authorization.

An account with several active memberships and no header gets **no** organization context
rather than an arbitrary one; the client chooses via `/api/organizations/mine`.

**The service is the enforcement point.** The pattern, from `PlanningService`:

```csharp
var record = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

if (record is null || !Owns(record))
{
    return Failures.NotFoundFor<PlanningResponse>("Planning record");
}
```

Note the two things that are load-bearing:

- `null` and "belongs to someone else" produce the **same** answer. Returning `FORBIDDEN`
  for the second confirms the id exists, which is already a leak.
- The check happens after every load by id, on reads as well as writes. A `GET` that skips
  it is exactly as bad as a `DELETE` that does.

**Foreign references are checked too.** Before persisting a planning record, the customer id
and assigned user id are each loaded and checked against the same organization
(`PlanningService.ValidateReferencesAsync`). Without that, a caller can attach their shift to
another tenant's customer.

**List queries filter by organization in the repository** —
`GetByOrganizationIdAsync(organizationId, ...)` — with the id coming from
`TryGetOrganizationId`, never from the request.

When you add an entity or an endpoint, replicate this. `Planning.Tests/Application/PlanningServiceTests.cs`
shows the shape of a test that holds it in place; a new tenant entity deserves the same three
tests (hidden on read, refused on delete, refused on mutate).

## Planning.Infrastructure

### Repositories

Constructor-inject `ApplicationDbContext`. Implement the Domain interface, nothing more.

- **Reads that return a list use `.AsNoTracking()`** and order explicitly. Reads that feed a
  mutation (`GetByIdAsync`) stay tracked.
- **Repositories call `SaveChangesAsync` themselves.** There is no unit of work and no
  `IUnitOfWork` — do not add one without discussing it. The consequence is that a service
  method doing two writes is not atomic; if you need atomicity, say so rather than
  quietly relying on it.
- Queries stay LINQ. No raw SQL, no string concatenation with user input.

### Entity configurations

`<Entity>Configuration.cs` next to the repository, implementing `IEntityTypeConfiguration<T>`.
Explicit table name, key, `IsRequired()`, `HasMaxLength()`, an index on `OrganizationId` for
tenant entities, and the FK to `Organization` with `OnDelete(DeleteBehavior.Cascade)`.

They are discovered automatically by `ApplicationDbContext`; there is nothing to register.

Two global conventions live in `Planning.Infrastructure/Data/`: `UtcDateTimeConverter`
(everything is `DateTimeKind.Utc` going in and coming out) and `JsonColumnConverter` (for
the value objects stored as JSON, e.g. `OrganizationPlanningDefaults`). Times are UTC
everywhere in the backend — the frontend converts for display, never the API.

### Migrations

```bash
dotnet ef migrations add <Name> --project Planning.Infrastructure --startup-project Planning.Api
```

There is currently one migration (`20260901071944_Init`, PostgreSQL). The provider is Npgsql;
`EnableRetryOnFailure` is on, which means a `DbContext` operation can be retried — do not
rely on a transaction spanning multiple `SaveChangesAsync` calls without an explicit
execution strategy.

**The API applies pending migrations on boot** (`Program.cs`, every environment except
`Test`). So:

- A migration merged to `main` ships on the next deploy with no manual step.
- A destructive migration (drop column, retype, narrow a length) destroys production data
  silently. Ask first — always.
- It is safe only because exactly one API instance runs. A second instance races this and it
  must become a one-shot job first.

### Email

`IEmailSender` resolves to `SmtpEmailSender` when `Email:SmtpHost` is configured and to
`LogOnlyEmailSender` otherwise. Locally that means mail is written to the console, link and
all — no credentials needed and no way to accidentally mail a real customer. `Email:AppBaseUrl`
still has to be right or the links in those mails point nowhere.

## Planning.Api

### Controllers

Derive from `ApiControllerBase` (in `ResultExtensions.cs`), which provides
`ValidateAndExecuteAsync`. Constructor-inject the service and the `IValidator<T>` for each
request type.

```csharp
[ApiController]
[Route("api/customers")]
[Authorize(Policy = "RequireKlantModule")]
public class CustomersController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CanManagePlanning")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> Create([FromBody] CreateCustomerRequest request) =>
        ValidateAndExecuteAsync(request, _createValidator, async () =>
        {
            var result = await _customerService.CreateAsync(request, HttpContext.RequestAborted);
            return result.ToCreatedActionResult(this, nameof(GetById), value => new { id = value!.Id });
        });
}
```

- A mutating action returns `Task<IActionResult>` from `ValidateAndExecuteAsync`; a read
  method is a plain `async Task<IActionResult>` that calls the service and `.ToActionResult(this)`.
- `HttpContext.RequestAborted` is the cancellation token. Always pass it.
- `[ProducesResponseType]` for every status the action can actually return — this is what
  orval turns into the frontend's types, so a missing one means a wrong TypeScript type.
- Route parameters are constrained: `{id:guid}`.
- No business logic, no `DbContext`, no repository, no `try/catch`.

### Authorization

Policies are declared in `Program.cs`:

| Policy | Meaning |
|---|---|
| `RequirePlanningModule` / `RequireKlantModule` / `RequireBeheerModule` | the organization *and* the user have that module enabled (`ModuleAuthorizationHandler`) |
| `CanManagePlanning` | role Owner, Admin or Planner |
| `RequireOwnerOrAdmin` | role Owner or Admin |

**Every controller declares a module policy at class level**, and **every mutating action
declares an action policy**. Multiple `[Authorize]` attributes stack as AND — see
`UsersController`, which requires `RequireOwnerOrAdmin` *and* `RequireBeheerModule` on role
changes.

`AuthController` is the exception: register, login, refresh, forgot-password and
reset-password are anonymous by necessity and rate-limited instead (`Auth` and
`PasswordReset` policies in `Program.cs`). A new anonymous endpoint needs a rate limit and a
sentence explaining why it is anonymous.

An endpoint that any authenticated member may reach (e.g. `GET /api/users`) is a deliberate
choice, not the default. State it.

### Middleware order

`UseForwardedHeaders` → `GlobalExceptionMiddleware` → Swagger (dev/test only) →
`UseRequestLocalization` → `UseRateLimiter` → `UseCors` → static files →
`UseAuthentication` → `OrganizationContextMiddleware` → `UseAuthorization` → controllers.

`OrganizationContextMiddleware` sits between authentication and authorization because the
module policies read the claims it adds. Moving it breaks every `Require*Module` policy
without an obvious error.

`GlobalExceptionMiddleware` returns `INTERNAL_ERROR` plus a trace id, and the exception
message **only in Development**. Keep that.

### Health and configuration

`/health/live` answers "is the process up" (no database) and is what the container
healthcheck polls; `/health/ready` includes the database and is what a deploy waits for. Do
not merge them — a database blip must not restart a healthy API.

Configuration that must come from the environment in production:
`Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Cors__AllowedOrigins__0`,
`Email__*`. An empty CORS origin list is a fatal startup error on purpose — a permissive
fallback would look like a broken frontend and invite someone to "fix" it with
`AllowAnyOrigin`.

**No secret ever goes in `appsettings*.json`.** Locally: `dotnet user-secrets --project Planning.Api`.

## Style

- `net10.0`, `Nullable` and `ImplicitUsings` enabled in all five projects.
- File-scoped namespaces, one public type per file (validators that share a base are the
  exception).
- Constructor injection with `private readonly` fields; no service locator, no
  `IServiceProvider` injection.
- Async all the way: `Async` suffix, `Task`/`Task<T>`, `CancellationToken` as the last
  parameter with `= default` on interfaces. No `.Result`, no `.Wait()`, no `Task.Run`.
- Comments explain **why**, never what. The existing ones are a good calibration: they exist
  where a reader would otherwise "fix" something deliberate.
- `<summary>` XML docs on anything whose contract is not obvious from the name — especially
  the tenant helpers.

## Tests

`Planning.Tests`, xUnit + NSubstitute. No database, no configuration, no web host; the whole
suite runs anywhere.

Two folders: `Domain/` for invariants, `Application/` for service behaviour with substituted
repositories.

Naming is a readable sentence in snake_case:
`GetById_hides_a_record_from_another_organization`.

Conventions worth copying from `PlanningServiceTests`:

- Fixed `Guid`s as `static readonly` fields (`OwnOrganizationId`, `OtherOrganizationId`) so
  a failure message says which tenant.
- A fixed `Now`, never `DateTime.UtcNow`.
- A `CreateService()` helper and a `Record(...)` builder instead of repeated setup.

Per `CLAUDE.md` we do not write tests by default. The exceptions, where a test earns its
keep: **a tenant-isolation rule**, **a domain invariant**, and **a pure calculation**
(overlap detection, availability expansion). Those are cheap to write, catch the expensive
class of bug, and are why `ValidatorParityTests` exists.

## Open questions

Places where the codebase is inconsistent. Pick an existing variant, do not invent a third,
and settle it in `docs/decisions/` rather than in this list.

- **Error payload shape.** `ResultExtensions.MapFailure` writes anonymous objects
  (`new { error, errorCode }`) while `Planning.Api/Models/ApiErrorResponse.cs` exists and is
  what the controllers advertise in `[ProducesResponseType]`. They happen to serialize the
  same, so the generated client is right by luck. Using the record would make it true by
  construction.
- **`Failures` constants vs. string literals.** The `switch` in `MapFailure` matches on
  `"NOT_FOUND"` etc. rather than `Failures.NotFound`, so a renamed constant would not break
  the build.
- **`CustomerConfiguration.cs` formatting.** That file has a blank line between every line of
  code; nothing else in the repo does. Cosmetic, but fix it the next time you touch it.
