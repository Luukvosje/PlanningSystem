import type { AvailabilityEntry } from '~/types/availability'
import type { PlanningRecord } from '~/types/planning'
import { hasAvailabilityConflict } from '~/utils/planning/availabilityMath'

export function useAvailabilityWarning(
  entries: Ref<AvailabilityEntry[]>,
  users: Ref<{ id?: string, firstName?: string | null, lastName?: string | null }[] | undefined>,
) {
  function getUserName(userId: string) {
    const user = users.value?.find(item => item.id === userId)
    if (!user) return 'Medewerker'
    return `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || 'Medewerker'
  }

  function checkRecord(record: Pick<PlanningRecord, 'assignedUserId' | 'startUtc' | 'endUtc'>) {
    return hasAvailabilityConflict(
      record.assignedUserId,
      record.startUtc,
      record.endUtc,
      entries.value,
      getUserName(record.assignedUserId),
    )
  }

  function checkAssignment(userId: string, startUtc: string, endUtc: string) {
    return hasAvailabilityConflict(
      userId,
      startUtc,
      endUtc,
      entries.value,
      getUserName(userId),
    )
  }

  return {
    checkRecord,
    checkAssignment,
  }
}
