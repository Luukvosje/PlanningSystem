# 0003 — Sharing a planning: frontend only, `wa.me` link

- **Date:** 2026-08-16
- **Status:** provisional — not built yet
- **Touches:** `Planning.Web/app/utils/planning/`, `app/components/planning/header/`

## Decision

No backend change. A text-format function turns the already-loaded week planning into readable
text, a "copy" button uses the clipboard API, and sharing goes through
`https://wa.me/?text=<url-encoded text>`. PDF export comes explicitly later.

## Why

The week planning data is already available through the existing query composables — no reason
to extend the API for what is purely a presentation problem. `wa.me` works on desktop (WhatsApp
Web) and mobile without an extra dependency.

## Alternative considered

Taking PDF export along now. Rejected: it is explicitly marked *later* in the [PRD](../prd.md).

## Still open

Text grouped per employee, or per day with all employees underneath? See
[plans/mvp-gaps.md](../plans/mvp-gaps.md) §2.

## Where this stands (2026-09-14)

Nothing built. There is no `formatWeekPlanningText.ts` and no clipboard or `wa.me` code in
`Planning.Web`.
