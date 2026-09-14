# Claude as a team member — Planning SaaS

This document does **not** replace `CLAUDE.md` (that stays the technical authority for coding
sessions). This is the wider frame: how Claude thinks along on dev **and** marketing, so that
every session — code, copy or strategy — starts from the same context and principles.

---

## 1. Product core (not to be changed without an explicit conversation)

- **Who:** small companies currently planning with Word, WhatsApp and Excel. First customer: a
  chain of gyms.
- **Promise:** build and share a complete week's planning in under five minutes.
- **Non-goals (deliberate):** payroll, contract management, leave, expenses, invoicing, HR
  files, AI planning, time tracking, clock in/out, certificates. Every feature suggestion —
  from either side — is tested against this: does it contribute to *planning faster, fewer
  mistakes, better overview, less WhatsApp*? If not, it does not belong in the MVP. Full
  version: [decision 0001](decisions/0001-product-scope-and-non-goals.md).
- **Possible future:** growth into a broader business SaaS (the `Modules` system in the code is
  already prepared for it), plus a separate employee app. That is a direction, not a promise —
  not to be anticipated in code or marketing copy until you say so explicitly.
- **Revenue model:** hybrid — planner seats are the core price, employee seats free up to a
  limit, plus Starter/Groei/Premium as an all-in option. Amounts still open. See
  [decision 0006](decisions/0006-pricing-model.md).

## 2. How Claude handles dev

- Follow `CLAUDE.md` in the repo root for every technical rule (architecture, multi-tenancy,
  security checklist, conventions). That document wins on questions of code style.
- Large features (schema changes, new modules) get planned in plain language first — what, why,
  what it touches, open questions — before any code. Files change only after you agree.
- Direction (architecture, scope, approach) is discussed first; implementation details are
  decided on the spot.
- Nothing is pushed and no PR is opened unless you ask. Work happens on a feature branch, one
  commit per logical step.

> **Note (2026-09-14):** this section used to say Claude had no push access to the repository
> and could not build the .NET side locally. Neither is true in Claude Code — the repository is
> read and written directly and `dotnet build` runs here. "It should compile" is therefore not
> an acceptable answer; the build is run and the output shown.

## 3. How Claude handles marketing

Still open (fill in once known):

- Brand name and domain name settled?
- Tone: businesslike and sober, or deliberately informal (fitting a gym manager with no
  appetite for IT jargon)?
- Main competitors and alternatives (besides Word, WhatsApp and Excel themselves) to position
  against in copy?

### Channel: Meta Ads

The decision and its reasoning are in
[decision 0005](decisions/0005-marketing-channel-meta-ads.md): narrowly targeted Meta Ads,
€10–20 a day, video over static text, lookalike audiences only once there are one or two paying
customers.

**Note:** that estimate was made without live web access — advertising costs and policies may
have moved. Check current CPM and targeting options at Meta before scaling.

### Pricing

The model and its reasoning are in [decision 0006](decisions/0006-pricing-model.md): hybrid,
planner seats as the core price, employees free up to a limit, packages alongside, no credits
for the planning itself.

That used to be written out here *and* in `my-company.md` *and* in the PRD, with three
different outcomes. One source now — extend it there, not here.

What still has to happen on your side before the price is fixed is listed under "Still open" in
that decision. The one item Claude cannot fill in for you: test with a few prospects how €19–29
lands. With B2B tools, underpricing is more often the problem than overpricing.

## 4. Decision log

See [`docs/decisions/`](decisions/) for the full, current log — the central place for every
decision taken (product, technical, marketing). New decisions go there, not here, so that two
documents cannot drift apart.

## 5. How a session runs

- A new conversation about this project starts with "we are working on PlanningSystem"; Claude
  then reads this file, `CLAUDE.md` and the PRD before starting. The decisions index arrives
  automatically through the `SessionStart` hook.
- Large scope questions ("should we build this at all?") are tested explicitly against section
  1 above, not against what is technically fun or easy.
- A short, concrete plan with one or two open questions beats a long document full of
  assumptions — you decide, Claude supplies the options and a recommendation.
