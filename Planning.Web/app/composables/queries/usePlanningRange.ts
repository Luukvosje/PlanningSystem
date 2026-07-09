import { useQuery } from '@tanstack/vue-query'
import { getPlanningList } from '~/utils/planningClient'
import { queryKeys } from '~/utils/queryKeys'
import { computeDateRange } from '~/utils/planning/timelineMath'
import { toUtcDateTimeIso } from '~/utils/planning/dateUtils'

export function usePlanningRange() {
  const auth = useAuthStore()
  const store = usePlanningStore()

  const dateRange = computed(() =>
    computeDateRange(store.currentDate, store.viewMode),
  )

  const filterKey = computed(() => JSON.stringify(store.filters))

  const startUtc = computed(() => toUtcDateTimeIso(dateRange.value.start))
  const endUtc = computed(() => {
    const end = dateRange.value.end
    return toUtcDateTimeIso(end)
  })

  const query = useQuery({
    queryKey: computed(() =>
      queryKeys.planning.range(startUtc.value, endUtc.value, filterKey.value),
    ),
    queryFn: () =>
      getPlanningList({
        startUtc: startUtc.value,
        endUtc: endUtc.value,
        userIds: store.filters.userIds.length ? store.filters.userIds.join(',') : undefined,
        customerIds: store.filters.customerIds.length ? store.filters.customerIds.join(',') : undefined,
        statuses: store.filters.statuses.length ? store.filters.statuses.join(',') : undefined,
        search: store.filters.search || undefined,
        pageSize: 2000,
      }),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
  })

  const records = computed(() => {
    const items = query.data.value?.items ?? []
    const patches = store.optimisticPatches

    return items.map((item) => {
      const patch = patches.get(item.id)
      return patch ? { ...item, ...patch } : item
    })
  })

  return {
    ...query,
    records,
    dateRange,
    startUtc,
    endUtc,
  }
}

export function usePlanningDetail(id: Ref<string | null | undefined>) {
  const auth = useAuthStore()
  const api = usePlanningApi()

  return useQuery({
    queryKey: computed(() => queryKeys.planning.detail(id.value ?? '')),
    queryFn: () => api.getById(id.value!),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization && !!id.value),
  })
}
