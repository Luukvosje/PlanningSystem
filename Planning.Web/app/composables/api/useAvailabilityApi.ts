import { useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  deleteAvailability,
  upsertDayPartAvailability,
  upsertTimeBlockAvailability,
} from '~/utils/availabilityClient'
import { queryKeys } from '~/utils/queryKeys'
import type { UpsertDayPartRequest, UpsertTimeBlockRequest } from '~/types/availability'

export function useAvailabilityApi() {
  const queryClient = useQueryClient()
  const toast = useToast()

  function invalidateAvailability() {
    return queryClient.invalidateQueries({ queryKey: ['availability'] })
  }

  const upsertDayPart = useMutation({
    mutationFn: (request: UpsertDayPartRequest) => upsertDayPartAvailability(request),
    onSuccess: async () => {
      await invalidateAvailability()
    },
    onError: () => {
      toast.add({ title: 'Beschikbaarheid opslaan mislukt', color: 'error' })
    },
  })

  const upsertTimeBlock = useMutation({
    mutationFn: (request: UpsertTimeBlockRequest) => upsertTimeBlockAvailability(request),
    onSuccess: async () => {
      await invalidateAvailability()
    },
    onError: () => {
      toast.add({ title: 'Tijdsblok opslaan mislukt', color: 'error' })
    },
  })

  const remove = useMutation({
    mutationFn: (id: string) => deleteAvailability(id),
    onSuccess: async () => {
      await invalidateAvailability()
    },
    onError: () => {
      toast.add({ title: 'Tijdsblok verwijderen mislukt', color: 'error' })
    },
  })

  return {
    upsertDayPart,
    upsertTimeBlock,
    remove,
    invalidateAvailability,
  }
}
