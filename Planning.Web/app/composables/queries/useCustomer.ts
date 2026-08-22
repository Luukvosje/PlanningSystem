import { useQuery } from '@tanstack/vue-query';

import { getApiCustomersId } from '~/generated/api/customers/customers';

import { queryKeys } from '~/utils/queryKeys';



export function useCustomer(id: Ref<string> | string) {

  const customerId = isRef(id) ? id : ref(id);

  const auth = useAuthStore();



  return useQuery({

    queryKey: computed(() => queryKeys.customers.detail(customerId.value)),

    queryFn: () => getApiCustomersId(customerId.value),

    enabled: computed(() => !!customerId.value && auth.isAuthenticated && auth.hasOrganization),

  });

}

