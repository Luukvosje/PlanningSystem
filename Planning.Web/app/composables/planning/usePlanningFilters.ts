import type { PlanningStatus } from '~/types/planning';

export function usePlanningFilters() {
  const { t } = useI18n();
  const store = usePlanningStore();

  const statusOptions = computed(() => [
    { label: t('planning.status.planned'), value: 'Planned' as PlanningStatus },
    { label: t('planning.status.confirmed'), value: 'Confirmed' as PlanningStatus },
    { label: t('planning.status.completed'), value: 'Completed' as PlanningStatus },
    { label: t('planning.status.cancelled'), value: 'Cancelled' as PlanningStatus },
  ]);

  const hasActiveFilters = computed(() =>
    store.filters.userIds.length > 0 ||
    store.filters.customerIds.length > 0 ||
    store.filters.statuses.length > 0 ||
    store.filters.search.length > 0,
  );

  function clearFilters() {
    store.filters = {
      userIds: [],
      customerIds: [],
      statuses: [],
      search: '',
    };
  }

  return {
    filters: computed(() => store.filters),
    statusOptions,
    hasActiveFilters,
    clearFilters,
  };
}
