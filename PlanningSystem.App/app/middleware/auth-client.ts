export default defineNuxtRouteMiddleware(async () => {
  const { isAuthenticated, fetchMe } = useAuth()
  const { $serviceFactory } = useNuxtApp()

  if (!isAuthenticated.value) {
    await fetchMe($serviceFactory)
  }

  if (!isAuthenticated.value) {
    return navigateTo('/auth/login')
  }
})
