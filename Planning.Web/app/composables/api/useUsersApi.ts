import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { getApiUsersId, putApiUsersIdModules, putApiUsersIdRole, putApiUsersIdStatus } from '~/generated/api/users/users';
import type { UpdateModulesRequest, UpdateUserRoleRequest, UpdateUserStatusRequest } from '~/generated/models';
import { queryKeys } from '~/utils/queryKeys';

export function useUsersApi() {
  const queryClient = useQueryClient();

  return {
    getById: (id: string) => getApiUsersId(id),

    updateRole: (id: string, request: UpdateUserRoleRequest) =>
      putApiUsersIdRole(id, request),

    updateStatus: (id: string, request: UpdateUserStatusRequest) =>
      putApiUsersIdStatus(id, request),

    updateModules: (id: string, request: UpdateModulesRequest) =>
      putApiUsersIdModules(id, request),

    useUpdateRoleMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request: UpdateUserRoleRequest }) =>
          putApiUsersIdRole(id, request),
        onSuccess: (_, { id }) => {
          queryClient.invalidateQueries({ queryKey: queryKeys.users.all });
          queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(id) });
        },
      }),

    useUpdateStatusMutation: () =>
      useMutation({
        mutationFn: ({ id, request }: { id: string, request: UpdateUserStatusRequest }) =>
          putApiUsersIdStatus(id, request),
        onSuccess: (_, { id }) => {
          queryClient.invalidateQueries({ queryKey: queryKeys.users.all });
          queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(id) });
        },
      }),
  };
}
