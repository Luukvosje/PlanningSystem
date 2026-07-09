import { useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  deleteApiCustomersId,
  getApiCustomers,
  getApiCustomersId,
  postApiCustomers,
  putApiCustomersId,
} from '~/generated/api/customers/customers'
import type { CreateCustomerRequest, UpdateCustomerRequest } from '~/generated/models'
import { queryKeys } from '~/utils/queryKeys'

export function useCustomersApi() {
  const queryClient = useQueryClient()

  return {
    getById: (id: string) => getApiCustomersId(id),

    getAll: () => getApiCustomers(),

    create: (request: CreateCustomerRequest) => postApiCustomers(request),

    update: (id: string, request: UpdateCustomerRequest) =>
      putApiCustomersId(id, request),

    delete: (id: string) => deleteApiCustomersId(id),

    useDeleteMutation: () =>
      useMutation({
        mutationFn: (id: string) => deleteApiCustomersId(id),
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: queryKeys.customers.all })
        },
      }),
  }
}
