import { useQueryClient } from '@tanstack/vue-query'
import { useCreate } from '~/lib/form/useCreate'
import { customerSchema } from '~/schemas/customer.schema'
import { queryKeys } from '~/utils/queryKeys'

const CUSTOMER_CREATE_KEY = Symbol('customer-create')

export function useCustomerCreate() {
  const customersApi = useCustomersApi()
  const queryClient = useQueryClient()
  const toast = useToast()
  const router = useRouter()

  return useCreate(CUSTOMER_CREATE_KEY, {
    title: 'Nieuwe klant',
    description: 'Voeg een klant toe aan je organisatie.',
    schema: customerSchema,
    initialState: { name: '', email: '', address: '' },
    controls: [
      { name: 'name', label: 'Naam', type: 'input', required: true },
      { name: 'email', label: 'E-mail', type: 'email', required: true },
      { name: 'address', label: 'Adres', type: 'textarea', props: { rows: 3 } },
    ],
    submitLabel: 'Klant aanmaken',
    validateOn: ['input', 'blur', 'change'],
    onSubmit: async (data) => {
      const customer = await customersApi.create({
        name: data.name,
        email: data.email,
        address: data.address || null,
      })

      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all })

      toast.add({ title: 'Klant aangemaakt', color: 'success' })

      if (customer.id) {
        await router.push(`/customers/${customer.id}`)
      }
    },
  })
}
