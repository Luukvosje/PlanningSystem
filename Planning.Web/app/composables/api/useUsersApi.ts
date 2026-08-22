import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { getApiUsersId, putApiUsersIdModules, putApiUsersIdRole } from '~/generated/api/users/users';
import type { UpdateModulesRequest, UpdateUserRoleRequest } from '~/generated/models';
import { queryKeys } from '~/utils/queryKeys';

export function useUsersApi() {
  const queryClient = useQueryClient();

  return {
    getById: (id: string) => getApiUsersId(id),

    updateRole: (id: string, request: UpdateUserRoleRequest) =>
      putApiUsersIdRole(id, request),

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
  };
}
