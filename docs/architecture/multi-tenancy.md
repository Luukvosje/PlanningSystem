# Multi-tenancy

The most important page in this folder. One mistake here is not a bug but a data leak: one
customer sees another customer's planning.

## The model

An **Account** is a person with an email address and a password. A **User** is that account's
membership of one organization, with a role. One account can hold several memberships — that
is how somebody plans for two companies.

```
Account ──1:n──► User ──n:1──► Organization
 (signing in)  (role, active)    (the tenant)
```

Everything owned by an organization derives from `TenantEntity`
(`Planning.Domain/Common/TenantEntity.cs`): `OrganizationId`, `CreatedAtUtc`, `UpdatedAtUtc`.

## The chain: from token to organization

This is the part to understand before writing an endpoint.

**1. The JWT carries `accountId`, not the organization.**
`JwtTokenService` puts `sub`, `accountId` and `email` in the token. Deliberately no
organization: a token that pinned one would have to be reissued on every switch, and would
stay valid after the membership was revoked.

**2. `OrganizationContextMiddleware` decides per request which organization it is.**
It reads `accountId` from the token and resolves the membership:

- Is there an `X-Organization-Id` header? It is honoured **only** if a membership exists for
  that account in that organization **and** that membership is active. The header is a
  *request*, not an assertion.
- No usable header? The single active membership.
- Several active memberships and no header? Then **no** organization context. It deliberately
  does not guess: picking one would silently decide which tenant the caller reads and writes.
  That choice stays with the client, via `GET /api/organizations/mine`.

When it succeeds, `userId`, `organizationId` and the role are added as claims on the identity —
there, in this request, not in the token.

**3. `ICurrentUserContext` reads those claims.**
`CurrentUserContext` (in `Planning.Api/Services/`) is the only place the application layer
touches identity. `HasOrganization` is true once there is both an `organizationId` and a
`userId`.

**4. The middleware sits between authentication and authorization.**
That is not a detail: the `Require*Module` policies read the claims this middleware adds. Move
it and every module policy fails — without a clear error.

## Where the boundary is enforced

**Repositories do not filter on organization.** `GetByIdAsync(id)` returns the entity, whoever
owns it. That is on purpose: if the repository filtered quietly, a missing check in the service
would *work* instead of standing out, and nobody would ever learn the check is needed.

**The application service is the only enforcement point.** The pattern, from `PlanningService`:

```csharp
var record = await _planningRecordRepository.GetByIdAsync(id, cancellationToken);

if (record is null || !Owns(record))
{
    return Failures.NotFoundFor<PlanningResponse>("Planning record");
}
```

Two things carry the weight here:

1. **`null` and "belongs to another tenant" give the same answer.** `NOT_FOUND`, never
   `FORBIDDEN`. With `FORBIDDEN` you confirm the id exists, and that is already a leak.
2. **It applies to reads too.** A `GET` that skips the check is exactly as bad as a `DELETE`
   that does.

`TenantServiceBase` supplies the tools: `TryGetOrganizationId` (organization or
`NO_ORGANIZATION`), `Owns(entity)` and `TranslateDomainErrorsAsync`.

## Foreign references count as well

A planning record points at a customer and at an employee. Both ids come from the request and
are therefore attacker-chosen. `PlanningService.ValidateReferencesAsync` loads them and checks
them against the same organization before persisting. Without that step, somebody can attach
their own shift to another tenant's customer.

The same holds for every new reference you add.

## List queries

Those *do* filter in the repository — `GetByOrganizationIdAsync(organizationId, ...)` — with an
id that comes from `TryGetOrganizationId`, never from the request.

## How to test it

`Planning.Tests/Application/PlanningServiceTests.cs` is the model. Fixed GUIDs for
`OwnOrganizationId` and `OtherOrganizationId`, so a failing test says immediately which tenant
it was about. Three tests per tenant entity are enough:

- reading another organization's record gives `NOT_FOUND`
- deleting it gives `NOT_FOUND`
- mutating it gives `NOT_FOUND`

This is where we *do* write tests, despite the "no tests by default" rule. They are cheap and
they catch the most expensive class of mistake.

## The checklist for every new endpoint

- [ ] the organization comes from `ICurrentUserContext`, never from body or query
- [ ] an `Owns` check after every load-by-id
- [ ] `NOT_FOUND`, not `FORBIDDEN`
- [ ] every foreign reference checked against the same organization
- [ ] list queries filtered on `organizationId`
- [ ] module policy on the controller, action policy on every mutation

See the `tenant-endpoint` skill for the full procedure.
