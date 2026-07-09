import { useQuery } from '@tanstack/vue-query'
import { getApiPlanningWeek } from '~/generated/api/planning/planning'
import { queryKeys } from '~/utils/queryKeys'

export function usePlanningWeek(weekStartUtc: Ref<string | undefined>) {
  const auth = useAuthStore()

  return useQuery({
    queryKey: computed(() => queryKeys.planning.week(weekStartUtc.value)),
    queryFn: () => getApiPlanningWeek({ WeekStartUtc: weekStartUtc.value }),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization && !!weekStartUtc.value),
  })
}
