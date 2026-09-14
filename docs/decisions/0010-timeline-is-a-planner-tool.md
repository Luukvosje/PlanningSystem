# 0010 — The timeline is a planner tool; an employee sees only their own week

- **Date:** 2026-08-22
- **Status:** final for the MVP
- **Touches:** `Planning.Web/app/utils/modules.ts`, `middleware/module.global.ts`

## Decision

`/timeline` — the organization-wide planning — requires a planner role. Out of the menu for
employees *and* blocked in `module.global.ts`, with a redirect to `/planning`. The employee
keeps their own week overview.

Open shifts can therefore only be filled by a planner; the "employee claims an open shift" flow
stays postponed.

## Why

An employee has no reason to see when colleagues work. Removing only the menu item is security
by menu — the URL stayed open. On top of that `/timeline` was not in `ROUTE_MODULE_MAP` at all,
so the page was reachable even *without* the Planning module. That was corrected at the same
time.

## Alternative considered

Showing open shifts in the personal week overview with "talk to your planner". Kept for later;
until then that conversation runs through the shared week planning (WhatsApp).

## Why the claim flow waits

It costs a new endpoint, the first mutation right for employees, a race rule *and*
notifications (SMTP depends on hosting). Only worthwhile once a paying customer asks for it.

## Where this stands (2026-09-14)

Built. `PLANNER_ONLY_ROUTE_PREFIXES` contains `/timeline`, and `/timeline` sits in
`ROUTE_MODULE_MAP` under the Planning module.
