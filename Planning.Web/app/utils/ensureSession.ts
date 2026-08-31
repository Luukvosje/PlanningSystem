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

  // Deliberately also runs without an organization cookie: /me resolves the
  // account's only active membership by itself, so the one case the cookie is
  // missing but unambiguous heals here instead of bouncing the user through
  // the chooser. resolveOrganizationTarget still owns the ambiguous ones.
  try {
    const user = await queryClient.ensureQueryData({
      queryKey: [...queryKeys.auth.me, authStore.organizationId],
      queryFn: () => getApiAuthMe(),
      retry: false,
    });

    // Adopt whatever organization the API answered for, so the header on every
    // later request matches the membership /me was resolved against.
    authStore.setOrganizationId(user.organizationId);
    authStore.currentUser = user;
  } catch (error) {
    if (isApiError(error) && error.code === 'NO_ORGANIZATION') {
      // Stale organization cookie (org deleted, membership revoked) rather than
      // a dead session: keep the tokens, drop the org so the guards re-resolve it.
      authStore.setOrganizationId(null);
      return;
    }

    // customFetch already spent a silent refresh before a 401 could reach this
    // far, and it clears the session itself when the refresh token was the
    // thing rejected. Everything else — the API being down or restarting, a
    // 5xx, a dropped connection — says nothing about whether the session is
    // still good, so the tokens stay and the next attempt can recover. Throwing
    // a valid refresh token away over one failed call is how a user ends up on
    // /login with the cookie they were just looking at gone.
  }
}
