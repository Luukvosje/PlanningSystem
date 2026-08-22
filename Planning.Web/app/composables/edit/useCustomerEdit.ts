import { useQueryClient } from '@tanstack/vue-query';
import type { CustomerResponse } from '~/generated/models';
import { useEdit } from '~/lib/form/useEdit';
import { createCustomerSchema } from '~/schemas/customer.schema';
import { queryKeys } from '~/utils/queryKeys';

const CUSTOMER_EDIT_KEY = Symbol('customer-edit');

function customerToState(customer: CustomerResponse) {
  return {
    name: customer.name ?? '',
    email: customer.email ?? '',
    address: customer.address ?? '',
  };
}

export function useCustomerEdit() {
  const customersApi = useCustomersApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  return useEdit(CUSTOMER_EDIT_KEY, {
    schema: createCustomerSchema(t),
    controls: computed(() => [
      { name: 'name', label: t('customers.fields.name'), type: 'input', required: true },
      { name: 'email', label: t('customers.fields.email'), type: 'email', required: true },
      { name: 'address', label: t('customers.fields.address'), type: 'textarea', props: { rows: 3 } },
    ]),
    toState: customerToState,
    onSubmit: async (customer, data) => {
      if (!customer.id) {
        return;
      }

      await customersApi.update(customer.id, {
        name: data.name,
        email: data.email,
        address: data.address || null,
      });

      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });
      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(customer.id) });

      toast.add({ title: t('customers.updated'), color: 'success' });
    },
  });
}
