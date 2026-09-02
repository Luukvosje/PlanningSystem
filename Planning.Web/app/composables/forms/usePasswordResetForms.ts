import { createForgotPasswordSchema, createResetPasswordSchema } from '~/schemas/auth.schema';

/**
 * Asks for a reset link. Always reports the same thing afterwards, whether or not the address
 * belongs to an account - the API answers identically on purpose, and saying "unknown email"
 * here would give away exactly what it refuses to.
 */
export function useForgotPasswordForm() {
  const api = useAuthApi();
  const { t } = useI18n();

  const submitted = ref(false);

  const form = useForm({
    schema: createForgotPasswordSchema(t),
    initialState: { email: '' },
    controls: computed(() => [
      {
        name: 'email',
        label: t('auth.email'),
        type: 'email',
        required: true,
        props: { autocomplete: 'email' },
      },
    ]),
    submit: computed(() => ({ label: t('auth.sendResetLink'), block: true })),
    onSubmit: async (data) => {
      await api.forgotPassword({ email: data.email });
      submitted.value = true;
    },
  });

  return { form, submitted };
}

/**
 * Sets the new password. The token comes from the link in the mail; without one there is
 * nothing to do here, so the page says so rather than showing a form that cannot succeed.
 */
export function useResetPasswordForm(token: Ref<string>) {
  const api = useAuthApi();
  const router = useRouter();
  const toast = useToast();
  const { t } = useI18n();

  const form = useForm({
    schema: createResetPasswordSchema(t),
    initialState: { password: '', confirmPassword: '' },
    controls: computed(() => [
      {
        name: 'password',
        label: t('auth.newPassword'),
        type: 'password',
        required: true,
        props: { autocomplete: 'new-password' },
      },
      {
        name: 'confirmPassword',
        label: t('auth.confirmPassword'),
        type: 'password',
        required: true,
        props: { autocomplete: 'new-password' },
      },
    ]),
    submit: computed(() => ({ label: t('auth.setNewPassword'), block: true })),
    onSubmit: async (data) => {
      await api.resetPassword({ token: token.value, newPassword: data.password });

      toast.add({
        title: t('auth.passwordChanged'),
        description: t('auth.passwordChangedDescription'),
        color: 'success',
      });

      await router.push('/login');
    },
  });

  return form;
}
