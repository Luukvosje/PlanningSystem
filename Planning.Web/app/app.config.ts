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
      neutral: 'gray',
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
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
      },
      variants: {
        fullscreen: {
          false: {
            content: 'rounded-xl',
          },
        },
        overlay: {
          true: {
            overlay: 'bg-elevated/25 backdrop-blur-(--overlay-blur)',
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
    /**
     * Everything that floats over the page is cut from glass. The fill is written as an
     * arbitrary value rather than the `glass` utility on purpose: tailwind-merge only drops a
     * component's own `bg-default` for a class it recognises as a background, and a custom
     * utility is not one - both fills would end up on the element and the cascade would decide.
     *
     * The scrims lose most of their dimming and blur instead. At `bg-elevated/75` there is
     * nothing left behind the panel to see through it, which makes the glass pointless.
     */
    slideover: {
      slots: {
        overlay: 'bg-elevated/25 backdrop-blur-(--overlay-blur)',
        content: 'bg-[var(--glass-bg)] glass-material',
      },
    },
    popover: {
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
      },
    },
    select: {
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
      },
    },
    selectMenu: {
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
      },
    },
    inputMenu: {
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
      },
    },
    dropdownMenu: {
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
      },
    },
    contextMenu: {
      slots: {
        content: 'bg-[var(--glass-bg)] glass-material',
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
        /**
         * The table is stretched to the container so the empty state can sit centred in it.
         * A table that is taller than its rows hands the leftover height to those rows, which
         * turned a one-row list into one row as tall as the table - hence the filler row after
         * the data, dropped again while the empty state is the thing that has to fill.
         */
        tbody: 'h-full [&_tr:has([data-slot=empty])]:h-full after:table-row after:h-full [&:has([data-slot=empty])]:after:hidden',
        thead: 'rounded-lg',
        empty: 'h-full align-middle',
        // Denser than the Nuxt UI default (th px-4 py-3.5 / td p-4): ~44px rows instead of ~56.
        // The header borrows the timeline's section-label recipe.
        th: 'px-3 py-2 text-xs font-semibold uppercase tracking-wide text-muted text-left',
        td: 'px-3 py-4 text-sm text-muted whitespace-nowrap',
      },
    },
  },
});
