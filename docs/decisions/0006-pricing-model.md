# 0006 — Pricing: hybrid seats plus packages

- **Date:** 2026-08-16
- **Status:** provisional — the model stands, the amounts do not
- **Touches:** revenue model, [prd.md](../prd.md), [collaboration.md](../collaboration.md),
  [my-company.md](../../my-company.md)

## Decision

A hybrid model. **Planner seats are the core price**; employee seats are free up to a limit and
a small amount each above it. Alongside that, the all-in package stays as a simple alternative
for anyone who does not want to puzzle over seats.

| Component | Direction |
|---|---|
| Planner seat — creates and changes rosters | the main price, this is where the value is |
| Employee seat — sees own roster, submits availability | free up to a limit (10–15), small amount each above it |
| All-in package Starter / Groei / Premium | stays as the all-in option |

The package rates as they stand today in the [PRD](../prd.md):

| Package | For | Price |
|---|---|---|
| Starter | ±15 employees | €19–29 per month |
| Groei | more employees, several planners | €49–69 per month |
| Premium | several locations, extended features | €99+ per month |

## Why

The value sits with the planner, not with the employee who only looks at their own roster. A
model that bills per employee punishes exactly the growth you want: a customer who hires five
more people is not using the tool five times as intensively.

## Alternative considered: credits

Credits per shift or per action. Rejected.

Credits suit sporadic use — AI image generation, for instance. With a planning tool you *want*
frequent use; credits per action create an incentive to use the tool *less*, directly against
the goal (less WhatsApp, more overview).

For later, optional extras credits are conceivable — not for the planning itself.

## Still open

None of these four is settled:

- Exact amounts per planner seat and per employee seat.
- At what number of employees the surcharge starts: 10 or 15?
- Do the package rates above stay as they are now that a seat model sits beside them, or do
  they move up?
- Test with a few prospects whether €19–29 reads as "too cheap to take seriously" or as
  "finally affordable". With B2B tools, underpricing is more often the problem than
  overpricing.

To be decided before the first pricing conversation with a customer — not earlier, but in time.

## Where this stands (2026-09-14)

This file is now the only place the revenue model is explained. `collaboration.md`,
`my-company.md` and `prd.md` point here instead of describing it again; all three used to say
something different, down to "not yet decided" in `my-company.md`.

The PRD keeps the package rates, since that is where they come from — but now says alongside
them that they are the package half of this model and not the whole story.
