import type { ImportantWorkTimeRow, OpeningHoursEntry } from '~/utils/planning/planningSettings';
import {
  DEFAULT_IMPORTANT_WORK_TIMES,
  getDefaultImportantWorkTimes,
  normalizeTimeValue,
  toTimeStrings,
  uniqueImportantTimes,
} from '~/utils/planning/planningSettings';
import type { Weekday } from '~/types/availability';

export function usePlanningSettings() {
  const { data: organization } = useCurrentOrganization();

  /** Rich entries (label + time), used by the settings editor and quick-pick chips. */
  const importantWorkTimeEntries = computed<ImportantWorkTimeRow[]>(() => {
    const entries = organization.value?.importantWorkTimes;
    if (!entries?.length) {
return getDefaultImportantWorkTimes();
}
    return entries.map((entry) => ({
      label: entry.label ?? '',
      startTime: normalizeTimeValue(entry.startTime),
    }));
  });

  /** Plain time-of-day strings only, for the existing grid/snap math. */
  const importantWorkTimes = computed(() => {
    const times = toTimeStrings(importantWorkTimeEntries.value);
    if (!times.length) {
return DEFAULT_IMPORTANT_WORK_TIMES;
}
    return uniqueImportantTimes(times);
  });

  const openingHours = computed<OpeningHoursEntry[]>(() =>
    (organization.value?.openingHours ?? []).map((entry) => ({
      day: entry.day as Weekday,
      openTime: normalizeTimeValue(entry.openTime ?? '00:00'),
      closeTime: normalizeTimeValue(entry.closeTime ?? '23:59'),
    })),
  );

  return {
    importantWorkTimes,
    importantWorkTimeEntries,
    openingHours,
  };
}
