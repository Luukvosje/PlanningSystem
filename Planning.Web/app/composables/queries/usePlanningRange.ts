import { useQueries, useQuery } from '@tanstack/vue-query';
import type { PlanningRecord } from '~/types/planning';
import { getPlanningList } from '~/utils/planningClient';
import { queryKeys } from '~/utils/queryKeys';
import { eachMonthInRange, toUtcDateTimeIso } from '~/utils/planning/dateUtils';

export function usePlanningRange() {
  const auth = useAuthStore();
  const store = usePlanningStore();

  const dateRange = computed(() => ({
    start: store.loadedRangeStart,
    end: store.loadedRangeEnd,
  }));

  const filterKey = computed(() => JSON.stringify(store.filters));

  const months = computed(() =>
    eachMonthInRange(store.loadedRangeStart, store.loadedRangeEnd),
  );

  const enabled = computed(() => auth.isAuthenticated && auth.hasOrganization);

  const monthQueries = useQueries({
    queries: computed(() =>
      months.value.map((month) => {
        const startUtc = toUtcDateTimeIso(month.start);
        const endUtc = toUtcDateTimeIso(month.end);
        return {
          queryKey: queryKeys.planning.range(startUtc, endUtc, filterKey.value),
          queryFn: () =>
            getPlanningList({
              startUtc,
              endUtc,
              userIds: store.filters.userIds.length ? store.filters.userIds.join(',') : undefined,
              customerIds: store.filters.customerIds.length ?
                store.filters.customerIds.join(',') :
                undefined,
              statuses: store.filters.statuses.length ?
                store.filters.statuses.join(',') :
                undefined,
              search: store.filters.search || undefined,
              pageSize: 2000,
            }),
          enabled: enabled.value,
        };
      }),
    ),
  });

  const records = computed(() => {
    const byId = new Map<string, PlanningRecord>();
    for (const query of monthQueries.value) {
      for (const item of query.data?.items ?? []) {
        byId.set(item.id, item);
      }
    }

    const patches = store.optimisticPatches;
    return [...byId.values()].map((item) => {
      const patch = patches.get(item.id);
      return patch ? { ...item, ...patch } : item;
    });
  });

  const isInitialLoading = computed(() => {
    if (!enabled.value || months.value.length === 0) {
      return false;
    }
    const hasAnyData = monthQueries.value.some((query) => query.data != null);
    if (hasAnyData) {
      return false;
    }
    return monthQueries.value.some((query) => query.isPending || query.isLoading);
  });

  const isFetching = computed(() =>
    monthQueries.value.some((query) => query.isFetching),
  );

  const isLoading = computed(() => isInitialLoading.value);

  const error = computed(() =>
    monthQueries.value.find((query) => query.error)?.error ?? null,
  );

  async function refetch() {
    await Promise.all(monthQueries.value.map((query) => query.refetch()));
  }

  return {
    records,
    dateRange,
    isInitialLoading,
    isLoading,
    isFetching,
    error,
    refetch,
    monthQueries,
  };
}

export function usePlanningDetail(id: Ref<string | null | undefined>) {
  const auth = useAuthStore();
  const api = usePlanningApi();

  return useQuery({
    queryKey: computed(() => queryKeys.planning.detail(id.value ?? '')),
    queryFn: () => api.getById(id.value!),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization && !!id.value),
  });
}
