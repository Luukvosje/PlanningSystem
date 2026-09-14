# My company — the core

The core of the business behind this product. Static at the top, the weekly focus at the
bottom. Update **Focus this week** every Monday and move the previous week into the log.

> Company name: _to be filled in_

## What we are

A **multi-tenant planning SaaS for small and medium businesses**, industry-independent: any
company that schedules people at customers. Built and run by one person (Luuk), **alongside a
job**, at roughly 5–10 hours a week.

## Why us

1. **Simpler than the rest.** Existing planning packages are too heavy and too expensive for
   small companies. Every feature has to keep that promise standing — complexity is not
   neutral here, it is the loss of the main reason to choose us.
2. **Personal contact and quick turnaround.** I know the customer and build requests fast.
   Large vendors do not.

Those two bite each other: building customer requests fast is exactly how a simple product
becomes a heavy one. See *What to guard* below.

## Product

One codebase, with modules that can be switched on per organization:

| Module   | Contains |
|----------|----------|
| Planning | create, move and confirm planning records, week overview |
| Klant    | customer management |
| Beheer   | organization, users, invites, roles, module assignment |

Stack: .NET 10 (Clean Architecture) + Nuxt/Vue 3. See `CLAUDE.md` for the rules.

## Phase and goal

- **Phase:** building, no customers yet.
- **Goal for the next 3 months:** get the product working well enough for the first customer.

"Working well enough" = all four done:
- [ ] Create and view a planning (create, move, confirm, week overview)
- [ ] Manage customers and employees (CRUD, invites, roles, modules)
- [ ] Employee availability, including conflict detection while scheduling
- [ ] Deployable with real data (hosting, login, backups)

Anything not on this list is **not now**.

## Revenue model

Hybrid: **planner seats are the core price**, employee seats free up to a limit and a small
amount each above it, with Starter/Groei/Premium alongside as the all-in option. No credits for
the planning itself — those create an incentive to use the tool less.

The amounts and the employee limit (10 or 15) are still open. To be decided before the first
pricing conversation with a customer — not earlier, but in time.

Full reasoning: [decision 0006](docs/decisions/0006-pricing-model.md).

## What to guard

- **Stay simple.** For every feature: does this make the product harder to explain?
- **Solve customer requests generically.** Per-organization configuration or a module first,
  bespoke work only after that. Hardcoded customer names or `if (organizationId == ...)` are a
  red flag.
- **Scope.** 5–10 hours a week. Every week not spent on the four MVP points above is a week of
  delay before the first customer.
- **Tenant isolation and secrets.** See the security checklist in `CLAUDE.md`.

## Marketing

Positioning, channels, budget, ads and results live in `marketing.md`. One file, updated
monthly.

---

# Focus this week

**Week of:** _to be filled in_

**Goal this week:**
- _to be filled in_

**Blockers:**
- _to be filled in_

**Deliberately not this week:**
- _to be filled in_

---

# Week log

_(completed weeks go here, newest first)_

## Week of 2026-08-17

**Goal that week:**
- Finish the availability refactor: conflict detection working through `availabilityMath.ts`
  and `usePlanningAvailabilityPeriods` (the old `AvailabilityOverlapChecker` was already gone).
- Frontend lint clean: `pnpm lint:fix`, then the remaining real errors by hand (including one
  `no-explicit-any`).
- Split the 170 uncommitted files into logical commits on a feature branch.

**Blockers:** none.

**Deliberately not:** hosting and deployment — moved to the following week.

**State of the code at the start of that week (2026-08-19):**
- Backend built clean (0 warnings, 0 errors).
- Last commit 2026-08-16; 170 changed files in the working tree.
- Data model complete for all four MVP points; last migration 2026-07-18.
- No Dockerfile, no CI, `.github` empty.

> Since then MVP point 4 has largely landed: PostgreSQL, two Dockerfiles, the production
> compose file with Caddy, a backup script, and CI plus deploy workflows — see
> [decision 0008](docs/decisions/0008-hosting-vps-docker-compose.md).
