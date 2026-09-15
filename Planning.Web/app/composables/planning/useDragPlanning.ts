import type { UnavailablePeriod } from '~/types/availability';
import type { PlanningRecord } from '~/types/planning';
import { pxFromPointerEvent } from '~/utils/planning/timelineMath';
import { OPEN_SHIFT_ROW_ID, UNASSIGNED_CUSTOMER_ROW_ID } from '~/utils/planning/constants';
import {
  collectBlockSnapPoints,
  getRowAvailabilitySnapPoints,
  getRowBlockBounds,
  mergeSnapPoints,
  snapDragLeftPx,
} from '~/utils/planning/blockSnap';

const DRAG_THRESHOLD_PX = 4;

const draggingId = ref<string | null>(null);
const dragHoverRowId = ref<string | null>(null);
const dragPreview = ref<{
  recordId: string
  leftPx: number
  widthPx: number
  offsetY: number
} | null>(null);
const suppressClickRecordId = ref<string | null>(null);

export function suppressPlanningBlockClick(recordId: string) {
  suppressClickRecordId.value = recordId;
}

function resolveDropTarget(_clientX: number, clientY: number) {
  const rows = document.querySelectorAll<HTMLElement>('[data-timeline-row]');
  for (const row of rows) {
    const rect = row.getBoundingClientRect();
    if (clientY >= rect.top && clientY < rect.bottom) {
      return {
        rowId: row.dataset.rowId ?? '',
        rowCustomerId: row.dataset.rowCustomerId ?? null,
      };
    }
  }
  return null;
}

function resolveMoveTarget(
  record: PlanningRecord,
  sourceRowId: string,
  dropTarget: { rowId: string, rowCustomerId: string | null } | null,
  rowMode: 'resource' | 'customer',
) {
  const targetRowId = dropTarget?.rowId || sourceRowId;

  if (rowMode === 'resource') {
    return {
      assignedUserId: targetRowId === OPEN_SHIFT_ROW_ID ? null : targetRowId,
      customerId: record.customerId ?? null,
    };
  }

  const customerId = targetRowId === UNASSIGNED_CUSTOMER_ROW_ID ? null : targetRowId;
  return {
    assignedUserId: record.assignedUserId ?? null,
    customerId,
  };
}

export function useDragPlanning() {
  const store = usePlanningStore();
  const api = usePlanningApi();
  const toast = useToast();
  const { canManage } = usePlanningPermissions();
  const { pxToUtcIso, dayWidth, timeToPx, dateRange, importantSnapPoints } = useTimeline();
  const rowRecordsMap = inject<ComputedRef<Map<string, PlanningRecord[]>>>(
    'timelineRowRecords',
    computed(() => new Map()),
  );
  const availabilityPeriods = inject<ComputedRef<UnavailablePeriod[]>>(
    'timelineAvailabilityPeriods',
    computed(() => []),
  );

  function getSnapPoints(rowId: string, excludeRecordId: string): number[] {
    const records = rowRecordsMap.value.get(rowId) ?? [];
    return mergeSnapPoints(
      collectBlockSnapPoints(getRowBlockBounds(records, timeToPx, excludeRecordId)),
      getRowAvailabilitySnapPoints(
        rowId,
        availabilityPeriods.value,
        dateRange.value.start,
        dateRange.value.end,
        dayWidth.value,
        store.rowMode,
      ),
      importantSnapPoints.value,
    );
  }

  function startDrag(
    event: PointerEvent,
    record: PlanningRecord,
    sourceRowId: string,
    layout: { leftPx: number, widthPx: number },
  ) {
    if (!canManage.value) {
      return;
    }
    if ((event.target as HTMLElement).dataset.resize) {
      return;
    }

    const rowEl = (event.target as HTMLElement).closest<HTMLElement>('[data-timeline-row]');
    if (!rowEl) {
      return;
    }

    const grabOffsetPx = pxFromPointerEvent(event, rowEl) - layout.leftPx;
    const startClientY = event.clientY;
    const widthPx = layout.widthPx;
    let leftPx = layout.leftPx;
    let isDragActive = false;

    const updatePreview = (offsetY: number) => {
      dragPreview.value = { recordId: record.id, leftPx, widthPx, offsetY };
    };

    const onMove = (e: PointerEvent) => {
      const mousePx = pxFromPointerEvent(e, rowEl);
      const nextLeftPx = mousePx - grabOffsetPx;
      const offsetY = e.clientY - startClientY;

      if (!isDragActive) {
        const movedH = Math.abs(nextLeftPx - layout.leftPx);
        const movedV = Math.abs(offsetY);
        if (movedH < DRAG_THRESHOLD_PX && movedV < DRAG_THRESHOLD_PX) {
          return;
        }
        isDragActive = true;
        draggingId.value = record.id;
        store.selectPlanning(record.id, { openSidebar: false });
        rowEl.setPointerCapture(e.pointerId);
      }

      const dropTarget = resolveDropTarget(e.clientX, e.clientY);
      leftPx = snapDragLeftPx(nextLeftPx, widthPx, dayWidth.value, {
        snapToBlocks: store.snapToBlocks,
        snapPoints: getSnapPoints(dropTarget?.rowId ?? sourceRowId, record.id),
      });
      dragHoverRowId.value = dropTarget?.rowId ?? null;
      updatePreview(offsetY);
    };

    const onUp = async (e: PointerEvent) => {
      document.removeEventListener('pointermove', onMove);
      document.removeEventListener('pointerup', onUp);

      if (rowEl.hasPointerCapture(e.pointerId)) {
        rowEl.releasePointerCapture(e.pointerId);
      }

      draggingId.value = null;
      dragHoverRowId.value = null;
      dragPreview.value = null;

      if (!isDragActive) {
        return;
      }

      e.preventDefault();
      suppressPlanningBlockClick(record.id);

      const times = {
        startUtc: pxToUtcIso(leftPx, false),
        endUtc: pxToUtcIso(leftPx + widthPx, false),
      };
      const dropTarget = resolveDropTarget(e.clientX, e.clientY);
      const moveTarget = resolveMoveTarget(record, sourceRowId, dropTarget, store.rowMode);

      store.applyOptimisticPatch(record.id, {
        ...times,
        assignedUserId: moveTarget.assignedUserId,
        customerId: moveTarget.customerId,
      });

      try {
        await api.move(record.id, {
          assignedUserId: moveTarget.assignedUserId,
          customerId: moveTarget.customerId,
          startUtc: times.startUtc,
          endUtc: times.endUtc,
        });
        await api.invalidatePlanning();
        store.clearOptimisticPatch(record.id);
      } catch {
        store.clearOptimisticPatch(record.id);
        toast.add({ title: 'Verplaatsen mislukt', color: 'error' });
      }
    };

    document.addEventListener('pointermove', onMove);
    document.addEventListener('pointerup', onUp);
  }

  function consumeClickSuppression(recordId: string) {
    if (suppressClickRecordId.value === recordId) {
      suppressClickRecordId.value = null;
      return true;
    }
    return false;
  }

  return {
    draggingId,
    dragHoverRowId,
    dragPreview,
    startDrag,
    consumeClickSuppression,
  };
}
