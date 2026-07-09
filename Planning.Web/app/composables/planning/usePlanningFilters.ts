import type { PlanningStatus } from '~/types/planning'

export function usePlanningFilters() {
  const store = usePlanningStore()

  const statusOptions = [
    { label: 'Gepland', value: 'Planned' as PlanningStatus },
    { label: 'Bevestigd', value: 'Confirmed' as PlanningStatus },
    { label: 'Afgerond', value: 'Completed' as PlanningStatus },
    { label: 'Geannuleerd', value: 'Cancelled' as PlanningStatus },
  ]

  const hasActiveFilters = computed(() =>
    store.filters.userIds.length > 0
    || store.filters.customerIds.length > 0
    || store.filters.statuses.length > 0
    || store.filters.search.length > 0,
  )

  function clearFilters() {
    store.filters = {
      userIds: [],
      customerIds: [],
      statuses: [],
      search: '',
    }
  }

  return {
    filters: computed(() => store.filters),
    statusOptions,
    hasActiveFilters,
    clearFilters,
  }
}
