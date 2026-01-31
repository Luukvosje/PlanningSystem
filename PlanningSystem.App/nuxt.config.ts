export default defineNuxtConfig({
    modules: ["@nuxt/ui"],
    devtools: { enabled: true },
    compatibilityDate: "2024-12-19",
    future: {
      compatibilityVersion: 4,
    },
    css: ["~/assets/css/main.css"],
    runtimeConfig: {
      public: {
        apiBaseUrl: process.env.API_URL || 'http://localhost:5157'
      }
    },
    ssr: true,
  });
