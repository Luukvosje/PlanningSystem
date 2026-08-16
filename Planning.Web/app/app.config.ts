export default defineAppConfig({
  ui: {
    colors: {
      primary: 'neutral',
      secondary: 'teal',
      neutral: 'slate',
    },
    icons: {
      light: 'i-lucide-sun',
      dark: 'i-lucide-moon',
    },
    button: {
      slots: {
        base: 'rounded-sm',
      },
      defaultVariants: {
        color: 'secondary',
      },
    },
    input: {
      slots: {
        base: 'rounded-sm',
      },
    },
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
    table: {
      slots: {
        base: 'h-full',
        tbody: 'h-full [&_tr:has([data-slot=empty])]:h-full',
        empty: 'h-full align-middle',
      },
    },
  },
});
