# 0011 — Werkbank Licht is the design system

- **Date:** 2026-09-14
- **Status:** final
- **Touches:** `Planning.Web/app/assets/css/main.css`, `Planning.Web/nuxt.config.ts`,
  `Planning.Web/app/layouts/default.vue`, `Planning.Web/app/layouts/auth.vue`,
  [../guidelines/design.md](../guidelines/design.md)

## Decision

The visual language is **Werkbank Licht**: Linear's structural rules on a light ground.

1. **A neutral ramp with hue, not Tailwind's blue gray.** Warm paper in 50–300, cool
   teal-leaning ink in 400–950. Overridden once as `--color-gray-*`, which is what `neutral`
   maps to, so every semantic token moves at once.
2. **Surfaces carry the hierarchy, not borders.** The shell's ground is `bg-elevated`
   (#f5f4f2) and panels float lighter on it. The auth screens use the same ground.
3. **The weight band caps at 590.** `--font-weight-medium: 510` and `--font-weight-semibold:
   590`, on Inter's variable axis. 600 and 700 read as shouting at 13px.
4. **Inter's own alternates are on** — `cv01`, `ss03` — with −0.011em tracking on the body,
   −0.018em on headings, and explicitly zero on the 10px labels inside a timeline block.
5. **Ink is action, teal is state.** Teal stays the brand, but it carries selection, "today"
   and confirmed; the primary action is the ink-filled button.
6. **Shadow stays scarce and keeps its one meaning.** It comes off the rail and the content
   panel, where it was decoration. It stays on a timeline block, where it means "this sits
   loose on the grid and you can pick it up".

Density and the radius ladder do not change: compact rows and 6/9/12/18 were already right.

## Why

Three reference systems were read live and compared on six axes — surface ladder, text
contrast, accent frequency, type scale and weights, radius and density, where depth comes
from. The audit is at
<https://claude.ai/code/artifact/f98174a9-7deb-421d-aa5a-766935e08d81>.

**Linear** is the only one of the three built for a dense tool, and it confirmed most of what
this document already said: small radii, hairlines instead of shadow, semantic greys,
selection as a ring. What it changed our mind about is the *amount* — our ladder was set too
soft. **Calendly** supplied the missing piece: a light surface ladder that carries hue.
Tailwind's gray is uniformly blue, which is why three near-identical surfaces were impossible
to tell apart and every boundary needed a hairline to exist at all.

Light rather than dark, because an MKB planner works in daylight and a prospect judges the
product on the light screen. Dark-first would have decided the whole brand, login and landing
included, on the strength of one screen.

Measured after the change (light): ground #f5f4f2 behind panels on #ffffff, border #d6d3ce,
text 12.6:1, highlighted 17.5:1, muted 5.4:1. Dark: bg #121b1c, text 13.8:1, the brand button
9.4:1.

## Alternative considered

- **Werkbank Donker** — the same system on Linear's near-black canvas. Rejected on audience,
  not on looks: it decides the brand for every surface a customer sees first.
- **Atelier** (Calendly) — warm paper, comfortable 60px rows, Geist. Rejected on density: 10
  rows in view against 14, which on the timeline is the difference between scrolling and
  seeing the week. Its surface technique was taken over.
- **Blad** (Sana) — no panels at all, ink pills, a weight-400 display heading. Rejected: with
  no containers the grid has to carry everything on hairlines, which is the problem the audit
  set out to fix.
- **Keeping Tailwind's gray and only darkening the text.** That was the first fix (see the
  commits before this one) and it helped, but it left the surfaces flat. The ramp replaced the
  need for that `--ui-text` override entirely.
