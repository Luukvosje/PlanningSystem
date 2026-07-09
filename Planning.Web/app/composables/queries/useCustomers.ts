import { useQuery } from '@tanstack/vue-query'
import { getApiCustomers } from '~/generated/api/customers/customers'
import { queryKeys } from '~/utils/queryKeys'

export function useCustomers() {
  const auth = useAuthStore()

  return useQuery({
    queryKey: queryKeys.customers.all,
    queryFn: () => getApiCustomers(),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
  })
}