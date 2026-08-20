import { createOrganizationSchema } from '~/schemas/organization.schema';

export function useOrganizationSettingsForm() {
  const { updateOrganization } = useOrganizationSettingsApi();
  const { data: organization } = useCurrentOrganization();
  const { t } = useI18n();

  const form = useForm({
    schema: createOrganizationSchema(t),
    initialState: {
      name: '',
      email: '',
    },
    controls: computed(() => [
      {
        name: 'name',
        label: t('organizations.fields.name'),
        type: 'input',
        required: true,
      },
      {
        name: 'email',
        label: t('organizations.fields.email'),
        type: 'email',
        required: true,
        props: { autocomplete: 'email' },
      },
    ]),
    grid: true,
    onSubmit: async (data) => {
      await updateOrganization.mutateAsync(data);
    },
  });

  watch(
    organization,
    (org) => {
      if (!org) {
        return;
      }

      form.state.name = org.name ?? '';
      form.state.email = org.email ?? '';
      form.markClean();
    },
    { immediate: true },
  );

  return form;
}
