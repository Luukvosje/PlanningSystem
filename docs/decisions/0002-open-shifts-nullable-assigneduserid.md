# 0002 — Open shifts: nullable `AssignedUserId`, no separate status field

- **Date:** 2026-08-16
- **Status:** final for the data model
- **Touches:** `Planning.Domain/Planning/PlanningRecord.cs`, the planning API, the timeline

## Decision

`PlanningRecord.AssignedUserId` is `Guid?`. A shift *without* an employee **is** the open
shift — no separate `OpenShift` status and no separate table. `IsOpenShift` is a convenience
property over the null check.

## Why

The simplest model that solves the problem, and it fits
[0001](0001-product-scope-and-non-goals.md): build nothing that does not contribute to planning
faster.

## Alternative considered

A separate status label for dashboard counts. Not needed — that count works off the null check
too.

## Deliberately not

The "employee accepts an open shift" flow. That is a separate action and a separate endpoint;
see [0010](0010-timeline-is-a-planner-tool.md) for why it stays postponed.

## Where this stands (2026-09-14)

Built and on `main`. `IsOpenShift` sits in the domain, `OPEN_SHIFT_ROW_ID` and
`OPEN_SHIFT_SELECT_VALUE` in `Planning.Web/app/utils/planning/constants.ts`. The open question
about its visual design was answered in [0007](0007-open-shifts-visual-design.md).
