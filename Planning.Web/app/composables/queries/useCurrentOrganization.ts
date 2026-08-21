import { useQuery } from '@tanstack/vue-query';
import { getApiOrganizationsCurrent } from '~/generated/api/organizations/organizations';
import { queryKeys } from '~/utils/queryKeys';

export function useCurrentOrganization() {
  const auth = useAuthStore();

  return useQuery({
    queryKey: computed(() => [...queryKeys.organizations.current, auth.organizationId] as const),
    queryFn: () => getApiOrganizationsCurrent(),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
  });
}
