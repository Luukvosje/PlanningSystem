import { createRegisterSchema } from '~/schemas/auth.schema';

export function useRegisterForm() {
  const auth = useAuthStore();
  const router = useRouter();
  const route = useRoute();
  const toast = useToast();
  const { t } = useI18n();

  return useForm({
    schema: createRegisterSchema(t),
    initialState: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
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
      {
        name: 'password',
        label: t('auth.password'),
        type: 'password',
        required: true,
        props: { autocomplete: 'new-password' },
      },
    ]),
    submit: computed(() => ({ label: t('auth.register'), block: true })),
    onSubmit: async (data) => {
      await auth.register(data);

      toast.add({ title: t('auth.accountCreated'), description: t('auth.accountCreatedDescription'), color: 'success' });

      const redirect = route.query.redirect as string | undefined;
      const inviteCode = route.query.code as string | undefined;
      await router.push({
        path: '/login',
        query: {
          ...(redirect ? { redirect } : {}),
          ...(inviteCode ? { code: inviteCode } : {}),
        },
      });
    },
  });
}
