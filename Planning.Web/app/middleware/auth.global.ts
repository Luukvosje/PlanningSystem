export default defineNuxtRouteMiddleware((to) => {
  const auth = useAuthStore()

  const publicRoutes = new Set(['/', '/login', '/register', '/join'])
  const bootstrapRoutes = new Set([
    '/organizations/new',
    '/organizations',
    '/join',
    '/login',
    '/register',
  ])

  if (!auth.isAuthenticated) {
    if (!publicRoutes.has(to.path)) {
      return navigateTo('/login')
    }
    return
  }

  if (to.path === '/login' || to.path === '/register') {
    return navigateTo(auth.hasOrganization ? '/dashboard' : '/organizations/new')
  }

  if (to.path === '/') {
    return navigateTo(auth.hasOrganization ? '/dashboard' : '/organizations/new')
  }

  if (!auth.hasOrganization && !bootstrapRoutes.has(to.path)) {
    return navigateTo('/organizations/new')
  }
})
