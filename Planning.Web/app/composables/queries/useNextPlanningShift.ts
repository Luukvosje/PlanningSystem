import { useQuery } from '@tanstack/vue-query'
import { getPlanningList } from '~/utils/planningClient'
import { queryKeys } from '~/utils/queryKeys'

const LOOKAHEAD_DAYS = 90

export function useNextPlanningShift() {
  const auth = useAuthStore()

  const userId = computed(() => auth.currentUser?.userId)

  const startUtc = computed(() => new Date().toISOString())

  const endUtc = computed(() => {
    const end = new Date()
    end.setDate(end.getDate() + LOOKAHEAD_DAYS)
    return end.toISOString()
  })

  const query = useQuery({
    queryKey: computed(() => queryKeys.planning.nextShift(userId.value)),
    queryFn: () =>
      getPlanningList({
        startUtc: startUtc.value,
        endUtc: endUtc.value,
        userIds: userId.value,
        pageSize: 500,
      }),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization && !!userId.value),
  })

  const nextShift = computed(() => {
    const now = Date.now()
    const items = (query.data.value?.items ?? [])
      .filter(record =>
        record.status !== 'Cancelled'
        && new Date(record.endUtc).getTime() > now,
      )
      .sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime())

    return items[0] ?? null
  })

  return {
    ...query,
    nextShift,
  }
}
