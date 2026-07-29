import { useQuery } from '@tanstack/vue-query';
import { getPlanningAvailability } from '~/utils/availabilityClient';
import { queryKeys } from '~/utils/queryKeys';
import { toDateKey, addDays } from '~/utils/planning/dateUtils';

export function usePlanningAvailabilityPeriods(options: {
  rangeStart: Ref<Date>
  rangeEnd: Ref<Date>
  employeeIds?: Ref<string[] | undefined>
  enabled?: Ref<boolean>
}) {
  const auth = useAuthStore();

  const range = computed(() => {
    const start = options.rangeStart.value;
    // API endDate is inclusive; loadedRangeEnd is exclusive.
    const inclusiveEnd = addDays(options.rangeEnd.value, -1);
    return {
      startDate: toDateKey(start),
      endDate: toDateKey(inclusiveEnd < start ? start : inclusiveEnd),
    };
  });

  const employeeIdsParam = computed(() => {
    if (options.employeeIds?.value?.length) {
      return options.employeeIds.value.join(',');
    }
    return undefined;
  });

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
      (options.enabled?.value ?? true) &&
      auth.isAuthenticated &&
      auth.hasOrganization,
    ),
  });
}
