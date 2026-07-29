import type { PlanningRecord } from '~/types/planning';
import { storeToRefs } from 'pinia';

export function usePlanningAvailability(records: Ref<PlanningRecord[]>) {
  const store = usePlanningStore();
  const { loadedRangeStart, loadedRangeEnd } = storeToRefs(store);
  const { data: users } = useUsers();

  const availabilityEmployeeIds = computed(() => {
    const activeUsers = (users.value ?? [])
      .filter((user) => user.isActive !== false)
      .map((user) => user.id!)
      .filter(Boolean);

    if (store.filters.userIds.length > 0) {
      return store.filters.userIds;
    }

    if (store.rowMode === 'resource') {
      return activeUsers;
    }

    const fromRecords = [...new Set(
      records.value
        .map((record) => record.assignedUserId)
        .filter((id) => id && id !== '0'),
    )];

    return fromRecords.length > 0 ? fromRecords : activeUsers;
  });

  const { data: availabilityData } = usePlanningAvailabilityPeriods({
    rangeStart: loadedRangeStart,
    rangeEnd: loadedRangeEnd,
    employeeIds: availabilityEmployeeIds,
  });

  const availabilityPeriods = computed(() => availabilityData.value?.periods ?? []);

  return {
    availabilityPeriods,
  };
}
