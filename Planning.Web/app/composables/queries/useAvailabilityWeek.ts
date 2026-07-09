import { useQuery } from '@tanstack/vue-query'
import { getAvailabilityWeek } from '~/utils/availabilityClient'
import { queryKeys } from '~/utils/queryKeys'
import { getMonday, toUtcDateTimeIso } from '~/utils/planning/dateUtils'

export function useAvailabilityWeek(options: {
  weekDate: Ref<Date>
  userId?: Ref<string | null | undefined>
  userIds?: Ref<string[] | undefined>
  enabled?: Ref<boolean>
}) {
  const auth = useAuthStore()

  const weekStartUtc = computed(() => toUtcDateTimeIso(getMonday(options.weekDate.value)))

  const userIdsParam = computed(() => {
    if (options.userIds?.value?.length) {
      return options.userIds.value.join(',')
    }
    return undefined
  })

  return useQuery({
    queryKey: computed(() =>
      queryKeys.availability.week(
        weekStartUtc.value,
        options.userId?.value ?? undefined,
        userIdsParam.value,
      ),
    ),
    queryFn: () =>
      getAvailabilityWeek({
        weekStartUtc: weekStartUtc.value,
        userId: options.userId?.value ?? undefined,
        userIds: userIdsParam.value,
      }),
    enabled: computed(() =>
      (options.enabled?.value ?? true)
      && auth.isAuthenticated
      && auth.hasOrganization,
    ),
  })
}
