---
name: frontend-conventions
description: Load the Nuxt/Vue conventions for this repo before writing frontend code. Use when working anywhere under Planning.Web — pages, components, composables, stores, schemas, middleware, i18n or the orval client. Covers the data flow from generated client to query composable, the form system, delete confirmation, route gating, i18n and the visual language.
---

# Frontend conventions

Read [docs/guidelines/frontend.md](../../../docs/guidelines/frontend.md) **in full** before
writing or changing anything in `Planning.Web`. It is the authority on:

- the data flow: `app/generated` → `apiClient.ts` → `composables/api` → `composables/queries`
- query keys, invalidation and error handling
- the form system (`useForm` / `useEdit` / `useCreate`) and where a definition lives
- editable-vs-read-only cards and permission checks
- delete confirmation
- routing, module gating and session state
- i18n, ESLint rules and the no-watchers rule

Also read [docs/guidelines/design.md](../../../docs/guidelines/design.md) when the change is
visual — tokens, radii, density, the glass material, page anatomy, which shared component to
reach for.

Then check `CLAUDE.md` for the non-negotiables and `docs/decisions/` for anything that
looks arbitrary.

Run `pnpm lint` on the files you edited rather than guessing at the formatting rules.
