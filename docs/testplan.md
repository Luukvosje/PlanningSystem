# Manual test list — after the cleanup round

None of this has been run in a browser. This list covers what changed in phases 1 to 5, sorted
by risk: **A** first, because that is where a mistake costs the most.

Preparation:

```bash
dotnet ef database update --project Planning.Infrastructure --startup-project Planning.Api
```

```bash
cd Planning.Web && pnpm dev
```

You need at least three accounts in one organization: an **Owner/Admin**, a **Planner** and an
**Employee**. For items 2 and 12 you need a second organization.

---

## A. Highest risk — this is where it can go wrong silently

### 1. Times are in the right place ⚠️ the most important test

The backend used to send timestamps without a `Z`, after which JavaScript read them as local
time. That was fixed in the database layer, and the frontend workaround that masked it has been
removed. If there is a mistake here, **everything** shifts by your UTC offset (currently 2
hours).

- [ ] Create a booking from **10:00 to 11:00**. Close the sidebar. Is the block at 10:00?
- [ ] Reload the page (F5). Is it still at 10:00, not 12:00 or 08:00?
- [ ] Open the booking again — does the form say 10:00–11:00?
- [ ] Check the red "now" line: is it at the actual current time?
- [ ] Create a booking around **midnight** (23:30–00:30) and see whether it crosses the day
      boundary as expected.
- [ ] In the network tab, look at the response of `GET /api/planning`. Do `startUtc` and
      `endUtc` end in a **`Z`**? If not, stop and report it.

### 2. Tenant isolation

- [ ] Sign in to organization A, copy a planning id from the URL or the network tab.
- [ ] Switch to organization B and call `GET /api/planning/{that-id}`.
      Expected: **404**, with the text "Planning record not found" — not 403, and certainly no
      data.
- [ ] Same for a customer id and an availability rule id.

### 3. Availability no longer leaks

This was a real leak: every employee could see the holiday and sickness reasons of all their
colleagues.

- [ ] Sign in as **Employee**. Open the planning. In the network tab, does
      `GET /api/availability/rules/for-planning` return only **your own** periods?
- [ ] As Employee: call that endpoint with `?EmployeeIds=<a colleague's id>`.
      Expected: **403**.
- [ ] As Employee: with your **own** id → allowed.
- [ ] Sign in as **Planner**. Without a filter: do you see the whole team?

### 4. Switching modules on and off still works

This is where I nearly got it wrong myself: an `AsNoTracking()` on the wrong query would have
dropped changes without an error.

- [ ] As Owner/Admin: switch a module **off** for the organization. Confirm. **Reload the
      page.** Is it still off?
- [ ] Switch it back on. Reload. Is it on?
- [ ] Same per user, if you can reach that UI (it is largely disabled).

---

## B. Changed behaviour — this is *supposed* to differ from before

### 5. Status transitions are now guarded

- [ ] Set a booking to **Cancelled**. Then try to set it to **Confirmed**.
      Expected: refused, with the Dutch message
      *"Een afgeronde of geannuleerde boeking kan niet meer van status wijzigen."*
- [ ] Same from **Completed**.
- [ ] Planned → Confirmed: **must simply work**.
- [ ] Confirmed → Completed: must work. Confirmed → Cancelled: must work.
- [ ] **Watch out here:** change only the *title* of a booking and save, without touching the
      status. That has to succeed. A status that stays the same is not a transition.
- [ ] Switch the app to English and repeat the first step — does the message come out in
      English?

### 6. The opening-hours window has changed

There were six different defaults in the code. Now there is one: **the whole day**. For an
organization **without** configured opening hours you used to see 07:00–19:00.

- [ ] Organization **without** opening hours: the timeline now shows the full day. Expected.
- [ ] Organization **with** opening hours (say 08:00–18:00): the window follows that setting.
- [ ] Do you find the whole day too wide as a default? Say so — we can put it back.

### 7. The grey shading lines up with the blocks (A4)

The "outside opening hours" shading was drawn with different arithmetic than the blocks, so
under the default settings the two did not line up.

- [ ] Set opening hours to, say, **09:00–17:00**. Leave the defaults (row layout *spacious*,
      "whole day" **off**).
- [ ] Does the grey band start exactly where 09:00 sits on the timeline, and end at 17:00?
- [ ] Create a booking from 08:00–10:00: does the left half fall inside the grey zone and the
      right half outside, neatly on the boundary?
- [ ] Switch the row layout to **compact** and back. Does the shading still line up?
- [ ] Switch "whole day" **on**: the shading disappears or covers the day, and the blocks stay
      correct.

### 8. Overlap marking

- [ ] Two overlapping bookings for the same employee → both get the red border.
- [ ] Two bookings that touch exactly (10:00–11:00 and 11:00–12:00) → **no** border.
- [ ] Overlapping bookings for **different** employees → no border.
- [ ] **New behaviour:** cancel one of two overlapping bookings. The red border on the other one
      must **disappear**.

### 9. Availability rules: who may do what

- [ ] As **Employee**: create your own holiday or sick note → must work.
- [ ] As **Employee**: try to **change** your own rule → refused.
- [ ] As **Employee**: try to **delete** your own rule → refused.
- [ ] As **Planner**: changing and deleting → must work.

*(This was your choice: "create it yourself, the planner deletes it". Changing deliberately
falls under deleting: otherwise you shorten your own rule to a minute and have effectively
removed it.)*

### 10. A malformed URL gives 400, not 500

- [ ] `GET /api/planning?StartUtc=2026-08-01T00:00:00Z&EndUtc=2026-08-31T00:00:00Z&UserIds=abc`
      → **400** with a field error on `UserIds`. Previously a 500.
- [ ] Same with `&Statuses=Forgotten` → 400.

---

## C. Does everything still work — the whole planning flow

The two hand-written API clients have been removed and replaced by the generated client. Every
action below now runs over a different code path than before.

### 11. Planning CRUD

- [ ] **Create** a booking from the sidebar
- [ ] **Open** and **change** a booking (title, description, notes, customer, employee, colour)
- [ ] **Move** a booking by dragging it to another time
- [ ] **Move** a booking to another row (another employee)
- [ ] **Extend/shorten** a booking by dragging its edge
- [ ] **Confirm** a booking
- [ ] **Duplicate** a booking
- [ ] **Delete** a booking
- [ ] After every action: does the change appear immediately, and does it survive F5?

### 12. The rest of the app

- [ ] Sign in and out; leave a session open long enough for the token to be refreshed (the
      refresh call was deliberately *not* migrated — check you are not thrown out)
- [ ] Multiple organizations: the picker shows the **right names** (these are now fetched in one
      query instead of one per membership)
- [ ] The user list loads and shows the right modules per user (same, a batch query)
- [ ] Customers: list, create, change, delete, search
- [ ] Users: list, change role, search
- [ ] Create and accept an invite
- [ ] **Upload a logo** in the organization settings (a new code path) — and does it then appear?
- [ ] Save the planning settings (opening hours plus important working times)
- [ ] The "My planning" view
- [ ] Dashboard: "next shift"

### 13. Forms

`components/form/Control.vue` was rewritten — **this had already broken the production build**
(`pnpm build` failed before this round). Every field type needs checking again.

- [ ] Text field, email field, **password field**, textarea, select — type into them and save
- [ ] Validation errors appear under the right field
- [ ] The create modal and the edit modal open, save and close (their internal form registration
      was merged)

### 14. Translations

13 hard-coded Dutch strings were moved into i18n.

- [ ] Save, change and delete an availability rule → toast in **Dutch**
- [ ] Switch the app to **English** → the same toasts in English
- [ ] Update the organization, save planning settings, save a logo, update a profile → both
      languages
- [ ] Navigate to a module you have no access to → "Geen toegang" / "No access"

---

## D. What has gone — nothing should be missing here

- [ ] There was a "mixed" timeline view in the code. It referenced store fields that do not
      exist and was never rendered. **Do you notice anything missing?**
- [ ] `GET /api/planning/week` has been removed; nothing used it.
- [ ] The colour picker on a booking: are all 10 colours still there?

---

## Known and deliberately not fixed

No bugs to report here — we already know about these:

| What | Why |
|---|---|
| **There is no sign-out endpoint.** Refresh tokens are never revoked on sign-out. | Kept outside this round. |
| Overlap is calculated per query, so overlap across a **month boundary** is not seen. | Fixing it properly needs a separate query. |
| Time zones in **availability**: the backend calculates in UTC-like time, the frontend in local time. | This is A7 — we do that together. |
| A shift **across midnight** misses the next day's rules. | Belongs to A7. |
| The module management UI is disabled (commented out). | Your choice: unfinished, not dead. |
| `AvailabilityRuleStatus.Preferred` / `.Available` are unreachable. | Left as is, your choice. |
| 10 typecheck errors in `nuxi typecheck`. | All of them predate this round. |
| Many `Missing semicolon` lint errors. | An existing style split in the codebase. |

## Still to discuss

- **A7** (time zones in availability) — together
- Merging `useCreate`/`useEdit` and the two modals — deliberately postponed until after the UI
  conversation
- `customers/index.vue` and `users/index.vue` are the same page — same
- Three separate implementations of the same save/cancel button pair — same
