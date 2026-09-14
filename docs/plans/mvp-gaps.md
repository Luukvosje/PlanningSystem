# MVP gaps — plan (no code, decisions only)

Status of the three points from [../prd.md](../prd.md), checked against the codebase.

| Feature | Status |
|---|---|
| Notes per shift | ✅ already built (domain, DTOs, frontend tooltip/sidebar) — only the UX still needs verifying |
| Open shifts | ✅ built 2026-08-22, now on `main` |
| Sharing a planning (text + WhatsApp) | ❌ not built |

---

## 1. Open shifts

**Decision:** `AssignedUserId` becomes nullable. No separate status field — a shift without an
employee *is* the open shift. The simplest option, and it fits "build no feature that does not
contribute to planning faster".

**What this touches:**

- **Domain** — `PlanningRecord`: `AssignedUserId` → `Guid?`, plus an `IsOpenShift` convenience
  property. `Create`/`Update`/`Move` accept `Guid?`.
- **Infrastructure** — EF configuration: make the column nullable → a new migration. The
  foreign key to `User` stays `Restrict`, which works as-is with a nullable FK.
- **Application** — DTOs (`CreatePlanningRequest`, `UpdatePlanningRequest`,
  `MovePlanningRequest`, `PlanningResponse`) → `AssignedUserId` nullable. Validators: no more
  `NotEmpty` check on `AssignedUserId`. Service: only validate the user reference when there is
  a value; overlap detection (`BuildOverlapLookup`) has to skip open shifts, which cannot
  overlap with themselves on a person who does not exist.
- **Frontend** — regenerate the orval models after the backend change; the timeline block shows
  an "open shift" state (dashed border, no employee colour); the new-shift form allows an empty
  employee field; the dashboard counter "X open shifts" (already in the PRD dashboard mockup)
  can be built on top.

**Deliberately not now:** the "employee accepts an open shift" flow (the PRD marks it *later*).
That is a separate action and endpoint, and belongs to a later phase rather than to making open
shifts possible at all.

**Decided 2026-08-22:** a dashed border in the outline style, the label "Open dienst" inside the
block, and an "Open diensten" row at the top of employee mode that you can drag onto to remove
somebody from a shift. See
[decision 0007](../decisions/0007-open-shifts-visual-design.md).

**Deviation from this plan:** the API sends `assignedUserName: null` rather than the literal
text "Open dienst" — the label comes from i18n, so the backend carries no UI text.

---

## 2. Sharing a planning (text + WhatsApp)

**Decision:** frontend only, no backend change needed — the week planning data is already
available through the existing query composables.

**Approach:**

1. **Text-format function** — turn the already-loaded week planning (per day, per employee,
   sorted by time) into the PRD format:
   ```
   Planning week 31

   Monday
   Kevin
   08:00 - 16:00

   Lisa
   16:00 - 22:00
   ```
   Open shifts named explicitly, which ties into point 1.
2. **Copy as text** — clipboard API, a button in the week header next to the existing actions.
3. **WhatsApp share** — `https://wa.me/?text=<url-encoded text>` in a new tab or deep link.
   Works on desktop (WhatsApp Web) and mobile (native app) without an extra dependency.
4. **PDF export** — explicitly *later* per the PRD, not part of this.

**Where in the code:** a new util in `app/utils/planning/` (for example
`formatWeekPlanningText.ts`) plus a component or button in `app/components/planning/header/`.

**Open question:** should the shared text be grouped per employee (as in one PRD example) or
per day with all employees underneath (as in the other)? Both examples are in the PRD, so this
needs a choice.

See [decision 0003](../decisions/0003-sharing-a-planning-frontend-only.md).

---

## 3. Notes — verification, nothing to build

Backend and frontend already exist. The only action is a short test together: where can you add
and see a note while planning, and is that clear enough without explanation, given the PRD
requirement that "a planner understands it within five minutes"? No code change expected unless
the test says otherwise.

See [decision 0004](../decisions/0004-shift-notes-no-build-needed.md).

---

## Suggested order

1. Open shifts (backend + frontend) — the largest change, and it unlocks the dashboard figure.
2. Sharing a planning — a small, standalone frontend feature; can run alongside or after.
3. The notes UX check — fifteen minutes together, no separate sprint needed.

## Where this stands (2026-09-14)

Points 1 and 3 are settled; only point 2 is left, and its open question about grouping is still
open. That makes this document almost finished: once sharing is built and its decision recorded,
it can go.
