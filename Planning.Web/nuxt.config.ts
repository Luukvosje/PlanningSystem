export default defineNuxtConfig({
  devtools: { enabled: true },

  app: {
    head: {
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap',
        },
      ],
    },
  },

  modules: ['@pinia/nuxt', '@nuxt/ui'],

  colorMode: {
    preference: 'light',
    fallback: 'light',
  },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    apiBaseUrl: process.env.NUXT_API_BASE_URL ?? 'http://localhost:5264',
    public: {
      apiBaseUrl: process.env.NUXT_PUBLIC_API_BASE_URL ?? '',
    },
  },

  typescript: {
    strict: true,
  },

  imports: {
    dirs: ['composables/**'],
  },
})
