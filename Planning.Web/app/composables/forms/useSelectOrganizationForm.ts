import { useQueryClient } from '@tanstack/vue-query';
import { createSelectOrganizationSchema } from '~/schemas/auth.schema';

/**
 * Picking which organization to enter, for an account that belongs to several.
 *
 * The membership watcher defends against landing here directly (a stale bookmark, say): with
 * zero memberships there is nothing to choose from, with one there is nothing to choose either.
 */
export function useSelectOrganizationForm() {
  const auth = useAuthStore();
  const router = useRouter();
  const route = useRoute();
  const queryClient = useQueryClient();
  const { t } = useI18n();

  const { data: memberships, isLoading } = useMyOrganizations();

  const orgOptions = computed(() =>
    (memberships.value ?? []).map((m) => ({
      label: m.organizationName ?? t('common.unknown'),
      value: m.organizationId ?? '',
    })),
  );

  async function selectAndContinue(organizationId: string) {
    auth.selectOrganization(organizationId);
    await auth.fetchMe();
    await queryClient.invalidateQueries();
    await router.push((route.query.redirect as string | undefined) ?? '/dashboard');
  }

  watch(memberships, async (list) => {
    if (!list) {
      return;
    }

    if (list.length === 0) {
      await router.replace('/organizations/new');
    } else if (list.length === 1 && list[0]?.organizationId) {
      await selectAndContinue(list[0].organizationId);
    }
  }, { immediate: true });

  const form = useForm({
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
      await selectAndContinue(data.organizationId);
    },
  });

  return { form, orgOptions, isLoading };
}
