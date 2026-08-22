import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { putApiAuthMe } from '~/generated/api/auth/auth';
import type { UpdateProfileRequest } from '~/generated/models';
import { queryKeys } from '~/utils/queryKeys';

export function useProfileApi() {
  const auth = useAuthStore();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  const updateProfile = useMutation({
    mutationFn: (request: UpdateProfileRequest) => putApiAuthMe(request),
    onSuccess: async (response) => {
      if (auth.currentUser) {
        auth.currentUser = {
          ...auth.currentUser,
          email: response.email ?? auth.currentUser.email,
          firstName: response.firstName ?? auth.currentUser.firstName,
          lastName: response.lastName ?? auth.currentUser.lastName,
        };
      }

      await queryClient.invalidateQueries({ queryKey: queryKeys.auth.me });
      toast.add({ title: t('settings.toast.profileUpdated'), color: 'success' });
    },
    onError: (error) => {
      const { message } = useApiError(error);
      toast.add({ title: message.value, color: 'error' });
    },
  });

  return {
    updateProfile,
  };
}
