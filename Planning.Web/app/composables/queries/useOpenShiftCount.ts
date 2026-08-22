import { useQuery } from '@tanstack/vue-query';
import { getApiPlanning } from '~/generated/api/planning/planning';
import { PLANNING_PAGE_SIZE } from '~/utils/planning/constants';
import { getMonday, addDays, toUtcDateTimeIso } from '~/utils/planning/dateUtils';
import { queryKeys } from '~/utils/queryKeys';

/**
 * Open shifts (no employee assigned) in the current week — the number a planner needs to act on.
 * Cancelled bookings are left out: they no longer need anyone.
 */
export function useOpenShiftCount(enabled: Ref<boolean>) {
  const auth = useAuthStore();

  const range = computed(() => {
    const start = getMonday(new Date());
    return {
      startUtc: toUtcDateTimeIso(start),
      endUtc: toUtcDateTimeIso(addDays(start, 7)),
    };
  });

  const query = useQuery({
    queryKey: computed(() => queryKeys.planning.openShifts(range.value.startUtc, range.value.endUtc)),
    queryFn: () =>
      getApiPlanning({
        StartUtc: range.value.startUtc,
        EndUtc: range.value.endUtc,
        PageSize: PLANNING_PAGE_SIZE,
      }),
    enabled: computed(() => enabled.value && auth.isAuthenticated && auth.hasOrganization),
  });

  const openShiftCount = computed(() =>
    (query.data.value?.items ?? []).filter(
      (record) => !record.assignedUserId && record.status !== 'Cancelled',
    ).length,
  );

  return {
    ...query,
    openShiftCount,
  };
}
