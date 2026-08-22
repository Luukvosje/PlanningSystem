import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { putApiOrganizationsCurrentModules } from '~/generated/api/organizations/organizations';
import { queryKeys } from '~/utils/queryKeys';

export function useModulesApi() {
  const queryClient = useQueryClient();
  const auth = useAuthStore();
  const toast = useToast();
  const { t } = useI18n();

  const updateOrganizationModules = useMutation({
    mutationFn: putApiOrganizationsCurrentModules,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current }),
        queryClient.invalidateQueries({ queryKey: queryKeys.auth.me }),
        invalidateOrgScopedQueries(queryClient),
      ]);
      await auth.fetchMe();
      toast.add({ title: t('organizations.toast.modulesUpdated'), color: 'success' });
    },
    onError: (error) => {
      const { message } = useApiError(error);
      toast.add({ title: message.value, color: 'error' });
    },
  });

  return {
    updateOrganizationModules,
  };
}
