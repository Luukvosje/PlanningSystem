import FormCreateInviteExtra from '~/components/form/create/InviteExtra.vue';
import type { UserResponse } from '~/generated/models';
import { useCreate } from '~/lib/form/useCreate';
import { createInviteSchema } from '~/schemas/invite.schema';

const INVITE_CREATE_KEY = Symbol('invite-create');

/**
 * Two kinds of invite share this modal: an open code that makes whoever accepts it a new member,
 * and - via `openFor` - one for a member who was added by name, which links their login to that
 * record instead.
 */
export function useInviteCreate() {
  const invitesApi = useInvitesApi();
  const toast = useToast();
  const { t } = useI18n();
  const createdInvite = ref<{ code: string, expiresAtUtc: string, emailSent: boolean } | null>(null);
  const targetUser = ref<UserResponse | null>(null);

  const create = useCreate(INVITE_CREATE_KEY, {
    title: computed(() => (targetUser.value ?
      t('invites.create.titleFor', { name: `${targetUser.value.firstName} ${targetUser.value.lastName}` }) :
      t('invites.create.title'))),
    description: computed(() => (targetUser.value ?
      t('invites.create.descriptionFor') :
      t('invites.create.description'))),
    schema: createInviteSchema(t),
    initialState: { email: '', userId: undefined },
    controls: computed(() => [
      {
        name: 'email',
        label: t('invites.create.emailLabel'),
        type: 'email',
        description: t('invites.create.emailHint'),
        props: { autocomplete: 'off' },
      },
    ]),
    submitLabel: computed(() => t('invites.create.submit')),
    footerMode: 'close-only',
    closeOnSuccess: false,
    bodyExtra: FormCreateInviteExtra,
    extensions: { createdInvite },
    onSubmit: async (data) => {
      const email = data.email?.trim() ? data.email.trim() : undefined;
      const response = await invitesApi.create({ email, userId: data.userId ?? undefined });

      createdInvite.value = {
        code: response.code ?? '',
        expiresAtUtc: response.expiresAtUtc ?? '',
        emailSent: response.emailSent ?? false,
      };

      // An address was given but the mail did not go out - say so, because the code on screen is
      // then the only way this invite reaches anyone.
      const mailFailed = email !== undefined && !response.emailSent;

      toast.add({
        title: mailFailed ? t('invites.emailFailed') : t('invites.created'),
        description: mailFailed ? t('invites.emailFailedDescription') : undefined,
        color: mailFailed ? 'warning' : 'success',
      });
    },
    onClose: () => {
      createdInvite.value = null;
      targetUser.value = null;
    },
  });

  function openFor(user: UserResponse) {
    targetUser.value = user;
    create.open({ email: user.email ?? '', userId: user.id });
  }

  return { ...create, openFor };
}
