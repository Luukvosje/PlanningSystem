import { useQueryClient } from '@tanstack/vue-query';
import { useCreate } from '~/lib/form/useCreate';
import { createOrganizationSchema } from '~/schemas/organization.schema';

const ORGANIZATION_CREATE_KEY = Symbol('organization-create');

export function useOrganizationCreate() {
  const auth = useAuthStore();
  const orgsApi = useOrganizationsApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  return useCreate(ORGANIZATION_CREATE_KEY, {
    title: computed(() => t('organizations.create.title')),
    description: computed(() => t('organizations.create.description')),
    schema: createOrganizationSchema(t),
    initialState: { name: '', email: '' },
    controls: computed(() => [
      { name: 'name', label: t('organizations.fields.name'), type: 'input', required: true },
      { name: 'email', label: t('organizations.fields.email'), type: 'email', required: true },
    ]),
    submitLabel: computed(() => t('organizations.create.title')),
    onSubmit: async (data) => {
      const response = await orgsApi.create(data);

      if (response.organization?.id) {
        auth.setOrganizationId(response.organization.id);
      }
      await auth.fetchMe();
      await queryClient.invalidateQueries();

      toast.add({ title: t('organizations.created'), color: 'success' });
    },
  });
}
