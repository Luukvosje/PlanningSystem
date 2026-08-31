import type { QueryClient } from '@tanstack/vue-query';
import { getApiOrganizationsMine } from '~/generated/api/organizations/organizations';
import { queryKeys } from '~/utils/queryKeys';

/**
 * Decides where an authenticated user without a valid current-organization
 * context should land: auto-selects and continues to `intendedPath` when
 * they belong to exactly one organization they may enter, sends them to the
 * chooser when they belong to several, to the deactivated page when every
 * membership they have is inactive, or to the create-organization flow when
 * they belong to none (or the membership lookup itself fails).
 *
 * authStore/queryClient must be captured by the caller before crossing any
 * async boundary (e.g. the failed request whose catch block is calling
 * this). Both useAuthStore() and useQueryClient() rely on Vue's injection
 * context, which isn't reliably still available by the time this runs.
 */
export async function resolveOrganizationTarget(
  intendedPath: string,
  authStore: ReturnType<typeof useAuthStore>,
  queryClient: QueryClient,
): Promise<string> {
  let memberships;
  try {
    memberships = await queryClient.ensureQueryData({
      queryKey: queryKeys.organizations.mine,
      queryFn: () => getApiOrganizationsMine(),
    });
  } catch {
    return '/organization/new';
  }

  const active = memberships.filter((membership) => membership.isActive);

  if (!active.length) {
    // Having memberships but none you may enter is a deactivated member, not a new account -
    // "create an organization" is a strange answer to being locked out of the one you were in.
    return memberships.length ? '/account/inactive' : '/organization/new';
  }

  if (active.length === 1 && active[0]?.organizationId) {
    authStore.selectOrganization(active[0].organizationId);
    await authStore.fetchMe();
    return intendedPath;
  }

  return '/organization/select';
}
