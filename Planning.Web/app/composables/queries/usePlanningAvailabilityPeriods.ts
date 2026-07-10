import { useQuery } from '@tanstack/vue-query'
import { getPlanningAvailability } from '~/utils/availabilityClient'
import { queryKeys } from '~/utils/queryKeys'
import { getMonday, toDateKey } from '~/utils/planning/dateUtils'

export function usePlanningAvailabilityPeriods(options: {
  weekDate: Ref<Date>
  employeeIds?: Ref<string[] | undefined>
  enabled?: Ref<boolean>
}) {
  const auth = useAuthStore()

  const range = computed(() => {
    const start = getMonday(options.weekDate.value)
    const end = new Date(start)
    end.setDate(end.getDate() + 6)
    return {
      startDate: toDateKey(start),
      endDate: toDateKey(end),
    }
  })

  const employeeIdsParam = computed(() => {
    if (options.employeeIds?.value?.length) {
      return options.employeeIds.value.join(',')
    }
    return undefined
  })

  return useQuery({
    queryKey: computed(() =>
      queryKeys.availability.planning(
        range.value.startDate,
        range.value.endDate,
        employeeIdsParam.value,
      ),
    ),
    queryFn: () =>
      getPlanningAvailability({
        startDate: range.value.startDate,
        endDate: range.value.endDate,
        employeeIds: employeeIdsParam.value,
      }),
    enabled: computed(() =>
      (options.enabled?.value ?? true)
      && auth.isAuthenticated
      && auth.hasOrganization,
    ),
  })
}
