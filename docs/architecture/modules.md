# Modules

One codebase, with parts that can be switched on per organization. This is the mechanism that
lets a customer request become *configuration* instead of a customer-specific branch in the
code — see [my-company.md](../../my-company.md), "What to guard".

## The three modules

| Module | Contains | Toggleable per organization? |
|---|---|---|
| `Planning` | planning records, timeline, availability, requests | yes |
| `Klant` | customer management | yes |
| `Beheer` | organization, users, invites, roles, module assignment | **no, always on** |

`AppModule` lives in `Planning.Domain/Modules/AppModule.cs`. The enum is serialized by name, so
renaming a member is a breaking change for both the API and the frontend.

## Two levels, and how they combine

A module is switched on in two places:

- **`OrganizationModule`** — has this organization bought the module?
- **`UserModule`** — may this employee use it?

The rules live in exactly one place, `Planning.Application/Modules/ModulePermissions.cs`, and
there are only three:

```csharp
IsAdminRole(role)                       => role is Owner or Admin
GetOrganizationEffectiveEnabled(m, org) => m is Beheer || org
HasEffectiveAccess(role, org, user)     => org && (IsAdminRole(role) || user)
```

From which it follows:

- **Beheer is always on at organization level.** Otherwise an organization could lock itself
  out of the very screen that switches modules on.
- **The organization is a hard gate.** If the module is off there, nobody has access — not even
  an Owner.
- **Owner and Admin always have access inside that gate**, regardless of their `UserModule`.
  That is why the module switches on the user page are shown fixed rather than hidden for an
  admin: visible but `disabled`, with a tooltip.

## Where it is enforced

**Backend — `ModuleAuthorizationHandler`.** The policies `RequirePlanningModule`,
`RequireKlantModule` and `RequireBeheerModule` (declared in `Program.cs`) each carry a
`ModuleRequirement`. The handler calls `IModuleService.HasEffectiveModuleAsync`, which fetches
the organization and user modules and runs them through `ModulePermissions`.

Every controller declares its module policy at class level. Multiple `[Authorize]` attributes
stack as AND — `UsersController` requires both `RequireOwnerOrAdmin` and `RequireBeheerModule`
on a role change.

**Frontend — `ROUTE_MODULE_MAP`.** In `Planning.Web/app/utils/modules.ts` sits a table from
route prefix to module:

| Prefix | Module |
|---|---|
| `/planning`, `/timeline`, `/beschikbaarheid` | Planning |
| `/customers` | Klant |
| `/users`, `/organization` | Beheer |

`middleware/module.global.ts` blocks a route that does not pass. Alongside it sits
`PLANNER_ONLY_ROUTE_PREFIXES` for routes that need a planner role on top of the module — see
[decision 0010](../decisions/0010-timeline-is-a-planner-tool.md).

**A new page in a gated area must be added to `ROUTE_MODULE_MAP`.** Removing the menu item is
security by menu; the URL stays open. That is exactly how `/timeline` was once reachable
without the Planning module at all.

## Both halves have to agree

The backend decides who may get in; the frontend decides what you are shown. They share no
code. Adding a module therefore always means touching both sides:

1. extend `AppModule`, plus a migration for existing organizations
2. a `Require<X>Module` policy in `Program.cs`
3. that policy on the controllers involved
4. `ALL_MODULES` and `ROUTE_MODULE_MAP` in `utils/modules.ts`
5. labels in `i18n/locales/nl.json` **and** `en.json`
6. `pnpm generate:api`, because the enum changed

Miss step 4 and the page is reachable while the API returns 403 — visibly broken. Miss step 3
and the API is open while the menu looks tidy — invisibly broken. The second one is the
dangerous one.
