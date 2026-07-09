<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning'
import type { AvailabilityEntry } from '~/types/availability'
import AvailabilityOverlay from './AvailabilityOverlay.vue'
import { getRowHeight, layoutOverlappingBlocks } from '~/utils/planning/overlapLayout'
import { SNAP_MINUTES, snapPx } from '~/utils/planning/timelineMath'
import { getUnavailableOverlaysForMatrix } from '~/utils/planning/availabilityMath'

const props = defineProps<{
  rowId: string
  label: string
  records: PlanningRecord[]
  rowCustomerId?: string | null
  availabilityEntries?: AvailabilityEntry[]
}>()

const { timeToPx, timelineWidth, rowLabelWidth, pxToUtcIso, dateRange, dayWidth } = useTimeline()
const store = usePlanningStore()
const { canManage } = usePlanningPermissions()
const { dragHoverRowId } = useDragPlanning()
const { data: users } = useUsers()

const isDropTarget = computed(() => dragHoverRowId.value === props.rowId)

const DRAG_THRESHOLD_PX = 4
const minDurationPx = computed(() => (SNAP_MINUTES / (24 * 60)) * dayWidth.value)
const minDurationMs = SNAP_MINUTES * 60 * 1000

const selection = ref<{ startPx: number, endPx: number } | null>(null)
const isSelecting = ref(false)

const layouts = computed(() =>
  layoutOverlappingBlocks(props.records, timeToPx),
)

const rowHeight = computed(() => {
  const maxLane = Math.max(...[...layouts.value.values()].map(l => l.lane), 0)
  return getRowHeight(maxLane + 1)
})

const availabilityOverlays = computed(() => {
  if (store.rowMode !== 'resource' || !props.availabilityEntries?.length) {
    return []
  }

  return getUnavailableOverlaysForMatrix(
    props.availabilityEntries,
    props.rowId,
    dateRange.value.start,
    dateRange.value.end,
    dayWidth.value,
  )
})

const selectionPreview = computed(() => {
  if (!selection.value) return null
  const left = Math.min(selection.value.startPx, selection.value.endPx)
  const right = Math.max(selection.value.startPx, selection.value.endPx)
  const width = Math.max(right - left, minDurationPx.value)
  return { left, width }
})

function pxFromEvent(event: PointerEvent, target: HTMLElement): number {
  const rect = target.getBoundingClientRect()
  return event.clientX - rect.left
}

function resolveAssignedUserId(): string | null {
  if (store.rowMode === 'resource') return props.rowId
  return users.value?.[0]?.id ?? null
}

function openCreatePopover(event: PointerEvent, startPx: number, endPx: number) {
  const dragged = Math.abs(endPx - startPx) >= DRAG_THRESHOLD_PX
  const minPx = Math.min(startPx, endPx)
  const maxPx = Math.max(startPx, endPx)

  let startUtc: string
  let endUtc: string

  if (dragged) {
    startUtc = pxToUtcIso(minPx)
    endUtc = pxToUtcIso(maxPx)
    const startTime = new Date(startUtc).getTime()
    const endTime = new Date(endUtc).getTime()
    if (endTime - startTime < minDurationMs) {
      endUtc = new Date(startTime + minDurationMs).toISOString()
    }
  }
  else {
    startUtc = pxToUtcIso(startPx)
    endUtc = new Date(new Date(startUtc).getTime() + minDurationMs).toISOString()
  }

  const assignedUserId = resolveAssignedUserId()
  if (!assignedUserId) return

  store.openCreatePopover({
    x: event.clientX,
    y: event.clientY,
    assignedUserId,
    customerId: props.rowCustomerId ?? (store.rowMode === 'customer' && props.rowId !== '__unassigned__' ? props.rowId : null),
    startUtc,
    endUtc,
  })
}

function onRowPointerDown(event: PointerEvent) {
  if (!canManage.value || event.button !== 0) return
  if ((event.target as HTMLElement).closest('[data-timeline-block]')) return

  event.preventDefault()

  const target = event.currentTarget as HTMLElement
  const startPx = snapPx(pxFromEvent(event, target), dayWidth.value)

  selection.value = { startPx, endPx: startPx }
  isSelecting.value = true

  const onMove = (e: PointerEvent) => {
    const endPx = snapPx(pxFromEvent(e, target), dayWidth.value)
    selection.value = { startPx, endPx }
  }

  const onUp = (e: PointerEvent) => {
    document.removeEventListener('pointermove', onMove)
    document.removeEventListener('pointerup', onUp)
    isSelecting.value = false

    const endPx = snapPx(pxFromEvent(e, target), dayWidth.value)
    selection.value = null
    openCreatePopover(e, startPx, endPx)
  }

  document.addEventListener('pointermove', onMove)
  document.addEventListener('pointerup', onUp)
}
</script>

<template>
  <div class="flex border-b border-default/60" :style="{ height: `${rowHeight}px` }">
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
        isDropTarget ? 'bg-primary/10 ring-1 ring-inset ring-primary/40' : '',
      ]"
      :style="{ minWidth: `${timelineWidth}px` }"
      @pointerdown="onRowPointerDown"
    >
      <div
        v-if="selectionPreview"
        class="absolute top-1 bottom-1 z-20 rounded-md border border-primary/50 bg-primary/20 pointer-events-none"
        :style="{
          left: `${selectionPreview.left}px`,
          width: `${selectionPreview.width}px`,
        }"
      />

      <AvailabilityOverlay :overlays="availabilityOverlays" />

      <PlanningTimelineBlock
        v-for="record in records"
        :key="record.id"
        :record="record"
        :layout="layouts.get(record.id)!"
        :selected="store.selectedPlanningId === record.id"
        :row-id="rowId"
        :availability-entries="availabilityEntries"
      />
    </div>
  </div>
</template>
