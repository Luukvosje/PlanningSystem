<script setup lang="ts">
import type { TimelineRow } from '~/types/planning';
import type { UnavailablePeriod } from '~/types/availability';
import { useResizeObserver } from '@vueuse/core';
import { nextTick } from 'vue';
import {
  getLaneHeight,
  ROW_LABEL_WIDTH,
  BLOCK_PADDING,
} from '~/utils/planning/timelineMath';
import { addDays, startOfMonth } from '~/utils/planning/dateUtils';

const props = defineProps<{
  rows: TimelineRow[]
  availabilityPeriods?: UnavailablePeriod[]
}>();

const store = usePlanningStore();

const containerRef = useTemplateRef<HTMLElement>('containerRef');
const containerWidth = ref(0);

const SCROLL_EDGE_PX = 240;
const extending = ref(false);
const didInitialScroll = ref(false);

// ─── Vertical virtualisation ────────────────────────────────────────────────
// We estimate the minimum row height based on current zoom/layout (single-lane).
// Rows with many overlapping blocks are taller, so this is a lower-bound estimate.
// We use a generous vertical buffer (1.5× viewport height) so rows are already
// mounted before they scroll into view.
const verticalScrollTop = ref(0);
const verticalClientHeight = ref(0);
const VERTICAL_BUFFER_ROWS = 3; // extra rows to keep rendered above/below viewport

const estimatedRowHeight = computed(() => {
  const lh = getLaneHeight(store.zoom, store.rowLayout);
  return lh + BLOCK_PADDING * 2;
});

/** Cumulative top-pixel offset for each row (index → top px). */
const rowTops = computed(() => {
  const tops: number[] = [];
  let top = 0;
  for (const _row of props.rows) {
    tops.push(top);
    top += estimatedRowHeight.value;
  }
  return tops;
});

const totalRowsHeight = computed(() =>
  props.rows.length * estimatedRowHeight.value,
);

const visibleRowRange = computed(() => {
  const viewTop = verticalScrollTop.value;
  const viewBottom = viewTop + (verticalClientHeight.value || 600);
  const bufferPx = VERTICAL_BUFFER_ROWS * estimatedRowHeight.value;

  let start = 0;
  let end = props.rows.length;

  for (let i = 0; i < rowTops.value.length; i++) {
    if (rowTops.value[i]! + estimatedRowHeight.value < viewTop - bufferPx) {
      start = i + 1;
    } else {
      break;
    }
  }

  for (let i = rowTops.value.length - 1; i >= 0; i--) {
    if (rowTops.value[i]! > viewBottom + bufferPx) {
      end = i;
    } else {
      break;
    }
  }

  return { start, end };
});

const spacerTopHeight = computed(() =>
  rowTops.value[visibleRowRange.value.start] ?? 0,
);
const spacerBottomHeight = computed(() =>
  totalRowsHeight.value - (rowTops.value[visibleRowRange.value.end] ?? totalRowsHeight.value),
);
const virtualRows = computed(() =>
  props.rows.slice(visibleRowRange.value.start, visibleRowRange.value.end),
);

useResizeObserver(containerRef, (entries: readonly ResizeObserverEntry[]) => {
  const entry = entries[0];
  containerWidth.value = entry?.contentRect.width ?? 0;
  verticalClientHeight.value = entry?.contentRect.height ?? 0;
  syncViewport();
});

provide('timelineContainerWidth', containerWidth);

const rowRecordsMap = computed(() =>
  new Map(props.rows.map((row) => [row.id, row.records])),
);
provide('timelineRowRecords', rowRecordsMap);

const availabilityPeriodsRef = computed(() => props.availabilityPeriods ?? []);
provide('timelineAvailabilityPeriods', availabilityPeriodsRef);

const { dayWidth, toPx } = useTimeline();

function syncViewport() {
  const el = containerRef.value;
  if (!el) {
    return;
  }

  store.updateTimelineViewport({
    scrollLeft: el.scrollLeft,
    clientWidth: el.clientWidth,
    dayWidth: dayWidth.value,
  });
}

async function extendPrevious() {
  if (extending.value) {
    return;
  }
  extending.value = true;
  try {
    const widthBefore = dayWidth.value;
    const addedDays = store.extendPreviousMonth();
    await nextTick();
    const el = containerRef.value;
    if (!el || addedDays <= 0) {
      return;
    }
    const width = dayWidth.value > 0 ? dayWidth.value : widthBefore;
    const rangeStart = store.loadedRangeStart;
    let visibleAdded = addedDays;
    if (!store.showWeekends) {
      visibleAdded = 0;
      for (let i = 0; i < addedDays; i++) {
        const d = addDays(rangeStart, i);
        if (d.getDay() !== 0 && d.getDay() !== 6) {
          visibleAdded++;
        }
      }
    }
    el.scrollLeft += visibleAdded * width;
    syncViewport();
  } finally {
    extending.value = false;
  }
}

async function extendNext() {
  if (extending.value) {
    return;
  }
  extending.value = true;
  try {
    store.extendNextMonth();
    await nextTick();
    syncViewport();
  } finally {
    extending.value = false;
  }
}

function onScroll() {
  const el = containerRef.value;
  if (!el || extending.value || !didInitialScroll.value) {
    return;
  }

  verticalScrollTop.value = el.scrollTop;
  verticalClientHeight.value = el.clientHeight;
  syncViewport();

  if (el.scrollLeft <= SCROLL_EDGE_PX) {
    void extendPrevious();
    return;
  }

  const distanceToRight = el.scrollWidth - el.clientWidth - el.scrollLeft;
  if (distanceToRight <= SCROLL_EDGE_PX) {
    void extendNext();
  }
}

function scrollToDate(date: Date) {
  const el = containerRef.value;
  if (!el || dayWidth.value <= 0) {
    return;
  }

  const target = new Date(date);
  target.setHours(0, 0, 0, 0);
  const rangeStart = store.loadedRangeStart;
  const rangeEnd = store.loadedRangeEnd;
  if (target < rangeStart || target >= rangeEnd) {
    return;
  }

  const noon = new Date(target);
  noon.setHours(12, 0, 0, 0);
  const px = toPx(noon.toISOString());
  const targetLeft = Math.max(
    0,
    ROW_LABEL_WIDTH + px - el.clientWidth / 3,
  );
  el.scrollLeft = targetLeft;
  syncViewport();
}

async function ensureDateInRange(date: Date) {
  const target = new Date(date);
  target.setHours(0, 0, 0, 0);

  if (target >= store.loadedRangeStart && target < store.loadedRangeEnd) {
    return;
  }

  const monthsBefore = (store.loadedRangeStart.getFullYear() - target.getFullYear()) * 12 +
    (store.loadedRangeStart.getMonth() - target.getMonth());
  const monthsAfter = (target.getFullYear() - store.loadedRangeEnd.getFullYear()) * 12 +
    (target.getMonth() - store.loadedRangeEnd.getMonth()) +
    1;

  // Far jumps: reset to the target month instead of extending many months.
  if (monthsBefore > 2 || monthsAfter > 2) {
    store.currentDate = startOfMonth(target);
    store.resetLoadedRangeToMonth(target);
    await nextTick();
    return;
  }

  while (target < store.loadedRangeStart) {
    await extendPrevious();
  }
  while (target >= store.loadedRangeEnd) {
    await extendNext();
  }
}

async function handleScrollRequest(date: Date) {
  await ensureDateInRange(date);
  await nextTick();
  scrollToDate(date);
  didInitialScroll.value = true;
}

watch(
  [containerWidth, dayWidth, () => store.loadedRangeStart, () => store.loadedRangeEnd],
  async () => {
    if (didInitialScroll.value || containerWidth.value <= 0 || dayWidth.value <= 0) {
      syncViewport();
      return;
    }
    await nextTick();
    scrollToDate(new Date());
    didInitialScroll.value = true;
  },
  { flush: 'post' },
);

watch(
  () => store.scrollRequest,
  (request) => {
    if (!request) {
      return;
    }
    void handleScrollRequest(request.date);
  },
);

watch(dayWidth, () => {
  syncViewport();
});

watch(
  () => startOfMonth(store.currentDate).getTime(),
  (monthKey, previousKey) => {
    if (previousKey == null) {
      return;
    }
    const date = store.currentDate;
    if (date < store.loadedRangeStart || date >= store.loadedRangeEnd) {
      store.resetLoadedRangeToMonth(date);
    }
    void monthKey;
  },
);
</script>

<template>
	<div
		ref="containerRef"
		class="relative overflow-auto rounded-xl border border-default bg-default shadow-sm h-full"
		@scroll.passive="onScroll"
	>
		<div class="w-max min-w-full flex flex-col">
			<PlanningTimelineHeader />
			<div
				v-if="rows.length === 0"
				class="p-12 text-center"
			>
				<UIcon
					name="i-lucide-calendar-off"
					class="size-10 text-muted mx-auto mb-3"
				/>
				<p class="text-muted">
					Geen resources om weer te geven.
				</p>
			</div>
			<div
				v-else
				class="relative"
			>
				<PlanningTimelineCurrentTimeIndicator />
				<!-- Top spacer for virtualised rows above viewport -->
				<div
					v-if="spacerTopHeight > 0"
					:style="{ height: `${spacerTopHeight}px` }"
					aria-hidden="true"
				/>
				<PlanningTimelineRow
					v-for="row in virtualRows"
					:key="row.id"
					:row-id="row.id"
					:label="row.label"
					:records="row.records"
					:row-customer-id="store.rowMode === 'customer' && row.id !== '__unassigned__' ? row.id : null"
					:availability-periods="availabilityPeriods"
				/>
				<!-- Bottom spacer for virtualised rows below viewport -->
				<div
					v-if="spacerBottomHeight > 0"
					:style="{ height: `${spacerBottomHeight}px` }"
					aria-hidden="true"
				/>
			</div>
		</div>
	</div>
</template>
