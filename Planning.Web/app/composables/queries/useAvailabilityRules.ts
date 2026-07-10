import { useQuery } from '@tanstack/vue-query'
import { getAvailabilityRules } from '~/utils/availabilityClient'
import { queryKeys } from '~/utils/queryKeys'

export function useAvailabilityRules(options: {
  employeeId: Ref<string | null | undefined>
  enabled?: Ref<boolean>
}) {
  const auth = useAuthStore()

  return useQuery({
    queryKey: computed(() => queryKeys.availability.rules(options.employeeId.value ?? undefined)),
    queryFn: () => getAvailabilityRules(options.employeeId.value!),
    enabled: computed(() =>
      (options.enabled?.value ?? true)
      && !!options.employeeId.value
      && auth.isAuthenticated
      && auth.hasOrganization,
    ),
  })
}
