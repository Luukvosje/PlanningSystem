import { useQuery } from '@tanstack/vue-query';
import { getApiRequests } from '~/generated/api/requests/requests';
import { queryKeys } from '~/utils/queryKeys';

/**
 * The request inbox. `includeDecided` also returns everything already approved or rejected, which
 * grows for as long as the organization exists - the caller decides whether it wants that.
 */
export function useRequests(options?: {
  includeDecided?: Ref<boolean>
  enabled?: Ref<boolean>
}) {
  const auth = useAuthStore();
  const includeDecided = computed(() => options?.includeDecided?.value ?? false);

  return useQuery({
    queryKey: computed(() => queryKeys.requests.list(includeDecided.value)),
    queryFn: () => getApiRequests({ includeDecided: includeDecided.value }),
    enabled: computed(() =>
      (options?.enabled?.value ?? true) &&
      auth.isAuthenticated &&
      auth.hasOrganization,
    ),
  });
}
