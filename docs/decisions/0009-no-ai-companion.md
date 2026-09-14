# 0009 — No AI companion in the planning

- **Date:** 2026-08-22
- **Status:** final for the MVP
- **Touches:** the planning flow, conflict detection

## Decision

The non-goal "AI planning" from [0001](0001-product-scope-and-non-goals.md) stands. No chat
companion, no LLM in the planning flow. Conflict detection and "who could take this open shift"
stay deterministic rules over `AvailabilityRule` plus overlap detection.

## Why

Those are rule questions, not language-model questions — deterministic, free, and explainable
to the planner. An LLM would send staff data (names, rosters, absences) to a third party, which
costs a data-processing agreement per customer, and costs money per request while there is no
revenue model yet. A chat window on top is also exactly what the bloated competitors do.

## Alternative considered

Narrow natural-language input ("Kevin mon 8-16" → shift). Not rejected, but postponed until
after the first paying customer. That is one endpoint, not a layer.
