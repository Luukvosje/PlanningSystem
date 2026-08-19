import { useQueryClient } from '@tanstack/vue-query';
import { createOrganizationSchema } from '~/schemas/organization.schema';

export function useCreateOrganizationForm() {
  const auth = useAuthStore();
  const orgsApi = useOrganizationsApi();
  const router = useRouter();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  return useForm({
    schema: createOrganizationSchema(t),
    initialState: { name: '', email: '' },
    controls: computed(() => [
      { name: 'name', label: t('organizations.fields.name'), type: 'input', required: true },
      { name: 'email', label: t('organizations.fields.email'), type: 'email', required: true },
    ]),
    submit: computed(() => ({ label: t('organizations.create.title'), block: true })),
    onSubmit: async (data) => {
      const response = await orgsApi.create(data);

      if (response.organization?.id) {
        auth.setOrganizationId(response.organization.id);
      }
      await auth.fetchMe();
      await queryClient.invalidateQueries();

      toast.add({ title: t('organizations.created'), color: 'success' });
      await router.push('/users');
    },
  });
}
