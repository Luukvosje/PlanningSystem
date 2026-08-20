import type { CurrentUserResponse } from '~/generated/models';
import { useEdit } from '~/lib/form/useEdit';
import { createUpdateProfileSchema } from '~/schemas/auth.schema';

const PROFILE_EDIT_KEY = Symbol('profile-edit');

export function useProfileEdit() {
  const { updateProfile } = useProfileApi();
  const { t } = useI18n();

  return useEdit(PROFILE_EDIT_KEY, {
    title: computed(() => t('settings.edit.title')),
    description: computed(() => t('settings.edit.description')),
    schema: createUpdateProfileSchema(t),
    controls: computed(() => [
      {
        name: 'firstName',
        label: t('auth.firstName'),
        type: 'input',
        required: true,
        props: { autocomplete: 'given-name' },
      },
      {
        name: 'lastName',
        label: t('auth.lastName'),
        type: 'input',
        required: true,
        props: { autocomplete: 'family-name' },
      },
      {
        name: 'email',
        label: t('auth.email'),
        type: 'email',
        required: true,
        props: { autocomplete: 'email' },
      },
    ]),
    toState: (user: CurrentUserResponse) => ({
      firstName: user.firstName ?? '',
      lastName: user.lastName ?? '',
      email: user.email ?? '',
    }),
    onSubmit: async (_user, data) => {
      await updateProfile.mutateAsync(data);
    },
  });
}
