# Visual language — Planning.Web

This describes what has **actually been built**, not what we once intended. The timeline is the
benchmark: it is the screen with the most thought in it, and everything else has been pulled
towards it. If you deviate, update this document — a spec that contradicts the code is worse
than no spec.

Code conventions (data flow, form composables, query keys) live in [frontend.md](frontend.md).
This document is only about how it looks.

## The nine rules

1. **Semantic tokens, almost dogmatically.** `bg-default` / `bg-muted` / `bg-elevated` /
   `bg-accented`, `text-default` / `text-muted` / `text-dimmed` / `text-toned` /
   `text-highlighted`, `border-default`. No `gray-*`, `slate-*` or `zinc-*` in components —
   there are currently zero of those. The only exceptions are status signals (the red dot in
   `CurrentTimeIndicator`) and overlays on top of a user-chosen block colour, where a token
   cannot contrast by definition.
2. **1px hairlines, hierarchy through opacity.** `/8`–`/25` for the finest grid, `/40`–`/70`
   for row and day separation. A primary boundary is the only thing allowed to be 2px:
   `border-r-2 border-default/70` between the resource column and the grid. Never thicker.
3. **One radius ladder.** `main.css` sets `--ui-radius: 0.375rem` (6px, where Nuxt UI itself
   uses 0.25rem) and Nuxt UI derives the whole Tailwind scale from it — `sm` = ×1, `md` = ×1.5,
   `lg` = ×2, `xl` = ×3:

   | Class | Value | For |
   |---|---|---|
   | `rounded-sm` | 6px | the smallest things: drag tooltip, availability overlay, badge inside a label |
   | `rounded-md` | 9px | controls and timeline objects: button, input, block |
   | `rounded-lg` | 12px | containers: card, table, context menu |
   | `rounded-xl` | 18px | app shells, modals, the nav panel |
   | `rounded-full` | — | true circles only: colour picker, status dot, day pill |

   Do not put a radius class on a component slot in `app.config.ts` — that bypasses
   `--ui-radius`, and it is exactly how buttons once ended up sharper than the blocks next to
   them.
4. **`truncate` + `min-w-0` on every text node.** When space is tight we hide lines
   progressively rather than letting them wrap. `timeline/Block.vue` is the example: five text
   nodes, all five `min-w-0 … truncate shrink-0`.
5. **What floats is glass; a shadow says "you can pick me up".** See [Glass](#glass) below.
   Shadow is otherwise deliberately scarce and means exactly one thing — this object sits loose
   on the grid: `shadow-sm` on a block, `hover:shadow-md`, `shadow-lg` while dragging. Beyond
   that, only on chrome that genuinely hangs over the page (context menu, expanded nav panel).
   The rail and the content panel used to carry one too; it came off in
   [decision 0011](../decisions/0011-design-system-werkbank-licht.md), because there it was
   decoration and it diluted the one thing a shadow is allowed to say here.
6. **Selection is a ring, not a colour change.** `ring-2 ring-brand ring-offset-1` plus a
   z-index. The object itself does not change colour. A drop target gets the soft variant:
   `ring-1 ring-inset ring-brand/40` with a `bg-brand/10` wash.
7. **Button idiom.** Inactive is `variant="outline"`; in menus and on icons, `ghost`. `subtle`
   is the third fixed variant and means "informative, not click-me-important": badges, status
   labels, the alert default. The primary action leaves its props off and inherits `solid` in
   the accent colour from `nuxt.config.ts` — which is why an explicit `variant="solid"` appears
   nowhere. Icon-only buttons always get an `aria-label`.
8. **Animation only where something changes place.** A handful of `transition-colors` and
   `transition-shadow` without `duration-*`, plus `transition-transform` on the two things that
   scale up on hover. The only place with explicit timing is the expanding nav panel in
   `layouts/default.vue` (`duration-150 ease-out` in, `duration-100 ease-in` out) — and it sits
   behind `motion-reduce:transition-none`. While dragging, the transition is deliberately
   switched off.
9. **Cards are for typing into, not for grouping.** The app shell's content area is already a
   bordered, rounded panel; a `LayoutCard` around a list or a chart is a box inside a box — two
   borders, two radii, double padding. So:

   - **`LayoutSection`** for anything you only read: lists, tiles, empty states. Heading,
     description, `gap-6` between them, no border.
   - **`LayoutCard`** for anything you type into — which is what `FormEditableSection` wraps
     around it — and for what genuinely floats on its own: the auth screens and the dashboard
     tiles.

   This rule was once phrased more strictly ("no cards inside a page"). That stopped being true
   the moment forms got the card; the doc comment in `layout/Section.vue` carries the current
   wording.

## Glass

Everything that hangs over the page is cut from the same material, defined in `main.css`:

| Token | What it is |
|---|---|
| `--glass-bg` | the fill: a `color-mix` of `--ui-bg` with transparent, 62% light / 72% dark |
| `--glass-filter` | `blur(20px) saturate(180%) brightness(105%)`; dark: 24px / 150% / 115% |
| `--overlay-blur` | 6px — the haze over a page a scrim covers |

Three utilities, and which one you reach for depends on what it sits on:

| Utility | Use |
|---|---|
| `glass` | a floating panel: its own fill **plus** material. The nav panel, the timeline ruler |
| `glass-fill` | fill only, for something standing on a panel that already blurs — the resource column's header cells, which land at roughly 86% and so read as one strip running under the ruler |
| `glass-material` | material only, for a Nuxt UI slot. The fill has to be written as `bg-[var(--glass-bg)]` there: tailwind-merge recognises *that* as a background and drops the component's own `bg-default`, whereas it does not know `glass` and would leave both fills standing |

Modal, slideover, popover, select, selectMenu, inputMenu, dropdownMenu and contextMenu are all
set to `glass-material` in `app.config.ts`. Their scrim is
`bg-elevated/25 backdrop-blur-(--overlay-blur)` — deliberately light, because at `/75` there is
nothing left to see behind the panel and the glass is pointless.

Saturation is pushed up because blur washes colour out, and the coloured blocks sliding under
the ruler are exactly what you want to keep recognising while dragging one.

`@media (prefers-reduced-transparency: reduce)` returns everything to fully opaque. That must
not be lost.

## Colour

`nuxt.config.ts` decides which aliases exist (`ui.theme.colors`); `app.config.ts` maps them to
a palette:

- **`brand`** = teal — the accent colour, and the default for every Nuxt UI component. The
  exact shades are overridden in `main.css`: `--color-teal-500: #0d9488`,
  `--color-teal-600: #006a61`. That block is `@theme static` on purpose — Tailwind only emits
  theme variables a utility references, nothing here writes `teal-500`, and without `static`
  the two declarations are dropped and Nuxt UI's own `var(--color-teal-500, <default>)`
  fallback renders #14b8a6 instead. Silently.
- **`neutral`** = gray — carries all the semantic greys, and the gray ramp itself is
  overridden in the same `@theme static` block. That is the single lever: `neutral` maps to
  `gray`, so rewriting eleven shades moves every surface, border and text token at once
  instead of a dozen `--ui-*` names one by one.

  **Warm paper, cool ink.** Shades 50–300 carry a touch of red so the page behind a panel
  reads as paper; 400–950 lean towards the brand's teal so text and borders belong to the
  same family as the accent. Tailwind's own gray is uniformly blue, which is exactly why
  three near-identical surfaces used to be impossible to tell apart and every boundary needed
  a hairline to exist at all.

  | Shade | Value | Lands as |
  |---|---|---|
  | 50 | `#faf9f8` | `bg-muted` |
  | 100 | `#f5f4f2` | `bg-elevated` — **the shell's ground** |
  | 200 | `#e6e4e1` | `bg-accented` |
  | 300 | `#d6d3ce` | `border-default` (light) |
  | 400 | `#9aa0a1` | `text-dimmed` |
  | 500 | `#646c6e` | `text-muted` |
  | 600 | `#4b5254` | `text-toned` |
  | 700 | `#2c3537` | `text-default` (light), `border-default` (dark) |
  | 800 | `#1d2628` | `bg-muted` / `bg-elevated` (dark) |
  | 900 | `#121b1c` | `text-highlighted`, `bg-inverted`, `bg-default` (dark) |
  | 950 | `#0b1112` | — |

  The ground is the darkest surface in the light stack and panels float lighter on it — the
  shell and the auth screens both use `bg-elevated` for it. It used to be a
  `bg-linear-to-r from-neutral-50 to-neutral-200` gradient, which reached past the palette
  into Tailwind's own *neutral* scale and so never moved with the theme.

> They were once named the other way round, with the brand colour under "secondary" and grey
> under "primary". Everything written as `text-primary` therefore rendered grey, including the
> links on the login page. Nuxt UI still knows a built-in `primary` alias that nothing maps to —
> do not use it; it gives an undefined colour rather than an error.

Two tokens are overridden unlayered, and both have to stay outside an `@layer`: Nuxt UI
declares them in `@layer theme`, and an override from inside a layer only wins from a later
layer. Because unlayered beats every layer, each one also has to be **restated in `.dark`** —
otherwise the light value leaks into dark mode.

| Token | Light | Dark | Why |
|---|---|---|---|
| `--ui-border` | neutral-300 | neutral-700 | — |
| `--ui-brand` | brand-**600** | brand-400 (Nuxt UI default) | Shade 500 puts white button labels at 3.7:1, below AA for the 14px text buttons and links use. 600 is 6.5:1. Dark sits under near-black labels and already has the contrast. |

There used to be a third, `--ui-text` on neutral-800, to drag the default text out of grey.
The ramp above made it unnecessary — shade 700 now carries 12.6:1 on its own.

Measured in the running app. Light: text #2c3537 **12.6:1**, highlighted #121b1c **17.5:1**,
muted #646c6e **5.4:1**, the brand button under white **6.5:1**. Dark: bg #121b1c, text
#e6e4e1 **13.8:1**, the brand button **9.4:1**.

**Ink is action, teal is state.** Teal is the brand and it carries selection, "today" and
confirmed — not every button. The primary action is the ink-filled button (`bg-inverted`).
This is what keeps the accent legible as a signal; see
[decision 0011](../decisions/0011-design-system-werkbank-licht.md).

Dark mode is carried entirely by the tokens and has to keep working.

## Type

One family, Inter, on its **variable axis** — `nuxt.config.ts` asks Google for `'400 700'`
rather than a list of weights. That is load-bearing: a first attempt listing weights
individually silently collapsed the family to 400 only, and 510/590 are not on the static
ladder at all.

| Setting | Value | Why |
|---|---|---|
| `--font-weight-medium` | **510** | Moves every `font-medium` in the app without touching a component. |
| `--font-weight-semibold` | **590** | Same for `font-semibold`. The band caps here — 600 and 700 read as shouting at 13px, which is most of this interface. |
| `font-feature-settings` | `"cv01", "ss03"` | Inter's own alternates. They are what stops it looking like the default it is. |
| `letter-spacing` | −0.011em body, −0.018em headings, **0** on `text-[10px]` | Tracking is a function of size. Every reading size here sits between 11 and 15px, where Inter is drawn loose; at 10px negative tracking closes the counters and a timeline block stops being scannable. |
| `font-variant-numeric` | `lining-nums tabular-nums` | Nearly every number here is compared against the one above it — shift times in a block, hours in a week header, counts in a table column — and proportional digits make those columns jitter as data changes. |

Do not reach for `font-bold`. If something needs more emphasis than 590, it needs more size
or more space, not more weight.

## Density

Two scales, deliberately different.

**Timeline** — row height follows from the lane height
(`utils/planning/timelineMath.ts`) plus 2 × `BLOCK_PADDING` (4px). A row with overlapping
shifts stacks lanes:

| Mode | Lane height | Row with one lane |
|---|---|---|
| `compact` | 36px | 44px |
| default | 52px | 60px |
| default at detail zoom (`15m`–`day`) | 80px | 88px |
| `spacious` | 96px | 104px |

Block padding is `px-2 py-0.5`, and `py-2` in `spacious`. Text inside a block: `text-xs
font-semibold` for the title, `text-[10px]` for everything below it.

**Management screens and forms** — the table is set denser in `app.config.ts` than the Nuxt UI
default (`th px-4 py-3.5` / `td p-4`): `th px-3 py-2`, `td px-3 py-4 text-sm text-muted`, which
comes out at roughly 44px row height instead of 56. Card slots are `p-3 sm:p-4`. The page
rhythm is `gap-6`.

You scan a timeline; you fill in a form. Management is therefore slightly roomier, but uses the
same tokens and the same heading recipe: `text-xs font-semibold uppercase tracking-wide
text-muted` — literally the table's `th`, and also what sits above the sections in the
dashboard, the sidebar and the personal week overview.

## Page anatomy

The app chrome (`layouts/default.vue`) is the only place with a page header. A page renders into
it through three slots and does not put a second bar underneath.

| Slot | What goes in |
|---|---|
| `#title` | only when the default title is wrong — on detail pages a `UBreadcrumb` |
| `#actions` | the page's actions: create, delete, period navigation |
| `#tabs` | a `LayoutPageTabs`, directly under the header |

A page that fills a slot sets `definePageMeta({ layout: false })` and wraps itself in
`<NuxtLayout name="default">`. Pages without actions skip that and keep the default header.

### List page

`UiDataTable` inside a `LayoutPageContainer fill`, a search field above it, the primary action
("Add customer") in `#actions` behind a permission check. A row navigates to the detail page.
`LayoutPageContainer` only ties itself to the viewport height with `fill` — without that a
taller card would hang out of it and have its `ring` (an outset box-shadow) clipped at the
scroll edge.

### Detail page

- **The title is a breadcrumb**: `Customers › Acme B.V.`, of which only the first crumb is
  clickable. While loading it says `Loading...` rather than showing an empty crumb. No separate
  back button — that is what the breadcrumb already does.
- **Tabs divide the page**, not cards side by side: overview / details / planning. Each tab is a
  different part of the page, not a panel — hence `:content="false"` in `LayoutPageTabs`.
- **The details tab *is* the form.** `FormEditableSection` shows the live form to anyone who may
  edit and `FormDisplay` with the same fields to anyone who may not. No read mode with an Edit
  button in between and no edit modal: a modal hides the very context you were reading, and a
  mode switch costs a click for something you are allowed to do anyway. The `Edit` button on the
  customer page in `#actions` is therefore not a mode switch but a shortcut to that tab, and it
  disappears once you are there.
- **Without permission** it stays read-only. Showing an input that returns 403 on save is worse
  than not showing the field; `can-edit` comes from `utils/userRole.ts`.
- **Deleting** sits in `#actions` and goes through `UiConfirmModal` — there, because the
  customer page wants to show progress while the delete runs. Elsewhere the default is the
  `UiDeleteConfirm` mounted once in `app.vue`. Nothing is deleted without asking first.

This holds for every entity with a detail page — customers, users, the organization, and
whatever comes next. Modals are left for **creating** (`useCreate`), where there is no record to
navigate to yet.

### Forms

**A form definition never lives in a `.vue` file.** Where it *does* live depends on how it
appears:

| Composable | Folder | Appears as |
|---|---|---|
| `useForm` | `composables/forms/` | whatever the page does with `form.render` |
| `useEdit` | `composables/edit/` | a `FormEditableSection` |
| `useCreate` | `composables/create/` | a `FormCreateModal` through the Nuxt UI overlay |

What belongs in such a definition: the zod schema, the controls, `onSubmit` including cache
invalidation, toasts and navigation, and for `useEdit` the `toState` that maps the entity onto
the form. What the page keeps: loading state, permissions, and the question of when the form is
visible.

That keeps a page readable as a page: load data, pick a state, render. `grid: true` provides the
label-left/field-right layout and the route-leave guard.

The buttons belong in the **`#footer` slot of `form.render`**, not in a slot of the surrounding
container. Only there do they sit inside the `<form>` element, and only there does
`type="submit"` do what it promises. Outside the form, the save button is a button wired to
nothing.

## Shared building blocks

Use these instead of writing the pattern again:

| Component | For |
|---|---|
| `LayoutPageContainer` | page wrapper, `gap-6` rhythm; `fill` for full height |
| `LayoutSection` | a read-only block within a page: heading, description, `#actions`, content — see rule 9 |
| `LayoutSectionHeader` | just that heading, or a loose description. **Never renders the page title** — that is already in the chrome |
| `LayoutCard` | something you type into, or something that floats on its own: auth screens, dashboard tiles |
| `LayoutPageTabs` | the tab row under the page header |
| `LayoutHeaderActions` | an `#actions` group built from a `HeaderAction[]` that collapses to a dropdown below 1024px. Currently only on the planning page, which has the most of them |
| `FormEditableSection` | the standard for an entity's fields: form or read-only view, one card |
| `FormDisplay` | read-only view of the same controls, usable on its own for fields the API cannot update |
| `FormCreateModal` | creating, opened by `useCreate` |
| `UiDataTable` | list view with search filter, empty state and clickable rows |
| `UiQueryState` | error → loading → content, in that order |
| `UiEmptyState` | "nothing here yet" |
| `UiLoadingIndicator` | a standalone loading indicator |
| `UiStatTile` | one number on a detail page — `rounded-lg p-4 ring ring-default`, not a card. Without a `value` it shows a dash plus placeholder, so an unfinished KPI row still reads as deliberate |
| `UiEntitySelect` | the picker for *every* kind of entity — employee, customer, whatever comes next. Search, active/inactive and A-Z/Recent live here once; what an entity *is* lives in `useEntitySource` |
| `UiConfirmModal` | a confirmation with its own progress, for a component that wants to drive it |
| `UiDeleteConfirm` | the one delete dialog, mounted in `app.vue`, reached through `useDeleteConfirm` |
| `AvailabilityRuleRow` | one row in a list with edit and delete actions |
| `AuthFooterLink` | "no account yet? Register" under an auth card |

Button sizes: the default (`md`) for page actions in the header, `sm` inside cards and rows.

## What deliberately is not here

A colour palette with fifty shades, a type scale with eight levels, a grid specification. We had
those, and the code did nothing with them. What is written here is written because it can be
found back in the app.
