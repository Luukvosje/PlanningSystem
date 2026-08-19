import { useMutation, useQueryClient } from '@tanstack/vue-query';
import {
  postApiOrganizationsCurrentLogo,
  putApiOrganizationsCurrent,
  putApiOrganizationsCurrentPlanningSettings,
} from '~/generated/api/organizations/organizations';
import type {
  UpdateOrganizationPlanningSettingsRequest,
  UpdateOrganizationRequest,
} from '~/generated/models';
import { queryKeys } from '~/utils/queryKeys';

export function useOrganizationSettingsApi() {
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  const updateOrganization = useMutation({
    mutationFn: (request: UpdateOrganizationRequest) => putApiOrganizationsCurrent(request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current }),
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.mine }),
      ]);
      toast.add({ title: t('organizations.toast.updated'), color: 'success' });
    },
    onError: (error) => {
      const { message } = useApiError(error);
      toast.add({ title: message.value, color: 'error' });
    },
  });

  const updatePlanningSettings = useMutation({
    mutationFn: (request: UpdateOrganizationPlanningSettingsRequest) =>
      putApiOrganizationsCurrentPlanningSettings(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current });
      toast.add({ title: t('organizations.toast.planningSettingsSaved'), color: 'success' });
    },
    onError: (error) => {
      const { message } = useApiError(error);
      toast.add({ title: message.value, color: 'error' });
    },
  });

  const uploadLogo = useMutation({
    mutationFn: (file: File) => postApiOrganizationsCurrentLogo({ file }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current });
      toast.add({ title: t('organizations.toast.logoSaved'), color: 'success' });
    },
    onError: (error) => {
      const { message } = useApiError(error);
      toast.add({ title: message.value, color: 'error' });
    },
  });

  return {
    updateOrganization,
    updatePlanningSettings,
    uploadLogo,
  };
}
