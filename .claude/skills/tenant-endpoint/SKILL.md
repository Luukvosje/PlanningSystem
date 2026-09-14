---
name: tenant-endpoint
description: Add or change an API endpoint that touches tenant-owned data, end to end — domain method, repository, application service with the ownership check, FluentValidation validator, controller with module and action policies, and regenerating the frontend client. Use whenever a new route appears under /api, a service method is added, or an existing endpoint starts reading or writing a different entity.
---

# Adding a tenant-scoped endpoint

Work in this order. Backend first, frontend after — the generated client is derived from the
API, so doing it the other way means generating twice.

Read [docs/guidelines/api.md](../../../docs/guidelines/api.md) first if you have not this
session. `Customers` is the smallest complete example of every file below.

## 1. Domain

Only if the operation changes state in a way the entity does not already express.

- Add a method on the entity (`Update`, `Move`, `Confirm`, …) with private setters kept
  private, taking `DateTime utcNow` and calling `Touch(utcNow)`.
- Enforce invariants there, throwing `ArgumentException`. Do not put the check in the service.
- Add the repository method to `I<Entity>Repository` in `Planning.Domain/<Feature>/`, with
  `CancellationToken cancellationToken = default`.

`GetByIdAsync` must **not** filter on organization. The ownership check belongs in step 3.

## 2. Infrastructure

- Implement the repository method in `Planning.Infrastructure/<Feature>/<Entity>Repository.cs`.
- List reads: `.AsNoTracking()` + explicit `OrderBy` + `.Where(x => x.OrganizationId == organizationId)`.
- The repository calls `SaveChangesAsync` itself.
- New column or table? Stop and use the `db-schema-change` skill instead.

## 3. Application — this is where tenancy is enforced

Add the request/response records to `<Feature>Dtos.cs`, the method to `I<Feature>Service`,
and the implementation to `<Feature>Service` (which derives from `TenantServiceBase`).

For a **create**:

```csharp
if (!TryGetOrganizationId(out var organizationId))
{
    return Failures.NoOrganizationContext<TResponse>();
}
```

For anything that **loads by id**:

```csharp
var entity = await _repository.GetByIdAsync(id, cancellationToken);

if (entity is null || !Owns(entity))
{
    return Failures.NotFoundFor<TResponse>("<Entity>");
}
```

`null` and "another tenant's" must give the **same** answer. `NOT_FOUND`, never `FORBIDDEN`.
This applies to reads as well as writes.

Then:

- Validate every foreign reference (customer id, assigned user id, …) against the same
  organization before persisting — see `PlanningService.ValidateReferencesAsync`.
- Wrap the domain call in `TranslateDomainErrorsAsync` so an `ArgumentException` becomes
  `VALIDATION_ERROR`.
- Map with `<Feature>Mapper.ToResponse(...)` and return `Result<T>.Success(...)`.
- Failure messages are static English strings. A new one needs an entry in
  `Planning.Web/app/utils/backendMessages.ts` to be translated.
- A new error code needs a case in `ResultExtensions.MapFailure`, or it falls through to 400.

## 4. Validator

Add a validator for each new request record in `<Feature>Validators.cs`. If create and update
validate the same fields, declare an `I<X>RequestFields` interface and write one generic
validator — do not copy it. Nothing to register; the assembly scan picks it up.

Validators cover shape only. "Does this id belong to my organization" is step 3's job.

## 5. Controller

```csharp
[HttpPost]
[Authorize(Policy = "CanManagePlanning")]
[ProducesResponseType(typeof(TResponse), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public Task<IActionResult> Create([FromBody] CreateXRequest request) =>
    ValidateAndExecuteAsync(request, _createValidator, async () =>
    {
        var result = await _service.CreateAsync(request, HttpContext.RequestAborted);
        return result.ToCreatedActionResult(this, nameof(GetById), value => new { id = value!.Id });
    });
```

Checklist:

- [ ] module policy on the **controller** (`RequirePlanningModule` / `RequireKlantModule` /
      `RequireBeheerModule`)
- [ ] action policy on every **mutating** action (`CanManagePlanning` / `RequireOwnerOrAdmin`)
- [ ] `[ProducesResponseType]` for every status the action can return — orval turns these into
      the frontend's types, so a missing one produces a wrong TypeScript type
- [ ] `HttpContext.RequestAborted` passed as the cancellation token
- [ ] `{id:guid}` route constraint
- [ ] no logic, no repository, no `try/catch` in the controller

An endpoint reachable by any authenticated member is a deliberate choice. If that is what you
want, say so out loud; otherwise it is a bug.

## 6. Register

New service → `Planning.Application/DependencyInjection.cs`.
New repository → `Planning.Infrastructure/DependencyInjection.cs`.

## 7. Build and test

```bash
dotnet build Planning.slnx
```

Write a test only where it earns its keep — a tenant-isolation rule or a domain invariant.
`Planning.Tests/Application/PlanningServiceTests.cs` shows the shape: fixed GUIDs for own vs.
other organization, a fixed `Now`, substituted repositories.

## 8. Frontend

```bash
dotnet run --project Planning.Api      # in one terminal
cd Planning.Web && pnpm generate:api   # rewrites app/generated/**
```

Then wire it up per the `query-slice` skill: a method on `composables/api/use*Api.ts`, a key
in `utils/queryKeys.ts`, a `useQuery` in `composables/queries/`, and invalidation wherever the
mutation succeeds. If the endpoint is a new page under a module-gated area, add the route
prefix to `ROUTE_MODULE_MAP` in `utils/modules.ts` — hiding the menu item leaves the URL open.
