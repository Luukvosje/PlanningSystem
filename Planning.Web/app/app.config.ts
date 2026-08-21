export default defineAppConfig({
  ui: {
    /**
     * `brand` is the accent (teal, exact shades in main.css); `neutral` carries the greys behind
     * the semantic tokens (bg-default, text-muted, border-default).
     *
     * They used to be named the other way round: the accent lived under the "secondary" alias
     * while "primary" was mapped to the grey palette, so accent-looking classes rendered grey.
     * The alias list itself lives in nuxt.config.ts under `ui.theme.colors`.
     *
     * Nuxt UI still knows a built-in "primary" alias that nothing maps to - using it would
     * produce an undefined colour rather than an error, so do not.
     */
    colors: {
      brand: 'teal',
      neutral: 'olive',
    },
    icons: {
      light: 'i-lucide-sun',
      dark: 'i-lucide-moon',
    },
    /**
     * One radius ladder, matching the timeline: controls at --ui-radius (6px), containers at
     * rounded-lg (8px), shells and modals at rounded-xl (12px). Buttons and inputs deliberately
     * carry no radius override - a hardcoded one here would bypass --ui-radius.
     */
    modal: {
      variants: {
        fullscreen: {
          false: {
            content: 'rounded-xl',
          },
        },
      },
    },
    card: {
      slots: {
        header: 'p-3 sm:p-4',
        body: 'p-3 sm:p-4',
        footer: 'p-3 sm:p-4',
      },
    },
    /** Pages used to render solid red error blocks while forms used subtle ones. */
    alert: {
      defaultVariants: {
        variant: 'subtle',
      },
    },
    table: {
      slots: {
        base: 'h-full',
        tbody: 'h-full [&_tr:has([data-slot=empty])]:h-full',
        thead: 'rounded-lg',
        empty: 'h-full align-middle',
        // Denser than the Nuxt UI default (th px-4 py-3.5 / td p-4): ~44px rows instead of ~56.
        // The header borrows the timeline's section-label recipe.
        th: 'px-3 py-2 text-xs font-semibold uppercase tracking-wide text-muted text-left',
        td: 'px-3 py-2.5 text-sm text-muted whitespace-nowrap',
      },
    },
  },
});
