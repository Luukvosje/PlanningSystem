import type { QueryClient } from '@tanstack/vue-query';
import { getApiAuthMe } from '~/generated/api/auth/auth';
import { isApiError } from '~/types/api-error';
import { queryKeys } from '~/utils/queryKeys';

/**
 * Validates the stored session once per app load, before any route guard gets
 * to decide where the user may go.
 *
 * Cookie presence says nothing about whether the session still works, so
 * entering the site — including landing straight on /login — always costs one
 * /me round trip. customFetch turns a 401 on that call into a silent
 * /api/auth/refresh plus retry; when that fails too the session is dropped
 * here and the guards take over from an honestly logged-out state.
 *
 * authStore/queryClient are passed in rather than resolved here because the
 * awaits below outlive the caller's injection context.
 */
export async function ensureSession(
  authStore: ReturnType<typeof useAuthStore>,
  queryClient: QueryClient,
): Promise<void> {
  const checked = useState('auth:session-checked', () => false);
  if (checked.value) {
    return;
  }
  checked.value = true;

  // A refresh token is the only thing that can outlive an expired access
  // token, so without one there is no session to validate or recover.
  if (!authStore.refreshToken) {
    authStore.logout();
    return;
  }

  // /me resolves a membership from the organization header; without one it can
  // only answer NO_ORGANIZATION. resolveOrganizationTarget owns that case.
  if (!authStore.hasOrganization) {
    return;
  }

  try {
    authStore.currentUser = await queryClient.ensureQueryData({
      queryKey: [...queryKeys.auth.me, authStore.organizationId],
      queryFn: () => getApiAuthMe(),
      retry: false,
    });
  } catch (error) {
    if (isApiError(error) && error.code === 'NO_ORGANIZATION') {
      // Stale organization cookie (org deleted, membership revoked) rather than
      // a dead session: keep the tokens, drop the org so the guards re-resolve it.
      authStore.setOrganizationId(null);
      return;
    }

    authStore.logout();
  }
}
