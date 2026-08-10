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
      defaultVariants: {
        color: 'secondary',
      },
    },
    table: {
      slots: {
        root: 'border border-default rounded-xl',
        base: 'h-full',
        tbody: 'h-full [&_tr:has([data-slot=empty])]:h-full',
        empty: 'h-full align-middle',
      },
    },
  },
});
