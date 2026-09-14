export default defineNuxtConfig({
  compatibilityDate: '2026-07-18',

  devtools: { enabled: true },

  // Avoid Vite failing to resolve the virtual `#app-manifest` import
  // (known Nuxt/Vite race: https://github.com/nuxt/nuxt/issues/33606).
  experimental: {
    appManifest: false,
  },

  // Inter is not linked here: @nuxt/fonts ships with @nuxt/ui and already self-hosts it under
  // /_fonts with metric-adjusted fallbacks. A Google Fonts <link> on top of that was two
  // render-blocking third-party connections for a font that is served locally.

  modules: ['@pinia/nuxt', '@nuxt/ui', '@nuxt/eslint', '@nuxtjs/i18n'],

  ui: {
    theme: {
      // Replaces the default ['primary', 'secondary', ...]. `neutral` is always available and is
      // not listed here. Mapping to actual palettes happens in app/app.config.ts.
      colors: ['brand', 'success', 'info', 'warning', 'error'],
      defaultVariants: {
        color: 'brand',
      },
    },
  },

  colorMode: {
    preference: 'light',
    fallback: 'light',
  },

  i18n: {
    defaultLocale: 'nl',
    locales: [
      { code: 'nl', language: 'nl-NL', name: 'Nederlands', file: 'nl.json' },
      { code: 'en', language: 'en-US', name: 'English', file: 'en.json' },
    ],
    strategy: 'no_prefix',
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'planning_locale',
      fallbackLocale: 'nl',
    },
    vueI18n: './i18n.config.ts',
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
});