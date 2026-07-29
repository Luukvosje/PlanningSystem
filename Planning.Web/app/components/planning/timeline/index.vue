<script setup lang="ts">
import type { TimelineRow } from '~/types/planning';
import type { UnavailablePeriod } from '~/types/availability';
import { useResizeObserver } from '@vueuse/core';
import { nextTick } from 'vue';
import {
  getDayCount,
  getDayWidth,
  ROW_LABEL_WIDTH,
} from '~/utils/planning/timelineMath';
import { startOfMonth } from '~/utils/planning/dateUtils';

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

useResizeObserver(containerRef, (entries: readonly ResizeObserverEntry[]) => {
  containerWidth.value = entries[0]?.contentRect.width ?? 0;
  syncViewport();
});

provide('timelineContainerWidth', containerWidth);

const rowRecordsMap = computed(() =>
  new Map(props.rows.map((row) => [row.id, row.records])),
);
provide('timelineRowRecords', rowRecordsMap);

const availabilityPeriodsRef = computed(() => props.availabilityPeriods ?? []);
provide('timelineAvailabilityPeriods', availabilityPeriodsRef);

const dayCount = computed(() =>
  getDayCount(store.loadedRangeStart, store.loadedRangeEnd),
);
const dayWidth = computed(() =>
  getDayWidth(store.zoom, dayCount.value, containerWidth.value, store.slotScale),
);

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
    // Prefer post-update dayWidth; fall back to previous if width briefly collapses.
    const width = dayWidth.value > 0 ? dayWidth.value : widthBefore;
    el.scrollLeft += addedDays * width;
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

  const dayIndex = getDayCount(rangeStart, target);
  const targetLeft = Math.max(
    0,
    ROW_LABEL_WIDTH + dayIndex * dayWidth.value - el.clientWidth / 3,
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
			class="w-max min-w-full flex flex-col"
		>
			<PlanningTimelineHeader />
			<div class="relative">
				<PlanningTimelineCurrentTimeIndicator />
				<PlanningTimelineRow
					v-for="row in rows"
					:key="row.id"
					:row-id="row.id"
					:label="row.label"
					:records="row.records"
					:row-customer-id="store.rowMode === 'customer' && row.id !== '__unassigned__' ? row.id : null"
					:availability-periods="availabilityPeriods"
				/>
			</div>
		</div>
	</div>
</template>
