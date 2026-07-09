import { VueQueryPlugin, QueryClient, dehydrate, hydrate } from '@tanstack/vue-query'

export default defineNuxtPlugin((nuxtApp) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        retry: 1,
        refetchOnWindowFocus: false,
      },
    },
  })

  nuxtApp.vueApp.use(VueQueryPlugin, { queryClient })

  if (import.meta.server) {
    nuxtApp.hooks.hook('app:rendered', () => {
      nuxtApp.payload.vueQueryState = dehydrate(queryClient)
    })
  }

  if (import.meta.client && nuxtApp.payload.vueQueryState) {
    hydrate(queryClient, nuxtApp.payload.vueQueryState)
  }
})

declare module '#app' {
  interface NuxtPayload {
    vueQueryState?: unknown
  }
}
