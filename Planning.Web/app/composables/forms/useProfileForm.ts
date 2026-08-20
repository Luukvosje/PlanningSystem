import { createUpdateProfileSchema } from '~/schemas/auth.schema';

export function useProfileForm() {
  const auth = useAuthStore();
  const { updateProfile } = useProfileApi();
  const { t } = useI18n();

  const form = useForm({
    schema: createUpdateProfileSchema(t),
    initialState: {
      firstName: auth.currentUser?.firstName ?? '',
      lastName: auth.currentUser?.lastName ?? '',
      email: auth.currentUser?.email ?? '',
    },
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
        hidden: true,
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
    grid: true,
    onSubmit: async (data) => {
      await updateProfile.mutateAsync(data);
    },
  });

  watch(
    () => auth.currentUser,
    (user) => {
      if (!user) {
        return;
      }

      form.state.firstName = user.firstName ?? '';
      form.state.lastName = user.lastName ?? '';
      form.state.email = user.email ?? '';
      form.markClean();
    },
    { immediate: true },
  );

  return form;
}
