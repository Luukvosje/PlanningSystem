export default defineNuxtConfig({
  compatibilityDate: '2026-07-18',

  devtools: { enabled: true },

  // Avoid Vite failing to resolve the virtual `#app-manifest` import
  // (known Nuxt/Vite race: https://github.com/nuxt/nuxt/issues/33606).
  experimental: {
    appManifest: false,
  },

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

  modules: ['@pinia/nuxt', '@nuxt/ui', '@nuxt/eslint'],

  colorMode: {
    preference: 'light',
    fallback: 'light',
  },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    apiBaseUrl: process.env.NUXT_API_BASE_URL ?? 'http://localhost:5264',
    public: {
      // Client-side API calls need an absolute URL; empty base hits the Nuxt app itself.
      apiBaseUrl: process.env.NUXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5264',
    },
  },

  typescript: {
    strict: true,
  },

  imports: {
    dirs: ['composables/**'],
  },
})