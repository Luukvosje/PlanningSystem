# Authorization

Roles, policies and what each endpoint requires. The mechanism behind it is in
[architecture/modules.md](../architecture/modules.md) and
[architecture/multi-tenancy.md](../architecture/multi-tenancy.md).

## Roles

`Planning.Domain/Enums/UserRole.cs`. The role hangs off the **membership** (`User`), not the
account — the same person can be Owner at one company and Employee at another.

| Role | Meant for |
|---|---|
| `Owner` | the owner; cannot be deactivated or have their role changed through the UI |
| `Admin` | manages organization, users and modules |
| `Planner` | creates and changes the planning, manages customers |
| `Employee` | sees their own week, submits requests |

The frontend equivalents live in `Planning.Web/app/utils/userRole.ts`
(`canManageOrganization`, `canManagePlanning`, `canManageCustomers`, `canEditUserRole`,
`canEditUserStatus`). Use those, never a loose role comparison in a component.

## Policies

Declared in `Planning.Api/Program.cs`.

| Policy | Succeeds when |
|---|---|
| `RequireOwnerOrAdmin` | role is Owner or Admin |
| `CanManagePlanning` | role is Owner, Admin or Planner |
| `RequirePlanningModule` | organization *and* user have the Planning module |
| `RequireKlantModule` | same for Klant |
| `RequireBeheerModule` | same for Beheer (always on at organization level) |

Multiple `[Authorize]` attributes on one action stack as **AND**.

## Endpoints

The module policy sits on the controller, the action policy on the action.

### `api/auth` — no module policy

| Method | Route | Requires |
|---|---|---|
| POST | `/register` | anonymous, rate limit `Auth` |
| POST | `/login` | anonymous, rate limit `Auth` |
| POST | `/refresh` | anonymous |
| POST | `/forgot-password` | anonymous, rate limit `PasswordReset` |
| POST | `/reset-password` | anonymous, rate limit `PasswordReset` |
| GET | `/me` | signed in |
| PUT | `/me` | signed in |

### `api/organizations` — `[Authorize]`

| Method | Route | Requires |
|---|---|---|
| POST | `/` | signed in |
| GET | `/current`, `/mine`, `/{id}` | signed in |
| PUT | `/current` | `RequireOwnerOrAdmin` |
| PUT | `/current/planning-settings` | `RequireOwnerOrAdmin` |
| PUT | `/current/modules` | `RequireOwnerOrAdmin` |
| GET | `/current/logo` | signed in |
| POST | `/current/logo` | `RequireOwnerOrAdmin` |

### `api/users` — `[Authorize]`

| Method | Route | Requires |
|---|---|---|
| GET | `/`, `/{id}` | signed in |
| PUT | `/{id}/role` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| PUT | `/{id}/status` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| PUT | `/{id}/approval` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| PUT | `/{id}/modules` | `RequireOwnerOrAdmin` |

### `api/invites` — `[Authorize]`

| Method | Route | Requires |
|---|---|---|
| POST | `/` | `RequireOwnerOrAdmin` + `RequireBeheerModule` |
| POST | `/accept` | signed in |
| GET | `/{code}` | signed in |

### `api/customers` — `RequireKlantModule`

| Method | Route | Requires |
|---|---|---|
| GET | `/`, `/{id}` | module |
| POST, PUT, DELETE | | `CanManagePlanning` |

### `api/planning` — `RequirePlanningModule`

| Method | Route | Requires |
|---|---|---|
| GET | `/`, `/{id}` | module |
| POST, PUT, DELETE | | `CanManagePlanning` |
| PATCH | `/{id}/move`, `/{id}/confirm` | `CanManagePlanning` |
| POST | `/{id}/duplicate` | `CanManagePlanning` |

### `api/availability` — `RequirePlanningModule`

| Method | Route | Requires |
|---|---|---|
| GET | `/rules`, `/rules/for-planning` | module |
| POST, PUT, DELETE | `/rules`, `/rules/{id}` | module — no action policy |

### `api/requests` — `RequirePlanningModule`

| Method | Route | Requires |
|---|---|---|
| GET | `/` | module |
| POST | `/approve`, `/reject` | `CanManagePlanning` |

## Worth knowing

**`api/availability` is the only controller whose mutations carry no action policy.** That is
deliberate: an employee manages their own availability, so `CanManagePlanning` would be exactly
wrong. The boundary sits in the service instead, checking whether you are touching your own
rules or somebody else's:

```csharp
if (existing.EmployeeId != CurrentUser.UserId && !CanManageAnyEmployee())
{
    return Failures.ForbiddenFor<AvailabilityRuleResponse>("You are not allowed to update this rule.");
}
```

If you change anything there, read `AvailabilityRuleService` first.

**`GET /api/users` is reachable by any signed-in member** of the organization. It has to be,
because the planning shows colleagues' names. That is a choice, not an omission.

## Rate limiting

Only on the anonymous auth endpoints, per IP (`Program.cs`):

| Policy | Limit | Why |
|---|---|---|
| `PasswordReset` | 5 per 15 minutes | sending mail costs money and lands in somebody else's inbox |
| `Auth` | 20 per 5 minutes | credential stuffing on login, account enumeration on register |

The real client IP is correct because `UseForwardedHeaders` runs before the rate limiter.

A new anonymous endpoint needs a rate limit *and* a sentence explaining why it is anonymous.

## JWT

`JwtTokenService` puts `sub`, `accountId` and `email` in the token — **not** the organization.
Which organization you are in this request is decided by `OrganizationContextMiddleware`, which
adds `userId`, `organizationId` and the role as claims. See
[architecture/multi-tenancy.md](../architecture/multi-tenancy.md).

Lifetimes: `Jwt:ExpirationMinutes` (60), refresh token `Jwt:RefreshTokenExpirationDays` (14).
Refresh tokens rotate and are single-use.
