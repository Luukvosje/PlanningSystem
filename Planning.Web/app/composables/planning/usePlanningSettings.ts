import type { OpeningHoursEntry } from '~/utils/planning/planningSettings'
import {
  DEFAULT_IMPORTANT_WORK_TIMES,
  normalizeTimeValue,
  uniqueImportantTimes,
} from '~/utils/planning/planningSettings'
import type { Weekday } from '~/types/availability'

export function usePlanningSettings() {
  const { data: organization } = useCurrentOrganization()

  const importantWorkTimes = computed(() => {
    const times = organization.value?.importantWorkTimes
    if (!times?.length) return DEFAULT_IMPORTANT_WORK_TIMES
    return uniqueImportantTimes(times.map(normalizeTimeValue))
  })

  const openingHours = computed<OpeningHoursEntry[]>(() =>
    (organization.value?.openingHours ?? []).map(entry => ({
      day: entry.day as Weekday,
      openTime: normalizeTimeValue(entry.openTime ?? '00:00'),
      closeTime: normalizeTimeValue(entry.closeTime ?? '23:59'),
    })),
  )

  return {
    importantWorkTimes,
    openingHours,
  }
}
