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
      },
    },
    card:{
     slots: {
      root: 'w-full lg:max-w-2xl mx-auto',
     }, 
    },
  },
});
