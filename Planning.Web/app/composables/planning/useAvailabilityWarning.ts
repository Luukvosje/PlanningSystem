import type { AvailabilityRule, UnavailablePeriod } from '~/types/availability'
import type { PlanningRecord } from '~/types/planning'
import { hasAvailabilityConflict } from '~/utils/planning/availabilityMath'

export function useAvailabilityWarning(
  users: Ref<{ id?: string, firstName?: string | null, lastName?: string | null }[] | undefined>,
  sources: {
    rules?: Ref<AvailabilityRule[]>
    planningPeriods?: Ref<UnavailablePeriod[]>
  },
) {
  function getEmployeeName(employeeId: string) {
    const user = users.value?.find(item => item.id === employeeId)
    if (!user) return 'Medewerker'
    return `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || 'Medewerker'
  }

  function checkRecord(record: Pick<PlanningRecord, 'assignedUserId' | 'startUtc' | 'endUtc'>) {
    return hasAvailabilityConflict(
      record.assignedUserId,
      record.startUtc,
      record.endUtc,
      sources.rules?.value ?? [],
      getEmployeeName(record.assignedUserId),
      sources.planningPeriods?.value,
    )
  }

  function checkAssignment(employeeId: string, startUtc: string, endUtc: string) {
    return hasAvailabilityConflict(
      employeeId,
      startUtc,
      endUtc,
      sources.rules?.value ?? [],
      getEmployeeName(employeeId),
      sources.planningPeriods?.value,
    )
  }

  return {
    checkRecord,
    checkAssignment,
  }
}
