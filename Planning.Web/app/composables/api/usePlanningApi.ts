import { useMutation, useQueryClient } from '@tanstack/vue-query';
import {
  deleteApiPlanningId,
  getApiPlanning,
  getApiPlanningId,
  patchApiPlanningIdConfirm,
  patchApiPlanningIdMove,
  postApiPlanning,
  postApiPlanningIdDuplicate,
  putApiPlanningId,
} from '~/generated/api/planning/planning';
import type { GetApiPlanningParams } from '~/generated/models';
import type {
  CreatePlanningRequest,
  DuplicatePlanningRequest,
  MovePlanningRequest,
  UpdatePlanningRequest,
} from '~/types/planning';

export function usePlanningApi() {
  const queryClient = useQueryClient();

  function invalidatePlanning() {
    return queryClient.invalidateQueries({ queryKey: ['planning'] });
  }

  return {
    getList: (params: GetApiPlanningParams) => getApiPlanning(params),
    getById: (id: string) => getApiPlanningId(id),
    create: (request: CreatePlanningRequest) => postApiPlanning(request),
    update: (id: string, request: UpdatePlanningRequest) => putApiPlanningId(id, request),
    move: (id: string, request: MovePlanningRequest) => patchApiPlanningIdMove(id, request),
    confirm: (id: string) => patchApiPlanningIdConfirm(id),
    duplicate: (id: string, request?: DuplicatePlanningRequest) =>
      postApiPlanningIdDuplicate(id, request ?? {}),
    delete: (id: string) => deleteApiPlanningId(id),
    invalidatePlanning,

    useCreateMutation: () =>
      useMutation({
        mutationFn: (request: CreatePlanningRequest) => postApiPlanning(request),
        onSuccess: invalidatePlanning,
      }),

    useUpdateMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request: UpdatePlanningRequest }) =>
          putApiPlanningId(id, request),
        onSuccess: invalidatePlanning,
      }),

    useMoveMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request: MovePlanningRequest }) =>
          patchApiPlanningIdMove(id, request),
        onSuccess: invalidatePlanning,
      }),

    useDeleteMutation: () =>
      useMutation({
        mutationFn: (id: string) => deleteApiPlanningId(id),
        onSuccess: invalidatePlanning,
      }),

    useConfirmMutation: () =>
      useMutation({
        mutationFn: (id: string) => patchApiPlanningIdConfirm(id),
        onSuccess: invalidatePlanning,
      }),

    useDuplicateMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request?: DuplicatePlanningRequest }) =>
          postApiPlanningIdDuplicate(id, request ?? {}),
        onSuccess: invalidatePlanning,
      }),
  };
}
