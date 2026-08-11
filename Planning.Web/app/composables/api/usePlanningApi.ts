import { useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  confirmPlanning,
  createPlanning,
  deletePlanning,
  duplicatePlanning,
  getPlanningById,
  getPlanningList,
  movePlanning,
  updatePlanning,
} from '~/utils/planningClient'
import type {
  CreatePlanningRequest,
  DuplicatePlanningRequest,
  MovePlanningRequest,
  UpdatePlanningRequest,
} from '~/types/planning'
import { queryKeys } from '~/utils/queryKeys'

export function usePlanningApi() {
  const queryClient = useQueryClient()

  function invalidatePlanning() {
    return queryClient.invalidateQueries({ queryKey: ['planning'] })
  }

  return {
    getList: getPlanningList,
    getById: getPlanningById,
    create: (request: CreatePlanningRequest) => createPlanning(request),
    update: (id: string, request: UpdatePlanningRequest) => updatePlanning(id, request),
    move: (id: string, request: MovePlanningRequest) => movePlanning(id, request),
    confirm: (id: string) => confirmPlanning(id),
    duplicate: (id: string, request?: DuplicatePlanningRequest) => duplicatePlanning(id, request),
    delete: (id: string) => deletePlanning(id),
    invalidatePlanning,

    useCreateMutation: () =>
      useMutation({
        mutationFn: createPlanning,
        onSuccess: invalidatePlanning,
      }),

    useUpdateMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request: UpdatePlanningRequest }) =>
          updatePlanning(id, request),
        onSuccess: invalidatePlanning,
      }),

    useMoveMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request: MovePlanningRequest }) =>
          movePlanning(id, request),
        onSuccess: invalidatePlanning,
      }),

    useDeleteMutation: () =>
      useMutation({
        mutationFn: deletePlanning,
        onSuccess: invalidatePlanning,
      }),

    useConfirmMutation: () =>
      useMutation({
        mutationFn: confirmPlanning,
        onSuccess: invalidatePlanning,
      }),

    useDuplicateMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request?: DuplicatePlanningRequest }) =>
          duplicatePlanning(id, request),
        onSuccess: invalidatePlanning,
      }),
  }
}
