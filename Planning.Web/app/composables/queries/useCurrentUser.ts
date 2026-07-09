import { useQuery } from '@tanstack/vue-query'
import { getApiAuthMe } from '~/generated/api/auth/auth'
import { queryKeys } from '~/utils/queryKeys'

export function useCurrentUser() {
  const auth = useAuthStore()

  return useQuery({
    queryKey: computed(() => [...queryKeys.auth.me, auth.organizationId] as const),
    queryFn: () => getApiAuthMe(),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
  })
}
