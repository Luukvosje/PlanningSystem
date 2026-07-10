import { useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  createAvailabilityRule,
  deleteAvailabilityRule,
  updateAvailabilityRule,
} from '~/utils/availabilityClient'
import type { CreateAvailabilityRuleRequest, UpdateAvailabilityRuleRequest } from '~/types/availability'

export function useAvailabilityApi() {
  const queryClient = useQueryClient()
  const toast = useToast()

  function invalidateAvailability() {
    return queryClient.invalidateQueries({ queryKey: ['availability'] })
  }

  const create = useMutation({
    mutationFn: (request: CreateAvailabilityRuleRequest) => createAvailabilityRule(request),
    onSuccess: async () => {
      await invalidateAvailability()
      toast.add({ title: 'Regel opgeslagen', color: 'success' })
    },
    onError: () => {
      toast.add({ title: 'Regel opslaan mislukt', color: 'error' })
    },
  })

  const update = useMutation({
    mutationFn: ({ id, request }: { id: string, request: UpdateAvailabilityRuleRequest }) =>
      updateAvailabilityRule(id, request),
    onSuccess: async () => {
      await invalidateAvailability()
      toast.add({ title: 'Regel bijgewerkt', color: 'success' })
    },
    onError: () => {
      toast.add({ title: 'Regel bijwerken mislukt', color: 'error' })
    },
  })

  const remove = useMutation({
    mutationFn: (id: string) => deleteAvailabilityRule(id),
    onSuccess: async () => {
      await invalidateAvailability()
      toast.add({ title: 'Regel verwijderd', color: 'success' })
    },
    onError: () => {
      toast.add({ title: 'Regel verwijderen mislukt', color: 'error' })
    },
  })

  return {
    create,
    update,
    remove,
    invalidateAvailability,
  }
}
