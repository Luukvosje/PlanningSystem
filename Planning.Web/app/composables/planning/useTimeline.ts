import type { PlanningRecord } from '~/types/planning';
import type { CompactCurrentTimeIndicator, DayTimeWindow } from '~/utils/planning/timelineMath';
import {
  countVisibleDaysInRange,
  computeWeekWindows,
  getCompactCurrentTimePx,
  getCompactImportantGridLines,
  getCompactTimelineGridLines,
  getCoarseGridLines,
  getCurrentTimePx,
  getDayCount,
  getDayWidth,
  getHeaderColumnMode,
  getHeaderPeriodMode,
  getImportantGridLines,
  getPrimaryBorderMode,
  getTimelineGridLines,
  getTimelineWidth,
  getTimeSlotMarkers,
  getZoomMinutes,
  isZoomedOutView,
  pxToUtcIso as pxToUtcIsoMath,
  pxToUtcIsoCompact,
  ROW_LABEL_WIDTH,
  showsTimeSlots,
  SNAP_MINUTES,
  timeToPx,
  timeToPxCompact,
  toDateKey,
} from '~/utils/planning/timelineMath';
import {
  addDays,
  formatCompactDayHeader,
  formatDayHeader,
  getIntlLocale,
  getMonday,
  startOfMonth,
} from '~/utils/planning/dateUtils';
import {
  formatWeekDateRange,
  getISOWeekNumber,
} from '~/utils/planning/planningSettings';

export interface TimelineDayHeader {
  date: Date
  label: string
  compact: { weekday: string, day: string }
  left: number
  width: number
  isWeekend: boolean
  weekKey: string
  monthKey: string
  isPrimaryBorderEnd: boolean
  window?: DayTimeWindow
  gridLines?: ReturnType<typeof getCompactTimelineGridLines>
  importantGridLines?: ReturnType<typeof getCompactImportantGridLines>
  isEmptyDay?: boolean
}

export interface TimelinePeriodHeader {
  key: string
  label: string
  dateRange?: string
  left: number
  width: number
  isPrimaryBorderEnd: boolean
}

/** @deprecated Prefer TimelinePeriodHeader */
export type TimelineWeekHeader = TimelinePeriodHeader

function monthKeyForDate(date: Date): string {
  return startOfMonth(date).toISOString();
}

function formatMonthLabel(date: Date, locale: string): string {
  return new Intl.DateTimeFormat(locale, {
    month: 'long',
    year: 'numeric',
  }).format(date);
}

function groupDaysByKey(
  days: TimelineDayHeader[],
  keyOf: (day: TimelineDayHeader) => string,
): Map<string, TimelineDayHeader[]> {
  const groups = new Map<string, TimelineDayHeader[]>();
  for (const day of days) {
    const key = keyOf(day);
    const existing = groups.get(key) ?? [];
    existing.push(day);
    groups.set(key, existing);
  }
  return groups;
}

function buildWeekPeriodHeaders(
  dayGroups: Map<string, TimelineDayHeader[]>,
  primaryBorder: ReturnType<typeof getPrimaryBorderMode>,
  locale: string,
): TimelinePeriodHeader[] {
  const entries = [...dayGroups.entries()];
  return entries.map(([, days], index) => {
    const startDate = days[0]!.date;
    const endDate = days[days.length - 1]!.date;
    const nextGroup = entries[index + 1]?.[1];
    const isPrimaryBorderEnd = primaryBorder === 'day' ||
      primaryBorder === 'week' ||
      (primaryBorder === 'month' &&
        (!nextGroup || nextGroup[0]!.monthKey !== days[0]!.monthKey));

    return {
      key: days[0]!.weekKey,
      label: `Week ${getISOWeekNumber(startDate)}`,
      dateRange: formatWeekDateRange(startDate, endDate, locale),
      left: days[0]!.left,
      width: days.reduce((total, day) => total + day.width, 0),
      isPrimaryBorderEnd,
    };
  });
}

function buildMonthPeriodHeaders(
  dayGroups: Map<string, TimelineDayHeader[]>,
  locale: string,
): TimelinePeriodHeader[] {
  return [...dayGroups.entries()].map(([, days]) => {
    const startDate = days[0]!.date;
    return {
      key: days[0]!.monthKey,
      label: formatMonthLabel(startDate, locale),
      left: days[0]!.left,
      width: days.reduce((total, day) => total + day.width, 0),
      isPrimaryBorderEnd: true,
    };
  });
}

function recordsForDay(records: PlanningRecord[], date: Date): PlanningRecord[] {
  const dayStart = new Date(date);
  dayStart.setHours(0, 0, 0, 0);
  const dayEnd = addDays(dayStart, 1);

  return records.filter((record) => {
    const start = new Date(record.startUtc);
    const end = new Date(record.endUtc);
    return start < dayEnd && end > dayStart;
  });
}

function getDayIndexForIso(utcIso: string, rangeStart: Date): number {
  const date = new Date(utcIso);
  const dayStart = new Date(date);
  dayStart.setHours(0, 0, 0, 0);
  return Math.floor((dayStart.getTime() - rangeStart.getTime()) / (24 * 60 * 60 * 1000));
}

export function useTimeline() {
  const store = usePlanningStore();
  const { locale } = useI18n();
  const intlLocale = computed(() => getIntlLocale(locale.value));
  const { importantWorkTimes, openingHours } = usePlanningSettings();
  const containerWidth = inject<Ref<number>>('timelineContainerWidth', ref(0));
  const rowRecords = inject<Ref<Map<string, PlanningRecord[]>>>('timelineRowRecords', ref(new Map()));

  const dateRange = computed(() => ({
    start: store.loadedRangeStart,
    end: store.loadedRangeEnd,
  }));

  /** Compact mode: tighter row layout based on user preference. */
  const isCompactMode = computed(() => store.rowLayout === 'compact');

  const allVisibleRecords = computed(() =>
    [...rowRecords.value.values()].flat(),
  );

  const dayCount = computed(() => getDayCount(dateRange.value.start, dateRange.value.end));

  const visibleDays = computed(() =>
    Array.from({ length: dayCount.value }, (_, index) => addDays(dateRange.value.start, index)),
  );

  const visibleDayCount = computed(() =>
    countVisibleDaysInRange(dateRange.value.start, dateRange.value.end, store.showWeekends),
  );

  const dayWidth = computed(() =>
    getDayWidth(store.zoom, visibleDayCount.value, containerWidth.value, store.slotScale),
  );
  const timelineWidth = computed(() => getTimelineWidth(visibleDayCount.value, dayWidth.value));

  /**
   * Map from date-key (YYYY-MM-DD) to left-pixel offset.
   * Hidden weekend days (showWeekends = false) contribute 0 width.
   */
  const dayLeftMap = computed<Map<string, number>>(() => {
    const map = new Map<string, number>();
    let left = 0;
    for (const date of visibleDays.value) {
      const key = toDateKey(date);
      map.set(key, left);
      const isWeekend = date.getDay() === 0 || date.getDay() === 6;
      if (!isWeekend || store.showWeekends) {
        left += dayWidth.value;
      }
    }
    return map;
  });

  /**
   * dayWindows: per-dag tijdvenster ingekrompen naar openingstijden + records.
   * Actief wanneer showFullDay = false (gebruik openingstijden) OF isCompactMode.
   */
  const dayWindows = computed(() =>
    (!store.showFullDay) ?
      computeWeekWindows(
        visibleDays.value,
        allVisibleRecords.value,
        openingHours.value,
        importantWorkTimes.value,
        { zoomMinutes: getZoomMinutes(store.zoom) },
      ) :
      null,
  );

  const dayHeaders = computed<TimelineDayHeader[]>(() => {
    const primaryBorder = getPrimaryBorderMode(store.zoom);
    const days = visibleDays.value;

    return days
      .map((date, index) => {
        const monday = getMonday(date);
        const monthKey = monthKeyForDate(date);
        const nextDate = days[index + 1];
        const nextWeekKey = nextDate ? getMonday(nextDate).toISOString() : null;
        const nextMonthKey = nextDate ? monthKeyForDate(nextDate) : null;
        const weekKey = monday.toISOString();
        const isWeekend = date.getDay() === 0 || date.getDay() === 6;
        const width = (!store.showWeekends && isWeekend) ? 0 : dayWidth.value;
        const left = dayLeftMap.value.get(toDateKey(date)) ?? 0;

        const isPrimaryBorderEnd =
          primaryBorder === 'day' ||
          (primaryBorder === 'week' && weekKey !== nextWeekKey) ||
          (primaryBorder === 'month' && monthKey !== nextMonthKey);

        const base: TimelineDayHeader = {
          date,
          label: formatDayHeader(date, intlLocale.value),
          compact: formatCompactDayHeader(date, intlLocale.value),
          left,
          width,
          isWeekend,
          weekKey,
          monthKey,
          isPrimaryBorderEnd,
        };

        if (!dayWindows.value) {
          return base;
        }

        const window = dayWindows.value.get(toDateKey(date))!;
        const dayRecords = recordsForDay(allVisibleRecords.value, date);

        return {
          ...base,
          window,
          gridLines: getCompactTimelineGridLines(store.zoom, dayWidth.value, window, importantWorkTimes.value, intlLocale.value),
          importantGridLines: getCompactImportantGridLines(dayWidth.value, window, importantWorkTimes.value, intlLocale.value),
          isEmptyDay: dayRecords.length === 0,
        };
      })
      .filter((day) => store.showWeekends || !day.isWeekend);
  });

  const columnMode = computed(() => getHeaderColumnMode(store.zoom));
  const periodMode = computed(() => getHeaderPeriodMode(store.zoom));
  const primaryBorder = computed(() => getPrimaryBorderMode(store.zoom));

  const weekGroups = computed(() =>
    groupDaysByKey(dayHeaders.value, (day) => day.weekKey),
  );

  const monthGroups = computed(() =>
    groupDaysByKey(dayHeaders.value, (day) => day.monthKey),
  );

  const weekHeaders = computed(() =>
    buildWeekPeriodHeaders(weekGroups.value, primaryBorder.value, intlLocale.value),
  );

  const monthHeaders = computed(() =>
    buildMonthPeriodHeaders(monthGroups.value, intlLocale.value),
  );

  const showPeriodHeaders = computed(() => periodMode.value !== null);

  const periodHeaders = computed<TimelinePeriodHeader[]>(() => {
    if (periodMode.value === 'week') {
      return weekHeaders.value;
    }
    if (periodMode.value === 'month') {
      return monthHeaders.value;
    }
    return [];
  });

  /** Labels for the second header row (day / week / month columns). */
  const columnHeaders = computed<TimelinePeriodHeader[]>(() => {
    if (columnMode.value === 'week') {
      return weekHeaders.value;
    }
    if (columnMode.value === 'month') {
      return monthHeaders.value;
    }
    return [];
  });

  /** @deprecated Use showPeriodHeaders */
  const showWeekHeaders = showPeriodHeaders;
  const slotWidth = computed(() => {
    const slotMinutes = getZoomMinutes(store.zoom);
    if (slotMinutes >= 24 * 60) {
      return dayWidth.value;
    }
    return dayWidth.value / ((24 * 60) / slotMinutes);
  });

  const isZoomedOut = computed(() => isZoomedOutView(store.zoom, slotWidth.value));

  const timeSlotMarkers = computed(() => getTimeSlotMarkers(store.zoom, dayWidth.value, intlLocale.value));

  const timelineGridLines = computed(() => getTimelineGridLines(store.zoom, dayWidth.value, importantWorkTimes.value, intlLocale.value));

  const importantGridLines = computed(() =>
    getImportantGridLines(dayWidth.value, importantWorkTimes.value, intlLocale.value),
  );

  /** Coarse vertical lines for calendar zoom levels (day / week / month). */
  const coarseGridLines = computed(() => getCoarseGridLines(store.zoom, dayWidth.value, intlLocale.value));

  const showTimeSlots = computed(() => showsTimeSlots(store.zoom));

  function toPx(utcIso: string): number {
    const dayIndex = getDayIndexForIso(utcIso, dateRange.value.start);
    const day = addDays(dateRange.value.start, dayIndex);
    const dayKey = toDateKey(day);

    if (!store.showWeekends) {
      // Use precomputed left offset; weekend days snap to their left edge
      const dayLeft = dayLeftMap.value.get(dayKey) ?? 0;
      const isWeekend = day.getDay() === 0 || day.getDay() === 6;
      if (isWeekend) {
return dayLeft;
}
      const fracMs = new Date(utcIso).getTime() - day.getTime();
      const fracPx = (fracMs / (24 * 60 * 60 * 1000)) * dayWidth.value;
      return dayLeft + fracPx;
    }

    if (!dayWindows.value) {
      return timeToPx(utcIso, dateRange.value.start, dayWidth.value);
    }

    const window = dayWindows.value.get(dayKey);
    if (!window) {
      return timeToPx(utcIso, dateRange.value.start, dayWidth.value);
    }

    return dayIndex * dayWidth.value + timeToPxCompact(utcIso, window, dayWidth.value);
  }

  function toIso(px: number, snap = true): string {
    if (!store.showWeekends) {
      // Find which visible (non-weekend) day this px falls in
      const entries = [...dayLeftMap.value.entries()];
      let bestKey = entries[0]?.[0] ?? toDateKey(dateRange.value.start);
      for (const [key, left] of entries) {
        const date = new Date(key);
        const isWeekend = date.getDay() === 0 || date.getDay() === 6;
        if (isWeekend) {
continue;
}
        if (left <= px) {
bestKey = key;
}
      }
      const dayStart = new Date(bestKey);
      dayStart.setHours(0, 0, 0, 0);
      const dayLeft = dayLeftMap.value.get(bestKey) ?? 0;
      const pxInDay = Math.max(0, px - dayLeft);
      const msInDay = (pxInDay / dayWidth.value) * 24 * 60 * 60 * 1000;
      const snapMs = snap ? SNAP_MINUTES * 60 * 1000 : 1;
      const snapped = Math.round((dayStart.getTime() + msInDay) / snapMs) * snapMs;
      return new Date(snapped).toISOString();
    }

    if (!dayWindows.value) {
      return pxToUtcIsoMath(px, dateRange.value.start, dayWidth.value, snap ? SNAP_MINUTES : 0);
    }

    const dayIndex = Math.floor(px / dayWidth.value);
    const pxInDay = px - dayIndex * dayWidth.value;
    const day = addDays(dateRange.value.start, dayIndex);
    const window = dayWindows.value.get(toDateKey(day));
    if (!window) {
      return pxToUtcIsoMath(px, dateRange.value.start, dayWidth.value, snap ? SNAP_MINUTES : 0);
    }

    return pxToUtcIsoCompact(pxInDay, window, dayWidth.value, snap ? SNAP_MINUTES : 0);
  }

  const currentTimeIndicator = computed<CompactCurrentTimeIndicator | null>(() => {
    const now = new Date();
    if (now < dateRange.value.start || now >= dateRange.value.end) {
      return null;
    }

    if (dayWindows.value) {
      for (const date of visibleDays.value) {
        const isWeekend = date.getDay() === 0 || date.getDay() === 6;
        if (!store.showWeekends && isWeekend) {
          continue;
        }

        const key = toDateKey(date);
        const window = dayWindows.value.get(key);
        if (!window) {
          continue;
        }

        const columnOffsetPx = dayLeftMap.value.get(key) ?? 0;
        const indicator = getCompactCurrentTimePx(
          now,
          window,
          dayWidth.value,
          columnOffsetPx,
        );
        if (indicator) {
          return indicator;
        }
      }
      return null;
    }

    if (!store.showWeekends) {
      return { px: toPx(now.toISOString()), clamped: 'none' };
    }

    const px = getCurrentTimePx(dateRange.value.start, dateRange.value.end, dayWidth.value);
    return px !== null ? { px, clamped: 'none' } : null;
  });

  const currentTimePx = computed(() => currentTimeIndicator.value?.px ?? null);

  function getBlockLayout(record: PlanningRecord) {
    const leftPx = toPx(record.startUtc);
    const rightPx = toPx(record.endUtc);
    return {
      leftPx,
      widthPx: Math.max(rightPx - leftPx, 4),
    };
  }

  function pxToUtcIso(px: number, snap = true): string {
    return toIso(px, snap);
  }

  return {
    dateRange,
    dayWidth,
    dayCount,
    timelineWidth,
    dayHeaders,
    weekHeaders,
    monthHeaders,
    periodHeaders,
    columnHeaders,
    showPeriodHeaders,
    showWeekHeaders,
    columnMode,
    periodMode,
    primaryBorder,
    timeSlotMarkers,
    timelineGridLines,
    importantGridLines,
    coarseGridLines,
    showTimeSlots,
    isZoomedOut,
    isCompactMode,
    dayWindows,
    rowLabelWidth: ROW_LABEL_WIDTH,
    currentTimePx,
    currentTimeIndicator,
    toPx,
    toIso,
    timeToPx: toPx,
    getBlockLayout,
    pxToUtcIso,
  };
}
