import { useQueryClient } from '@tanstack/vue-query';

export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuthStore();
  const queryClient = useQueryClient();

  // Entering the site validates the session against /me first (with a silent
  // token refresh behind it), so every decision below runs on a session that
  // is known to work instead of on mere cookie presence.
  await ensureSession(auth, queryClient);

  const publicRoutes = new Set(['/', '/login', '/register', '/join']);
  const bootstrapRoutes = new Set([
    '/organization/new',
    '/organization/select',
    '/organization',
    '/join',
    '/login',
    '/register',
  ]);

  if (!auth.isAuthenticated) {
    if (!publicRoutes.has(to.path)) {
      return navigateTo('/login');
    }
    return;
  }

  if (to.path === '/login' || to.path === '/register') {
    return navigateTo(auth.hasOrganization ? '/dashboard' : await resolveOrganizationTarget('/dashboard', auth, queryClient));
  }

  if (to.path === '/') {
    return navigateTo(auth.hasOrganization ? '/dashboard' : await resolveOrganizationTarget('/dashboard', auth, queryClient));
  }

  if (!auth.hasOrganization && !bootstrapRoutes.has(to.path)) {
    // resolveOrganizationTarget may auto-select a lone membership and return
    // to.fullPath itself — in that case the org context is now valid, so
    // just let this same navigation proceed instead of redirecting to it.
    const target = await resolveOrganizationTarget(to.fullPath, auth, queryClient);
    if (target !== to.fullPath) {
      return navigateTo(target);
    }
  }
});
