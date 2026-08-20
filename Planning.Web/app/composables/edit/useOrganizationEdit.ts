import type { OrganizationResponse } from '~/generated/models';
import { useEdit } from '~/lib/form/useEdit';
import { createOrganizationSchema } from '~/schemas/organization.schema';

const ORGANIZATION_EDIT_KEY = Symbol('organization-edit');

export function useOrganizationEdit() {
  const { updateOrganization } = useOrganizationSettingsApi();
  const { t } = useI18n();

  return useEdit(ORGANIZATION_EDIT_KEY, {
    title: computed(() => t('organizations.edit.title')),
    description: computed(() => t('organizations.edit.description')),
    schema: createOrganizationSchema(t),
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
    toState: (organization: OrganizationResponse) => ({
      name: organization.name ?? '',
      email: organization.email ?? '',
    }),
    onSubmit: async (_organization, data) => {
      await updateOrganization.mutateAsync(data);
    },
  });
}
