import { useQuery } from '@tanstack/vue-query';
import { getApiOrganizationsMine } from '~/generated/api/organizations/organizations';
import { queryKeys } from '~/utils/queryKeys';

export function useMyOrganizations() {
  const auth = useAuthStore();

  return useQuery({
    queryKey: queryKeys.organizations.mine,
    queryFn: () => getApiOrganizationsMine(),
    enabled: computed(() => auth.isAuthenticated),
  });
}
