import type { PlanningRecord } from '~/types/planning';
import { addDays, getIntlLocale, getMonday } from '~/utils/planning/dateUtils';
import { getRowHeight, layoutOverlappingBlocks } from '~/utils/planning/overlapLayout';
import {
  filterRecordsForDay,
  getMixedBlockLayout,
  getMixedCurrentTimePx,
  getMixedTimelineWidth,
  getHourMarkers,
  timeToMixedPx,
} from '~/utils/planning/mixedTimelineMath';

export function useMixedTimeline(records: Ref<PlanningRecord[]>) {
  const store = usePlanningStore();
  const { locale } = useI18n();

  const timelineWidth = computed(() => getMixedTimelineWidth());
  const hourMarkers = computed(() => getHourMarkers(getIntlLocale(locale.value)));

  const weekDays = computed(() => {
    const monday = getMonday(store.currentDate);
    return Array.from({ length: 7 }, (_, index) => addDays(monday, index));
  });

  const dayRecords = computed(() =>
    filterRecordsForDay(records.value, store.mixedSelectedDay),
  );

  // Reuse greedy lane assignment: overlapping blocks stack vertically.
  const layouts = computed(() => {
    const timeToPx = (utcIso: string) => timeToMixedPx(utcIso);
    return layoutOverlappingBlocks(dayRecords.value, timeToPx);
  });

  const contentHeight = computed(() => {
    const maxLane = Math.max(
      ...[...layouts.value.values()].map((layout) => layout.lane),
      0,
    );
    return getRowHeight(maxLane + 1);
  });

  const currentTimePx = computed(() =>
    getMixedCurrentTimePx(store.mixedSelectedDay),
  );

  function getBlockLayout(record: PlanningRecord) {
    const base = layouts.value.get(record.id);
    if (!base) {
return null;
}

    const clipped = getMixedBlockLayout(record, timelineWidth.value);
    return {
      ...base,
      leftPx: clipped.leftPx,
      widthPx: clipped.widthPx,
    };
  }

  function isSelectedDay(day: Date): boolean {
    return day.toDateString() === store.mixedSelectedDay.toDateString();
  }

  function selectDay(day: Date) {
    store.setMixedSelectedDay(day);
  }

  watch(
    () => store.currentDate,
    () => {
      if (store.displayMode !== 'mixed') {
return;
}
      const weekStart = getMonday(store.currentDate);
      const weekEnd = addDays(weekStart, 7);
      const selected = store.mixedSelectedDay;
      if (selected < weekStart || selected >= weekEnd) {
        store.setMixedSelectedDay(weekStart);
      }
    },
  );

  return {
    timelineWidth,
    hourMarkers,
    weekDays,
    dayRecords,
    layouts,
    contentHeight,
    currentTimePx,
    getBlockLayout,
    isSelectedDay,
    selectDay,
  };
}
