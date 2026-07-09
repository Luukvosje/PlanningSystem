export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuthStore()
  const toast = useToast()

  const excludedPaths = new Set(['/organizations/new', '/join'])

  if (!auth.isAuthenticated || !auth.hasOrganization || excludedPaths.has(to.path)) {
    return
  }

  if (!auth.currentUser?.modules) {
    await auth.fetchMe()
  }

  const requiredModule = getRequiredModuleForPath(to.path)

  if (!requiredModule) {
    return
  }

  if (!canAccessRoute(to.path, auth.currentUser?.modules)) {
    const isOrgAdminRoute = to.path === '/organizations' || to.path.startsWith('/organizations/')
    if (isOrgAdminRoute && canManageOrganization(auth.currentUser?.role)) {
      return
    }

    toast.add({
      title: 'Geen toegang',
      description: 'Deze module is uitgeschakeld voor jouw account.',
      color: 'warning',
    })
    return navigateTo('/dashboard')
  }
})
