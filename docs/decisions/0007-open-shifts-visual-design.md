# 0007 — Open shifts: visual design and the unassign flow

- **Date:** 2026-08-22
- **Status:** final, built
- **Touches:** `Planning.Web/app/components/planning/timeline/Block.vue`, the planning API

## Decision

An open shift gets no colour of its own but the outline style with a **dashed border** (the
shift's colour stays on the border), plus the label "Open dienst" inside the block.

In employee mode a row "Open diensten" sits at the top as soon as there is at least one.
Dragging a shift into that row removes the employee; dragging it out assigns somebody. In the
form, "Open dienst" is the first option in the employee picker.

The API sends `assignedUserName: null`, not the text "Open dienst" — a deviation from
[plans/mvp-gaps.md](../plans/mvp-gaps.md) §1. The label comes from i18n, so the backend carries
no UI text.

## Why

The dashed border reads as "not filled in yet" without claiming a colour that could also be a
real shift colour. The row mirrors the existing "Zonder klant" row, so there is no new pattern
to explain.

## Alternative considered

A grey block. Rejected: grey is already in use for draft shifts (`CONCEPT_BLOCK_STYLE`).

## Where this stands (2026-09-14)

On `main`. `Block.vue` puts `border-dashed` on `isOpenShift` and shows
`t('planning.openShift')` once the block is wider than 80px.
