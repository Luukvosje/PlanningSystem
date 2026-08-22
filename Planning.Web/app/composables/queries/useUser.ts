import { useQuery } from '@tanstack/vue-query';
import { getApiUsersId } from '~/generated/api/users/users';
import { queryKeys } from '~/utils/queryKeys';

export function useUser(id: Ref<string> | string) {
  const userId = isRef(id) ? id : ref(id);
  const auth = useAuthStore();

  return useQuery({
    queryKey: computed(() => queryKeys.users.detail(userId.value)),
    queryFn: () => getApiUsersId(userId.value),
    enabled: computed(() => !!userId.value && auth.isAuthenticated && auth.hasOrganization),
  });
}
