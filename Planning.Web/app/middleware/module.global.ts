import { useQueryClient } from '@tanstack/vue-query';
import { getApiAuthMe } from '~/generated/api/auth/auth';
import { isApiError } from '~/types/api-error';
import { queryKeys } from '~/utils/queryKeys';

export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuthStore();
  const toast = useToast();
  const queryClient = useQueryClient();
  // Route middleware is not a setup context, so useI18n() would throw here.
  const { t } = useNuxtApp().$i18n;

  const excludedPaths = new Set(['/organization/new', '/organization/select', '/join']);

  if (!auth.isAuthenticated || !auth.hasOrganization || excludedPaths.has(to.path)) {
    return;
  }

  if (!auth.currentUser?.modules) {
    // Fetches through the same query key/client as useCurrentUser() (used by
    // the layout) instead of calling fetchMe() directly, so the two don't
    // race as independent, non-deduped requests for the same data.
    try {
      auth.currentUser = await queryClient.ensureQueryData({
        queryKey: [...queryKeys.auth.me, auth.organizationId],
        queryFn: () => getApiAuthMe(),
        retry: false,
      });
    } catch (error) {
      if (isApiError(error) && error.code === 'NO_ORGANIZATION') {
        // The current org cookie is stale (deleted org, revoked membership,
        // ...) rather than the session itself being invalid: send the user
        // to pick a still-valid organization instead of logging them out.
        // If resolveOrganizationTarget auto-selected a lone membership, it
        // already refreshed auth.currentUser — fall through to the module
        // check below instead of returning early.
        const target = await resolveOrganizationTarget(to.fullPath, auth, queryClient);
        if (target !== to.fullPath) {
          return navigateTo(target);
        }
      } else {
        // customFetch already tried a silent token refresh before this
        // rejected; a redirect here (real middleware, not a nested ofetch
        // hook) is the safe, supported way to send the user back to login
        // instead of letting the rejection surface as an SSR error page.
        auth.logout();
        return navigateTo('/login');
      }
    }
  }

  if (isPlannerOnlyRoute(to.path) && !canManagePlanning(auth.currentUser?.role)) {
    toast.add({
      title: t('errors.plannerOnly.title'),
      description: t('errors.plannerOnly.description'),
      color: 'warning',
    });
    return navigateTo('/planning');
  }

  const requiredModule = getRequiredModuleForPath(to.path);

  if (!requiredModule) {
    return;
  }

  if (!canAccessRoute(to.path, auth.currentUser?.modules)) {
    const isOrgAdminRoute = to.path === '/organization' || to.path.startsWith('/organization/');
    if (isOrgAdminRoute && canManageOrganization(auth.currentUser?.role)) {
      return;
    }

    toast.add({
      title: t('errors.moduleDisabled.title'),
      description: t('errors.moduleDisabled.description'),
      color: 'warning',
    });
    return navigateTo('/dashboard');
  }
});
