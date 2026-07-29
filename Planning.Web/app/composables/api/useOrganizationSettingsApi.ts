import { useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  putApiOrganizationsCurrent,
  putApiOrganizationsCurrentPlanningSettings,
} from '~/generated/api/organizations/organizations'
import type {
  OrganizationLogoUploadResponse,
  UpdateOrganizationPlanningSettingsRequest,
  UpdateOrganizationRequest,
} from '~/generated/models'
import { customFetch } from '~/utils/apiClient'
import { queryKeys } from '~/utils/queryKeys'

async function postOrganizationLogo(file: File): Promise<OrganizationLogoUploadResponse> {
  const formData = new FormData()
  formData.append('file', file)

  return customFetch<OrganizationLogoUploadResponse>('/api/organizations/current/logo', {
    method: 'POST',
    body: formData,
    headers: {},
  })
}

export function useOrganizationSettingsApi() {
  const queryClient = useQueryClient()
  const toast = useToast()

  const updateOrganization = useMutation({
    mutationFn: (request: UpdateOrganizationRequest) => putApiOrganizationsCurrent(request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current }),
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.mine }),
      ])
      toast.add({ title: 'Organisatie bijgewerkt', color: 'success' })
    },
    onError: (error) => {
      const { message } = useApiError(error)
      toast.add({ title: message.value, color: 'error' })
    },
  })

  const updatePlanningSettings = useMutation({
    mutationFn: (request: UpdateOrganizationPlanningSettingsRequest) =>
      putApiOrganizationsCurrentPlanningSettings(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current })
      toast.add({ title: 'Planninginstellingen opgeslagen', color: 'success' })
    },
    onError: (error) => {
      const { message } = useApiError(error)
      toast.add({ title: message.value, color: 'error' })
    },
  })

  const uploadLogo = useMutation({
    mutationFn: (file: File) => postOrganizationLogo(file),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.organizations.current })
      toast.add({ title: 'Logo opgeslagen', color: 'success' })
    },
    onError: (error) => {
      const { message } = useApiError(error)
      toast.add({ title: message.value, color: 'error' })
    },
  })

  return {
    updateOrganization,
    updatePlanningSettings,
    uploadLogo,
  }
}
