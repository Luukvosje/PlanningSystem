import type { AvailabilityRule, UnavailablePeriod } from '~/types/availability';
import type { PlanningRecord } from '~/types/planning';
import { hasAvailabilityConflict } from '~/utils/planning/availabilityMath';

export function useAvailabilityWarning(
  users: Ref<{ id?: string, firstName?: string | null, lastName?: string | null }[] | undefined>,
  sources: {
    rules?: Ref<AvailabilityRule[]>
    planningPeriods?: Ref<UnavailablePeriod[]>
  },
) {
  const { t } = useI18n();

  function getEmployeeName(employeeId: string) {
    const user = users.value?.find((item) => item.id === employeeId);
    if (!user) {
return t('planning.fields.employee');
}
    return `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || t('planning.fields.employee');
  }

  function checkRecord(record: Pick<PlanningRecord, 'assignedUserId' | 'startUtc' | 'endUtc'>) {
    // An open shift has nobody assigned, so there is no availability to conflict with.
    if (!record.assignedUserId) {
      return { hasConflict: false };
    }

    return hasAvailabilityConflict(
      record.assignedUserId,
      record.startUtc,
      record.endUtc,
      sources.rules?.value ?? [],
      t,
      getEmployeeName(record.assignedUserId),
      sources.planningPeriods?.value,
    );
  }

  function checkAssignment(employeeId: string, startUtc: string, endUtc: string) {
    return hasAvailabilityConflict(
      employeeId,
      startUtc,
      endUtc,
      sources.rules?.value ?? [],
      t,
      getEmployeeName(employeeId),
      sources.planningPeriods?.value,
    );
  }

  return {
    checkRecord,
    checkAssignment,
  };
}
