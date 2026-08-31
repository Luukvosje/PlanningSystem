import { useQuery } from '@tanstack/vue-query';
import { getApiRequests } from '~/generated/api/requests/requests';
import { queryKeys } from '~/utils/queryKeys';

/**
 * The request inbox. Decided requests are only fetched once someone asks for them: the pending list
 * is short and stays short, the decided one grows for as long as the organization exists.
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
