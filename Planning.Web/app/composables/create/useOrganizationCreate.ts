import { useQueryClient } from '@tanstack/vue-query'
import { useCreate } from '~/lib/form/useCreate'
import { organizationSchema } from '~/schemas/organization.schema'

const ORGANIZATION_CREATE_KEY = Symbol('organization-create')

export function useOrganizationCreate() {
  const auth = useAuthStore()
  const orgsApi = useOrganizationsApi()
  const queryClient = useQueryClient()
  const toast = useToast()

  return useCreate(ORGANIZATION_CREATE_KEY, {
    title: 'Organisatie aanmaken',
    description: 'Maak een nieuwe organisatie aan.',
    schema: organizationSchema,
    initialState: { name: '', email: '' },
    controls: [
      { name: 'name', label: 'Naam', type: 'input', required: true },
      { name: 'email', label: 'E-mail organisatie', type: 'email', required: true },
    ],
    submitLabel: 'Organisatie aanmaken',
    onSubmit: async (data) => {
      const response = await orgsApi.create(data)

      if (response.organization?.id) {
        auth.setOrganizationId(response.organization.id)
      }
      await auth.fetchMe()
      await queryClient.invalidateQueries()

      toast.add({ title: 'Organisatie aangemaakt', color: 'success' })
    },
  })
}
