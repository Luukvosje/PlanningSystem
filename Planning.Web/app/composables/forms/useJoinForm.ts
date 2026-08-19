import { useQueryClient } from '@tanstack/vue-query';
import InvitesCodeInput from '~/components/invites/CodeInput.vue';
import { createAcceptInviteSchema } from '~/schemas/invite.schema';

/**
 * Accepting an invite code. Unauthenticated visitors are sent to the login page with the code
 * in the query, so they land back here with it prefilled.
 */
export function useJoinForm() {
  const auth = useAuthStore();
  const invitesApi = useInvitesApi();
  const router = useRouter();
  const route = useRoute();
  const toast = useToast();
  const queryClient = useQueryClient();
  const { t } = useI18n();

  return useForm({
    schema: createAcceptInviteSchema(t),
    initialState: { code: ((route.query.code as string) ?? '').toUpperCase() },
    controls: computed(() => [
      {
        name: 'code',
        label: t('invites.code'),
        required: true,
        component: InvitesCodeInput,
      },
    ]),
    submit: computed(() => ({
      label: auth.isAuthenticated ? t('invites.join') : t('invites.loginToJoin'),
      block: true,
    })),
    onSubmit: async (data) => {
      if (!auth.isAuthenticated) {
        await router.push({ path: '/login', query: { redirect: '/join', code: data.code } });
        return;
      }

      const response = await invitesApi.accept({ code: data.code.toUpperCase() });
      auth.setOrganizationId(response.organizationId);
      await auth.fetchMe();
      await queryClient.invalidateQueries();

      toast.add({ title: t('invites.accepted'), color: 'success' });
      await router.push('/users');
    },
  });
}
