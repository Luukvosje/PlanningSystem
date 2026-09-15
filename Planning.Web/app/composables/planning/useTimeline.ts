import type { InjectionKey, Ref } from 'vue';
import type { PlanningRecord } from '~/types/planning';
import type { CompactCurrentTimeIndicator, DayTimeWindow, TimelineGridLine } from '~/utils/planning/timelineMath';
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
  recordsForDay,
  getTimelineGridLines,
  getTimelineWidth,
  getZoomMinutes,
  isZoomedOutView,
  pxToUtcIso as pxToUtcIsoMath,
  pxToUtcIsoCompact,
  ROW_LABEL_WIDTH,
  showsTimeSlots,
  SNAP_MINUTES,
  suppressCollidingLabels,
  timeToPxCompact,
} from '~/utils/planning/timelineMath';
import {
  addDays,
  formatCompactDayHeader,
  formatDayHeader,
  getIntlLocale,
  getMonday,
  isWeekend as isWeekendDate,
  startOfMonth,
  toDateKey,
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

function getDayIndexForIso(utcIso: string, rangeStart: Date): number {
  const date = new Date(utcIso);
  const dayStart = new Date(date);
  dayStart.setHours(0, 0, 0, 0);
  return Math.floor((dayStart.getTime() - rangeStart.getTime()) / (24 * 60 * 60 * 1000));
}

export interface TimelineSources {
  containerWidth?: Ref<number>
  rowRecords?: Ref<Map<string, PlanningRecord[]>>
}

/**
 * Builds the timeline geometry. Prefer {@link provideTimeline} / {@link useTimeline} over calling
 * this directly - every call produces its own set of computeds, and computeWeekWindows walks all
 * visible records per day.
 *
 * `sources` exists because a component cannot inject what it provides itself: the board root has
 * to hand in its own containerWidth ref, otherwise it would silently compute a different dayWidth
 * than its children.
 */
function createTimeline(sources: TimelineSources = {}) {
  const store = usePlanningStore();
  const { locale } = useI18n();
  const intlLocale = computed(() => getIntlLocale(locale.value));
  const { importantWorkTimes, openingHours } = usePlanningSettings();
  const containerWidth = sources.containerWidth ??
    inject<Ref<number>>('timelineContainerWidth', ref(0));
  const rowRecords = sources.rowRecords ??
    inject<Ref<Map<string, PlanningRecord[]>>>('timelineRowRecords', ref(new Map()));

  const dateRange = computed(() => ({
    start: store.loadedRangeStart,
    end: store.loadedRangeEnd,
  }));

  /** Row height preference. Purely vertical - says nothing about the time axis. */
  const isCompactRowLayout = computed(() => store.rowLayout === 'compact');

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
      const isWeekend = isWeekendDate(date);
      if (!isWeekend || store.showWeekends) {
        left += dayWidth.value;
      }
    }
    return map;
  });

  /**
   * dayWindows: per-dag tijdvenster ingekrompen naar openingstijden + records.
   * Actief wanneer showFullDay = false. Dit comprimeert de tijd-as horizontaal en staat los
   * van rowLayout (rijhoogte) - zie isCompactRowLayout.
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

  /**
   * True wanneer de tijd-as per dag is ingekrompen. Alles dat tijd naar pixels vertaalt moet
   * hierop kijken - niet op de rijhoogte - anders lopen blokken en achtergrond uiteen.
   */
  const usesDayWindow = computed(() => dayWindows.value !== null);

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
        const isWeekend = isWeekendDate(date);
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
  const slotWidth = computed(() => {
    const slotMinutes = getZoomMinutes(store.zoom);
    if (slotMinutes >= 24 * 60) {
      return dayWidth.value;
    }
    return dayWidth.value / ((24 * 60) / slotMinutes);
  });

  const isZoomedOut = computed(() => isZoomedOutView(store.zoom, slotWidth.value));


  const timelineGridLines = computed(() => getTimelineGridLines(store.zoom, dayWidth.value, importantWorkTimes.value, intlLocale.value));

  const importantGridLines = computed(() =>
    getImportantGridLines(dayWidth.value, importantWorkTimes.value, intlLocale.value),
  );

  /**
   * Important-time labels flattened into one shared coordinate space across all visible day
   * columns, with colliding labels suppressed. Rendering from this list (instead of nesting
   * label markup inside each day column) means a label positioned near the edge of a day can
   * never be visually clipped by the next day column painting over it.
   */
  const flatImportantGridLines = computed<TimelineGridLine[]>(() => {
    const lines: TimelineGridLine[] = [];
    for (const day of dayHeaders.value) {
      const dayLines = day.importantGridLines ?? importantGridLines.value;
      for (const line of dayLines) {
        lines.push({ ...line, leftPx: day.left + line.leftPx });
      }
    }
    return suppressCollidingLabels(lines);
  });

  /**
   * The important work times as absolute timeline pixels, so dragging can snap to them.
   * Derived from the very lines the grid draws rather than recomputed from minutes-of-day: a
   * compact day window maps pixels to time non-linearly, and a magnet that does not sit on the
   * line the planner sees is worse than no magnet at all.
   */
  const importantSnapPoints = computed<number[]>(() =>
    dayHeaders.value.flatMap((day) =>
      (day.importantGridLines ?? importantGridLines.value).map((line) => day.left + line.leftPx),
    ),
  );

  /** Coarse vertical lines for calendar zoom levels (day / week / month). */
  const coarseGridLines = computed(() => getCoarseGridLines(store.zoom, dayWidth.value, intlLocale.value));

  const showTimeSlots = computed(() => showsTimeSlots(store.zoom));

  /**
   * Left offset of the column a day is drawn in. Falls back to the plain index for a date
   * outside the loaded range, which dayLeftMap does not cover.
   */
  function dayLeftPx(dayKey: string, dayIndex: number): number {
    return dayLeftMap.value.get(dayKey) ?? dayIndex * dayWidth.value;
  }

  function toPx(utcIso: string): number {
    const dayIndex = getDayIndexForIso(utcIso, dateRange.value.start);
    const day = addDays(dateRange.value.start, dayIndex);
    const dayKey = toDateKey(day);
    const dayLeft = dayLeftPx(dayKey, dayIndex);

    // A hidden weekend column has no width; anything inside it collapses onto its left edge.
    if (!store.showWeekends && isWeekendDate(day)) {
      return dayLeft;
    }

    const window = dayWindows.value?.get(dayKey);
    if (window) {
      return dayLeft + timeToPxCompact(utcIso, window, dayWidth.value);
    }

    const fracMs = new Date(utcIso).getTime() - day.getTime();
    return dayLeft + (fracMs / (24 * 60 * 60 * 1000)) * dayWidth.value;
  }

  /** The visible day column that contains px, with that column's own left offset. */
  function dayColumnAtPx(px: number): { day: Date, dayKey: string, dayLeft: number } {
    const first = dateRange.value.start;
    let best = { day: first, dayKey: toDateKey(first), dayLeft: 0 };

    for (const day of visibleDays.value) {
      if (!store.showWeekends && isWeekendDate(day)) {
        continue;
      }
      const dayKey = toDateKey(day);
      const dayLeft = dayLeftMap.value.get(dayKey) ?? 0;
      if (dayLeft <= px) {
        best = { day, dayKey, dayLeft };
      }
    }

    return best;
  }

  function toIso(px: number, snap = true): string {
    const snapMinutes = snap ? SNAP_MINUTES : 0;
    const { day, dayKey, dayLeft } = dayColumnAtPx(px);
    const pxInDay = Math.max(0, px - dayLeft);

    const window = dayWindows.value?.get(dayKey);
    if (window) {
      return pxToUtcIsoCompact(pxInDay, window, dayWidth.value, snapMinutes);
    }

    return pxToUtcIsoMath(pxInDay, day, dayWidth.value, snapMinutes);
  }

  const currentTimeIndicator = computed<CompactCurrentTimeIndicator | null>(() => {
    const now = new Date();
    if (now < dateRange.value.start || now >= dateRange.value.end) {
      return null;
    }

    if (dayWindows.value) {
      for (const date of visibleDays.value) {
        const isWeekend = isWeekendDate(date);
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
    columnMode,
    periodMode,
    primaryBorder,
    timelineGridLines,
    importantGridLines,
    flatImportantGridLines,
    coarseGridLines,
    showTimeSlots,
    isZoomedOut,
    isCompactRowLayout,
    usesDayWindow,
    dayWindows,
    rowLabelWidth: ROW_LABEL_WIDTH,
    currentTimePx,
    currentTimeIndicator,
    importantSnapPoints,
    toPx,
    toIso,
    timeToPx: toPx,
    getBlockLayout,
    pxToUtcIso,
  };
}

export type TimelineApi = ReturnType<typeof createTimeline>;

const TIMELINE_KEY: InjectionKey<TimelineApi> = Symbol('planning-timeline');

/**
 * Creates the single timeline instance for a board and shares it with the whole subtree.
 * Call once, from the board root.
 */
export function provideTimeline(sources: TimelineSources = {}): TimelineApi {
  const timeline = createTimeline(sources);
  provide(TIMELINE_KEY, timeline);
  return timeline;
}

/**
 * The board's timeline. Returns the shared instance when called inside a board; falls back to a
 * private one so the composable still works in isolation (tests, storybook, a stray component).
 */
export function useTimeline(): TimelineApi {
  return inject(TIMELINE_KEY, null) ?? createTimeline();
}
