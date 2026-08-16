import { useQuery } from '@tanstack/vue-query';
import { getApiAuthMe } from '~/generated/api/auth/auth';
import { queryKeys } from '~/utils/queryKeys';

export function useCurrentUser() {
  const auth = useAuthStore();

  return useQuery({
    queryKey: computed(() => [...queryKeys.auth.me, auth.organizationId] as const),
    queryFn: () => getApiAuthMe(),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
    // customFetch already retries once after a silent token refresh; a
    // second, vue-query-scheduled retry on top of that happens far enough
    // removed from the original request that Nuxt composables it needs
    // (useAuthStore, useNuxtApp) can resolve against a stale SSR context.
    retry: false,
  });
}
