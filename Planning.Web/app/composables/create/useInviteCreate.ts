import FormCreateInviteExtra from '~/components/form/create/InviteExtra.vue';
import { useCreate } from '~/lib/form/useCreate';
import { createInviteSchema } from '~/schemas/invite.schema';

const INVITE_CREATE_KEY = Symbol('invite-create');

export function useInviteCreate() {
  const invitesApi = useInvitesApi();
  const toast = useToast();
  const { t } = useI18n();
  const createdInvite = ref<{ code: string, expiresAtUtc: string } | null>(null);

  return useCreate(INVITE_CREATE_KEY, {
    title: computed(() => t('invites.create.title')),
    description: computed(() => t('invites.create.description')),
    schema: createInviteSchema,
    initialState: {},
    controls: [],
    submitLabel: computed(() => t('invites.create.submit')),
    footerMode: 'close-only',
    closeOnSuccess: false,
    bodyExtra: FormCreateInviteExtra,
    extensions: { createdInvite },
    onSubmit: async () => {
      const response = await invitesApi.create({});

      createdInvite.value = {
        code: response.code ?? '',
        expiresAtUtc: response.expiresAtUtc ?? '',
      };

      toast.add({ title: t('invites.created'), color: 'success' });
    },
    onClose: () => {
      createdInvite.value = null;
    },
  });
}
