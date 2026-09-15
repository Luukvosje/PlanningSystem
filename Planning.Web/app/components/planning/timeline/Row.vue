<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning';
import type { UnavailablePeriod } from '~/types/availability';
import AvailabilityOverlay from './AvailabilityOverlay.vue';
import { getRowHeight, layoutOverlappingBlocks } from '~/utils/planning/overlapLayout';
import { SNAP_MINUTES } from '~/utils/planning/timelineMath';
import {
  collectBlockSnapPoints,
  getRowAvailabilitySnapPoints,
  getRowBlockBounds,
  mergeSnapPoints,
  snapPxToTimeline,
} from '~/utils/planning/blockSnap';
import { getUnavailableOverlaysForMatrix } from '~/utils/planning/availabilityMath';
import { OPEN_SHIFT_ROW_ID, UNASSIGNED_CUSTOMER_ROW_ID } from '~/utils/planning/constants';

const props = defineProps<{
  rowId: string
  label: string
  records: PlanningRecord[]
  rowCustomerId?: string | null
  availabilityPeriods?: UnavailablePeriod[]
}>();

const { t } = useI18n();
const { toPx, toIso, timelineWidth, rowLabelWidth, dateRange, dayWidth, importantSnapPoints } = useTimeline();
const store = usePlanningStore();
const { canManage } = usePlanningPermissions();
const { dragHoverRowId } = useDragPlanning();
const { data: users } = useUsers();

const isDropTarget = computed(() => dragHoverRowId.value === props.rowId);

const DRAG_THRESHOLD_PX = 4;
const minDurationPx = computed(() => (SNAP_MINUTES / (24 * 60)) * dayWidth.value);
const minDurationMs = SNAP_MINUTES * 60 * 1000;

const selection = ref<{ startPx: number, endPx: number } | null>(null);
const isSelecting = ref(false);

const layouts = computed(() =>
  layoutOverlappingBlocks(props.records, toPx, store.zoom, store.rowLayout),
);

// Horizontal viewport culling: only render blocks visible in the current scroll viewport.
const visibleRecords = computed(() => {
  const { scrollLeft, clientWidth } = store.timelineViewport;
  if (clientWidth <= 0) {
    return props.records;
  }
  const viewLeft = scrollLeft - rowLabelWidth;
  const viewRight = viewLeft + clientWidth;
  // Add generous buffer (2 screens) so blocks near the edge are already mounted when
  // the user scrolls, avoiding pop-in during fast scrolling.
  const buffer = clientWidth;
  return props.records.filter((record) => {
    const layout = layouts.value.get(record.id);
    if (!layout) {
      return true;
    }
    return layout.leftPx + layout.widthPx >= viewLeft - buffer &&
      layout.leftPx <= viewRight + buffer;
  });
});

const rowHeight = computed(() => {
  const maxLane = Math.max(...[...layouts.value.values()].map((l) => l.lane), 0);
  return getRowHeight(maxLane + 1, store.zoom, store.rowLayout);
});

const availabilityOverlays = computed(() => {
  if (store.rowMode !== 'resource' || !props.availabilityPeriods?.length) {
    return [];
  }

  return getUnavailableOverlaysForMatrix(
    [],
    props.rowId,
    dateRange.value.start,
    dateRange.value.end,
    dayWidth.value,
    props.availabilityPeriods,
    t,
  );
});

const selectionPreview = computed(() => {
  if (!selection.value) {
    return null;
  }
  const left = Math.min(selection.value.startPx, selection.value.endPx);
  const right = Math.max(selection.value.startPx, selection.value.endPx);
  const width = Math.max(right - left, minDurationPx.value);
  return { left, width };
});

const snapPoints = computed(() =>
  mergeSnapPoints(
    collectBlockSnapPoints(getRowBlockBounds(props.records, toPx)),
    getRowAvailabilitySnapPoints(
      props.rowId,
      props.availabilityPeriods ?? [],
      dateRange.value.start,
      dateRange.value.end,
      dayWidth.value,
      store.rowMode,
    ),
    importantSnapPoints.value,
  ),
);

function snapTimelinePx(px: number): number {
  return snapPxToTimeline(px, dayWidth.value, {
    snapToBlocks: store.snapToBlocks,
    snapPoints: snapPoints.value,
  });
}

function pxFromEvent(event: PointerEvent, target: HTMLElement): number {
  const rect = target.getBoundingClientRect();
  return event.clientX - rect.left;
}

function resolveAssignedUserId(): string | null {
  if (store.rowMode === 'resource') {
return props.rowId === OPEN_SHIFT_ROW_ID ? null : props.rowId;
}
  const filteredUserIds = store.filters.userIds;
  const activeUsers = (users.value ?? []).filter((u) => u.isActive !== false);
  if (filteredUserIds.length > 0) {
    return activeUsers.find((u) => u.id && filteredUserIds.includes(u.id))?.id ?? null;
  }
  return activeUsers[0]?.id ?? null;
}

function resolveCustomerId(): string | null {
  if (props.rowCustomerId) {
return props.rowCustomerId;
}
  if (store.rowMode === 'customer' && props.rowId !== UNASSIGNED_CUSTOMER_ROW_ID) {
return props.rowId;
}
  const filteredCustomerIds = store.filters.customerIds;
  if (filteredCustomerIds.length === 1) {
    return filteredCustomerIds[0]!;
  }
  return null;
}

function openCreate(startPx: number, endPx: number) {
  const dragged = Math.abs(endPx - startPx) >= DRAG_THRESHOLD_PX;
  const minPx = Math.min(startPx, endPx);
  const maxPx = Math.max(startPx, endPx);

  let startUtc: string;
  let endUtc: string;

  if (dragged) {
    startUtc = toIso(minPx, false);
    endUtc = toIso(maxPx, false);
    const startTime = new Date(startUtc).getTime();
    const endTime = new Date(endUtc).getTime();
    if (endTime - startTime < minDurationMs) {
      endUtc = new Date(startTime + minDurationMs).toISOString();
    }
  } else {
    startUtc = toIso(startPx, false);
    endUtc = new Date(new Date(startUtc).getTime() + minDurationMs).toISOString();
  }

  store.openCreateSidebar({
    assignedUserId: resolveAssignedUserId(),
    customerId: resolveCustomerId(),
    status: store.filters.statuses[0],
    startUtc,
    endUtc,
  });
}

function onRowPointerDown(event: PointerEvent) {
  if (!canManage.value || event.button !== 0) {
    return;
  }
  if ((event.target as HTMLElement).closest('[data-timeline-block]')) {
    return;
  }

  event.preventDefault();

  const target = event.currentTarget as HTMLElement;
  const startPx = snapTimelinePx(pxFromEvent(event, target));

  selection.value = { startPx, endPx: startPx };
  isSelecting.value = true;

  const onMove = (e: PointerEvent) => {
    const endPx = snapTimelinePx(pxFromEvent(e, target));
    selection.value = { startPx, endPx };
  };

  const onUp = (e: PointerEvent) => {
    document.removeEventListener('pointermove', onMove);
    document.removeEventListener('pointerup', onUp);
    isSelecting.value = false;

    const endPx = snapTimelinePx(pxFromEvent(e, target));
    selection.value = null;
    openCreate(startPx, endPx);
  };

  document.addEventListener('pointermove', onMove);
  document.addEventListener('pointerup', onUp);
}
</script>

<template>
	<div
		class="flex border-b border-default/25"
		:style="{ height: `${rowHeight}px` }"
	>
		<div
			class="sticky left-0 z-1000 shrink-0 border-r border-default bg-default px-3 flex items-center"
			:style="{ width: `${rowLabelWidth}px` }"
		>
			<span class="text-sm font-medium truncate">{{ label }}</span>
		</div>

		<div
			class="relative flex-1 transition-colors"
			data-timeline-row
			:data-row-id="rowId"
			:data-row-customer-id="rowCustomerId ?? ''"
			:class="[
				isSelecting ? 'select-none cursor-crosshair' : canManage ? 'cursor-cell' : '',
				isDropTarget ? 'bg-brand/10 ring-1 ring-inset ring-brand/40' : '',
			]"
			:style="{ minWidth: `${timelineWidth}px` }"
			@pointerdown="onRowPointerDown"
		>
			<div
				v-if="selectionPreview"
				class="absolute top-1 bottom-1 z-20 rounded-md border border-brand/50 bg-brand/20 pointer-events-none"
				:style="{
					left: `${selectionPreview.left}px`,
					width: `${selectionPreview.width}px`,
				}"
			/>

			<AvailabilityOverlay
				v-if="store.showAvailability"
				:overlays="availabilityOverlays"
			/>

			<PlanningTimelineBlock
				v-for="record in visibleRecords"
				:key="record.id"
				:record="record"
				:layout="layouts.get(record.id)!"
				:selected="store.selectedPlanningId === record.id"
				:row-id="rowId"
				:availability-periods="availabilityPeriods"
			/>
		</div>
	</div>
</template>
