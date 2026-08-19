import { useQueryClient } from '@tanstack/vue-query';
import type { Ref } from 'vue';
import type { CustomerResponse } from '~/generated/models';
import { createCustomerSchema } from '~/schemas/customer.schema';
import { queryKeys } from '~/utils/queryKeys';

/**
 * Edit form for a single customer. Mirrors the loaded customer into form state, so the page
 * only decides when the form is visible.
 */
export function useCustomerForm(
  customer: Ref<CustomerResponse | undefined>,
  options: { onSaved?: () => void } = {},
) {
  const customersApi = useCustomersApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  const form = useForm({
    schema: createCustomerSchema(t),
    initialState: {
      name: '',
      email: '',
      address: '',
    },
    controls: computed(() => [
      { name: 'name', label: t('customers.fields.name'), type: 'input', required: true },
      { name: 'email', label: t('customers.fields.email'), type: 'email', required: true },
      { name: 'address', label: t('customers.fields.address'), type: 'textarea', props: { rows: 3 } },
    ]),
    grid: true,
    onSubmit: async (data) => {
      const id = customer.value?.id;

      if (!id) {
        return;
      }

      await customersApi.update(id, {
        name: data.name,
        email: data.email,
        address: data.address || null,
      });

      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });
      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(id) });

      toast.add({ title: t('customers.updated'), color: 'success' });
      options.onSaved?.();
    },
  });

  watch(
    customer,
    (value) => {
      if (!value) {
        return;
      }

      form.state.name = value.name ?? '';
      form.state.email = value.email ?? '';
      form.state.address = value.address ?? '';
      form.markClean();
    },
    { immediate: true },
  );

  return form;
}
