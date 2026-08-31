import { useQueryClient } from '@tanstack/vue-query';
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
  const queryClient = useQueryClient();
  const { t } = useI18n();

  const memberships = ref<OrganizationMembership[]>([]);
  const showOrgPicker = ref(false);

  async function navigateAfterLogin() {
    const redirect = route.query.redirect as string | undefined;
    const inviteCode = route.query.code as string | undefined;

    if (redirect) {
      await router.push(redirect);
      return;
    }

    if (!auth.hasOrganization && inviteCode) {
      await router.push({ path: '/join', query: { code: inviteCode } });
      return;
    }

    // Where an account without a current organization belongs is one decision, and
    // resolveOrganizationTarget owns it. This page used to answer it a second time with a
    // hardcoded /organization/new, which is how a deactivated member ended up being asked to
    // start an organization.
    await router.push(auth.hasOrganization ?
      '/dashboard' :
      await resolveOrganizationTarget('/dashboard', auth, queryClient));
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

      // Nothing cached belongs to the session that just started, and some of it - the
      // membership list this very navigation reads - would otherwise be answered from the
      // previous account's data in a tab that never reloaded.
      queryClient.clear();

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
