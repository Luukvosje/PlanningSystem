import type { PlanningRecord } from '~/types/planning'
import {
  computeDateRange,
  getCurrentTimePx,
  getDayCount,
  getDayWidth,
  getTimelineWidth,
  getTimeSlotMarkers,
  pxToUtcIso as pxToUtcIsoMath,
  ROW_LABEL_WIDTH,
  showsTimeSlots,
  SNAP_MINUTES,
  timeToPx,
} from '~/utils/planning/timelineMath'
import { addDays, formatDayHeader } from '~/utils/planning/dateUtils'

export function useTimeline() {
  const store = usePlanningStore()
  const containerWidth = inject<Ref<number>>('timelineContainerWidth', ref(0))

  const dateRange = computed(() =>
    computeDateRange(store.currentDate, store.viewMode),
  )

  const dayCount = computed(() => getDayCount(dateRange.value.start, dateRange.value.end))
  const dayWidth = computed(() =>
    getDayWidth(store.zoom, dayCount.value, containerWidth.value, store.slotScale),
  )
  const timelineWidth = computed(() => getTimelineWidth(dayCount.value, dayWidth.value))

  const dayHeaders = computed(() =>
    Array.from({ length: dayCount.value }, (_, index) => {
      const date = addDays(dateRange.value.start, index)
      return {
        date,
        label: formatDayHeader(date),
        left: index * dayWidth.value,
        width: dayWidth.value,
        isWeekend: date.getDay() === 0 || date.getDay() === 6,
      }
    }),
  )

  const timeSlotMarkers = computed(() => getTimeSlotMarkers(store.zoom, dayWidth.value))
  const showTimeSlots = computed(() => showsTimeSlots(store.zoom))

  const timeToPxFn = (utcIso: string) =>
    timeToPx(utcIso, dateRange.value.start, dayWidth.value)

  const currentTimePx = computed(() =>
    getCurrentTimePx(dateRange.value.start, dateRange.value.end, dayWidth.value),
  )

  function getBlockLayout(record: PlanningRecord) {
    const leftPx = timeToPxFn(record.startUtc)
    const rightPx = timeToPxFn(record.endUtc)
    return {
      leftPx,
      widthPx: Math.max(rightPx - leftPx, 4),
    }
  }

  function pxToUtcIso(px: number, snap = true): string {
    return pxToUtcIsoMath(px, dateRange.value.start, dayWidth.value, snap ? SNAP_MINUTES : 0)
  }

  return {
    dateRange,
    dayWidth,
    dayCount,
    timelineWidth,
    dayHeaders,
    timeSlotMarkers,
    showTimeSlots,
    rowLabelWidth: ROW_LABEL_WIDTH,
    currentTimePx,
    timeToPx: timeToPxFn,
    getBlockLayout,
    pxToUtcIso,
  }
}
