import { useQuery } from '@tanstack/vue-query';
import { getApiInvitesCode } from '~/generated/api/invites/invites';
import { queryKeys } from '~/utils/queryKeys';

export function useInvitePreview(code: Ref<string> | string) {
  const inviteCode = isRef(code) ? code : ref(code);
  const auth = useAuthStore();

  return useQuery({
    queryKey: computed(() => queryKeys.invites.preview(inviteCode.value)),
    queryFn: () => getApiInvitesCode(inviteCode.value),
    enabled: computed(() => !!inviteCode.value && auth.isAuthenticated),
  });
}
