# Availability — redesign (plan, no code)

Status: **draft, awaiting agreement.** No files changed.

This plan responds to a brainstorm idea: a unified pattern-plus-exception model, spread over
three places in the UI instead of one availability page. Below: what already exists, where the
idea is right, where it is a genuine architecture change rather than a UI change, and where it
departs from the PRD and MVP scope.

---

## 1. What already exists (findings, not assumptions)

At database level these are **not** two separate features. There is already one entity,
`AvailabilityRule` (`Planning.Domain/Availability/AvailabilityRule.cs`), with a `Type`
discriminator (`Weekly` / `OneTime`). Both types share the same table, repository and service.
It *feels* like two features because the UI renders them as two separate lists on one page
(`RulesEditor.vue` → a weekly section and a one-time section, each with its own "add" button),
not because the model is split.

More importantly, the way an exception combines with the pattern today is **pure union, not
override**:

- `AvailabilityRuleStatus` knows `Unavailable`, `Preferred` and `Available` — but the entity
  hard-blocks everything except `Unavailable` (`ValidateStatus`: *"Only unavailable rules are
  supported at this time."*), and the validator repeats that rule at application level.
- So today the model is a **blocklist**. Everyone is available by default; rules (weekly or
  one-time) only punch holes of *un*availability.
- `AvailabilityPeriodExpander.ExpandForDate` (Application layer) simply merges every matching
  Weekly and OneTime period for a given date into one list — there is no precedence logic. A
  OneTime rule does not "override" a Weekly rule; it is added on top.

Planner visibility while scheduling **already exists**, partly as the brainstorm proposes: a
background layer in the planning calendar (`showAvailability` toggle, default **off**, purely
visual — no click-to-edit, no "create on somebody's behalf" from the calendar). Point 3 of the
idea is therefore largely built, not new.

The employee has **no** combined calendar of roster plus their own exceptions. The "My
planning" view (`PlanningMyView.vue` → `PlanningMyDayStrip.vue` + `PlanningMyDayDetail.vue`)
shows shifts only; availability does not appear there at all. Point 2 of the idea *is* new work.

Backend authorization for "on somebody else's behalf" already exists for **both** rule types:
`CanManageForEmployeeAsync` in `AvailabilityRuleService` already lets a Planner, Admin or Owner
manage any employee's rules, not only their own. So that is not a new backend decision — only a
UI choice about where in the planner flow to offer it.

Finally, `RuleSheet.vue` (the create/edit form) is a completely separate, hand-written form —
not built on the generic `Form`/`useCreate`/`useEdit` stack the rest of the app has been
migrating to.

---

## 2. Where the idea is right — and where it is an architecture change, not a UI change

**Right, and mostly a UI rearrangement (low risk):**

- "One model" — already true at database level. The real work is making the UI *feel* that way.
- The planner background layer in the existing planning calendar — exists already, a small
  extension.
- Moving the base pattern to settings or the profile — purely a matter of giving the weekly
  section of `RulesEditor.vue` a different home; the model does not change.

**Conceptually right, but a genuine model change (higher risk), not "just UI":**

- "An exception always overrides the pattern, in both directions" — this requires:
  1. Unlocking `Status.Available` from the current hard block (changing `ValidateStatus`), so an
     exception can say "I *am* available this day" despite a weekly block.
  2. Rewriting `AvailabilityPeriodExpander` from "merge" to "apply the pattern, then lay OneTime
     rules over the overlapping time slots with precedence" — an interval replace/subtract
     algorithm, no longer a simple list.
  3. The same logic **also exists client-side** (`availabilityMath.ts`, to avoid an extra round
     trip for conflict checks while scheduling) — it would have to change in lockstep, or server
     and client will disagree about what "available" means on a given day. That is exactly the
     duplicated-logic risk that already exists, and this proposal makes that logic more complex,
     not simpler.

This second part is why I would not treat it as a "small starting point": it touches Domain
(releasing an invariant), Application (the expander algorithm) *and* two synchronised
implementations of the same rule.

---

## 3. Testing it against the MVP principle and the architecture

Two things worth naming explicitly. Neither is a showstopper, but both matter for ordering.

1. **Availability is Phase 2 in the PRD**, and the Phase 1 gaps from
   [mvp-gaps.md](mvp-gaps.md) are still open. Availability already *works*, if imperfectly —
   this is an improvement to something that functions, not the closing of a gap that blocks the
   first customer. No objection to doing it now if you want to, but the priority trade-off
   should be visible rather than ignored.
2. **The "both directions" extension is not in the PRD.** The PRD example
   (`Friday: unavailable 19:00–22:00`) is one-directional: the pattern is the default
   availability and an exception blocks an extra piece. The scenario "I am normally unavailable
   on Monday evenings, but *this* Monday I am" is an extension coming out of the brainstorm, not
   something the gym customer asked for. Tested against "planning faster, fewer mistakes, better
   overview, less WhatsApp": it might prevent one WhatsApp message, but it is the most expensive
   change in this whole plan in risk and complexity, for a scenario not yet confirmed as a real
   problem. See open question 1.

No other conflicts with the architecture — the existing model (TenantEntity, the Result pattern,
FluentValidation, a module policy on the controller) already follows the conventions, and this
plan proposes no change there.

---

## 4. Suggested approach

Split this into two independent steps, so that step 1 delivers value on its own and step 2
happens only if there is a real reason.

### Step 1 — UI rearrangement, the model stays a blocklist, no backend risk

- Move the base-pattern editor to settings or the profile (decouple the weekly section of
  `RulesEditor.vue` from the one-time section; no data model change).
- Employee: extend `PlanningMyDayStrip.vue` / `PlanningMyDayDetail.vue` so that their own
  exceptions (and optionally a pattern indication) are shown alongside shifts on the same day
  axis. Creating a new exception could live here too, instead of on a separate page.
- Planner: keep the existing `showAvailability` overlay, possibly default it on, and optionally
  add a way to create an exception on an employee's behalf from the calendar (the backend
  authorization already exists).
- While we are in there: migrate `RuleSheet.vue` to the generic `Form`/`useCreate`/`useEdit`
  stack. That is opportunistically riding along with conventions the rest of the app already
  follows, not a separate project.

### Step 2 — "overrides in both directions" (only if you really want it), scheduled separately

Unlock `Status.Available`, rewrite the expander with precedence, and carry the client-side
mirror logic along in lockstep. Only worth starting once step 1 is in place and you confirm
that this scenario (normally unavailable → available this one day) genuinely occurs at the gym
customer.

This separates "safe and quick" from "risky and expensive", instead of treating them as one
brainstorm package.

---

## 5. Open questions (your call)

1. **Is the "both directions" extension (step 2) needed now**, or is the existing blocklist
   direction (pattern plus extra blocks) enough for the time being? This decides whether step 2
   gets scheduled at all.
2. **Day-part precision**: there has never been a DayPart enum in the current code — it was
   deliberately removed in an earlier iteration. Today everything is free `TimeOnly` start/end
   with UI presets (all day / until / from / custom). Do you want morning/afternoon/evening back
   as fixed blocks, or do free times plus presets stay the norm?
3. **Who may create an exception?** The backend already allows "planner on behalf of employee"
   for both rule types. The question is purely UX: do you offer that explicitly from the
   planning calendar (step 1), or does it stay available only through the employee
   calendar/settings with a `?userId=` style detour as now?
4. **Conflict handling**: when an exception is added *after* a shift has already been scheduled
   on that date, do you want (a) nothing — the planner finds out through the overlay, (b) a soft
   warning (toast or dashboard notice) at creation time, or (c) a hard block until it is
   resolved? Option (b) is the smallest addition that serves "fewer mistakes" without
   introducing a blocking flow.
5. **Priority**: pick this up now, or finish the Phase 1 gaps from [mvp-gaps.md](mvp-gaps.md)
   (open shifts, sharing a planning) first? No technical blocker, purely a question of order.

---

## Where this stands (2026-09-14)

Parts of step 1 have happened on their own since this was written, but the plan itself was never
agreed and step 2 is untouched:

- **Done:** the hand-written `availabilityClient.ts` and the server-side
  `AvailabilityOverlapChecker` are both gone. Conflict detection now runs through
  `utils/planning/availabilityMath.ts` and `usePlanningAvailabilityPeriods`.
- **Not done:** `RuleSheet.vue` is still a hand-written form on `UFormField`, not on the
  `Form`/`useCreate`/`useEdit` stack. The base-pattern editor has not moved. The employee still
  has no combined roster-plus-availability view.
- **Step 2 untouched:** `ValidateStatus` still throws on anything but `Unavailable`, so the
  model is still a blocklist, and `AvailabilityPeriodExpander` still merges rather than
  overrides.

The five open questions are still open.
