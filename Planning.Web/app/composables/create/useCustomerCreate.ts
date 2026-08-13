import { useQueryClient } from '@tanstack/vue-query';
import { useCreate } from '~/lib/form/useCreate';
import { createCustomerSchema } from '~/schemas/customer.schema';
import { queryKeys } from '~/utils/queryKeys';

const CUSTOMER_CREATE_KEY = Symbol('customer-create');

export function useCustomerCreate() {
  const customersApi = useCustomersApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const router = useRouter();
  const { t } = useI18n();

  return useCreate(CUSTOMER_CREATE_KEY, {
    title: computed(() => t('customers.create.title')),
    description: computed(() => t('customers.create.description')),
    schema: createCustomerSchema(t),
    initialState: { name: '', email: '', address: '' },
    controls: computed(() => [
      { name: 'name', label: t('customers.fields.name'), type: 'input', required: true },
      { name: 'email', label: t('customers.fields.email'), type: 'email', required: true },
      { name: 'address', label: t('customers.fields.address'), type: 'textarea', props: { rows: 3 } },
    ]),
    submitLabel: computed(() => t('customers.create.submit')),
    validateOn: ['input', 'blur', 'change'],
    onSubmit: async (data) => {
      const customer = await customersApi.create({
        name: data.name,
        email: data.email,
        address: data.address || null,
      });

      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });

      toast.add({ title: t('customers.created'), color: 'success' });

      if (customer.id) {
        await router.push(`/customers/${customer.id}`);
      }
    },
  });
}
