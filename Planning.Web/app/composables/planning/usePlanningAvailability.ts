import type { PlanningRecord } from '~/types/planning'

export function usePlanningAvailability(records: Ref<PlanningRecord[]>) {
  const store = usePlanningStore()
  const { data: users } = useUsers()

  const weekDate = computed(() => store.currentDate)

  const availabilityEmployeeIds = computed(() => {
    const activeUsers = (users.value ?? [])
      .filter(user => user.isActive !== false)
      .map(user => user.id!)
      .filter(Boolean)

    if (store.filters.userIds.length > 0) {
      return store.filters.userIds
    }

    if (store.rowMode === 'resource') {
      return activeUsers
    }

    const fromRecords = [...new Set(
      records.value
        .map(record => record.assignedUserId)
        .filter(id => id && id !== '0'),
    )]

    return fromRecords.length > 0 ? fromRecords : activeUsers
  })

  const { data: availabilityData } = usePlanningAvailabilityPeriods({
    weekDate,
    employeeIds: availabilityEmployeeIds,
  })

  const availabilityPeriods = computed(() => availabilityData.value?.periods ?? [])

  return {
    availabilityPeriods,
  }
}
