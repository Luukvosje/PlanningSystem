import { useQuery } from '@tanstack/vue-query';
import { getApiUsers } from '~/generated/api/users/users';
import { queryKeys } from '~/utils/queryKeys';

export function useUsers() {
  const auth = useAuthStore();

  return useQuery({
    queryKey: queryKeys.users.all,
    queryFn: () => getApiUsers(),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
  });
}
