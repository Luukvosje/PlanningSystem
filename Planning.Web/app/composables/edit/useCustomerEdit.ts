import { useQueryClient } from '@tanstack/vue-query';
import UiColorPicker from '~/components/ui/ColorPicker.vue';
import type { CustomerResponse } from '~/generated/models';
import { useEdit } from '~/lib/form/useEdit';
import { createCustomerSchema } from '~/schemas/customer.schema';
import { DEFAULT_PLANNING_COLOR } from '~/types/planning';
import { queryKeys } from '~/utils/queryKeys';

const CUSTOMER_EDIT_KEY = Symbol('customer-edit');

function customerToState(customer: CustomerResponse) {
  return {
    name: customer.name ?? '',
    email: customer.email ?? '',
    address: customer.address ?? '',
    color: customer.color || DEFAULT_PLANNING_COLOR,
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
      {
        name: 'color',
        label: t('customers.fields.color'),
        description: t('customers.colorDescription'),
        component: UiColorPicker,
        display: (value) => (typeof value === 'string' ? value : null),
      },
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
        color: data.color,
      });

      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });
      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(customer.id) });

      toast.add({ title: t('customers.updated'), color: 'success' });
    },
  });
}
