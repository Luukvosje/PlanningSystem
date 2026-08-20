import { createLoginCredentialsSchema, createSelectOrganizationSchema } from '~/schemas/auth.schema';
import type { OrganizationMembership } from '~/types/api-error';

/**
 * The two steps of signing in: credentials, and — only for an account that belongs to more
 * than one organization — picking which one to enter.
 */
export function useLoginForms() {
  const auth = useAuthStore();
  const router = useRouter();
  const route = useRoute();
  const { t } = useI18n();

  const memberships = ref<OrganizationMembership[]>([]);
  const showOrgPicker = ref(false);

  function defaultPath(inviteCode: string | undefined) {
    if (auth.hasOrganization) {
      return '/dashboard';
    }

    return inviteCode ? '/join' : '/organizations/new';
  }

  async function navigateAfterLogin() {
    const redirect = route.query.redirect as string | undefined;
    const inviteCode = route.query.code as string | undefined;
    const target = redirect ?? defaultPath(inviteCode);

    await router.push(inviteCode && target === '/join' ?
      { path: target, query: { code: inviteCode } } :
      target);
  }

  const credentialsForm = useForm({
    schema: createLoginCredentialsSchema(t),
    initialState: { email: '', password: '' },
    controls: computed(() => [
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
        props: { autocomplete: 'current-password' },
      },
    ]),
    submit: computed(() => ({ label: t('auth.login'), block: true })),
    onSubmit: async (data) => {
      const result = await auth.login(data);

      if (result.requiresOrganizationSelection && result.memberships?.length) {
        memberships.value = result.memberships;
        orgForm.reset({
          organizationId: result.memberships[0]?.organizationId ?? '',
        });
        showOrgPicker.value = true;
        return;
      }

      await auth.fetchMe();
      await navigateAfterLogin();
    },
  });

  const orgOptions = computed(() =>
    memberships.value.map((m) => ({
      label: m.organizationName ?? t('common.unknown'),
      value: m.organizationId,
    })),
  );

  const orgForm = useForm({
    schema: createSelectOrganizationSchema(t),
    initialState: { organizationId: '' },
    controls: computed(() => [
      {
        name: 'organizationId',
        label: t('nav.organization'),
        type: 'select',
        required: true,
        props: { items: orgOptions.value },
      },
    ]),
    submit: computed(() => ({ label: t('auth.continue'), block: true })),
    onSubmit: async (data) => {
      await auth.selectOrganization(data.organizationId);
      await auth.fetchMe();
      await navigateAfterLogin();
    },
  });

  return { credentialsForm, orgForm, showOrgPicker };
}
