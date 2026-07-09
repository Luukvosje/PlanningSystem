<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning'
import type { AvailabilityEntry } from '~/types/availability'
import { formatTimeRange, getBlockColor } from '~/utils/planning/dateUtils'

const props = defineProps<{
  record: PlanningRecord
  layout: { leftPx: number, widthPx: number, topPx: number, heightPx: number }
  selected?: boolean
  rowId: string
  availabilityEntries?: AvailabilityEntry[]
}>()

const store = usePlanningStore()
const { canManage } = usePlanningPermissions()
const { data: users } = useUsers()
const { checkRecord } = useAvailabilityWarning(
  computed(() => props.availabilityEntries ?? []),
  users,
)
const { startDrag, draggingId, dragPreview, consumeClickSuppression } = useDragPlanning()
const { startResize, resizingId, resizePreview } = useResizePlanning()
const { rowLabelWidth } = useTimeline()
const headerHeight = inject<Ref<number>>('timelineHeaderHeight', ref(0))

const blockColor = computed(() => getBlockColor(props.record.color, props.record.status))
const availabilityWarning = computed(() => checkRecord(props.record))
const isDragging = computed(() => draggingId.value === props.record.id)
const isResizing = computed(() => resizingId.value === props.record.id)

const displayLayout = computed(() => {
  if (isDragging.value && dragPreview.value?.recordId === props.record.id) {
    return {
      ...props.layout,
      leftPx: dragPreview.value.leftPx,
      widthPx: dragPreview.value.widthPx,
      offsetY: dragPreview.value.offsetY,
    }
  }
  if (isResizing.value && resizePreview.value?.recordId === props.record.id) {
    return {
      ...props.layout,
      leftPx: resizePreview.value.leftPx,
      widthPx: resizePreview.value.widthPx,
      offsetY: 0,
    }
  }
  return { ...props.layout, offsetY: 0 }
})

function onPointerDown(event: PointerEvent) {
  if (!canManage.value) return
  if ((event.target as HTMLElement).dataset.resize) return
  startDrag(event, props.record, props.rowId, props.layout)
}

function onClick() {
  if (consumeClickSuppression(props.record.id)) return
  store.selectPlanning(props.record.id)
}

function onDoubleClick() {
  if (consumeClickSuppression(props.record.id)) return
  store.openEdit(props.record.id)
}

function onContextMenu(event: MouseEvent) {
  event.preventDefault()
  store.openContextMenu({ x: event.clientX, y: event.clientY, recordId: props.record.id })
}
</script>

<template>
  <div
    data-timeline-block
    class="absolute rounded-md shadow-sm cursor-grab active:cursor-grabbing overflow-visible hover:shadow-md group"
    :class="[
      selected ? 'ring-2 ring-secondary ring-offset-1 z-30' : '',
      isDragging || isResizing ? 'opacity-90 shadow-lg z-50 cursor-grabbing' : 'transition-shadow',
      record.hasOverlap ? 'border-l-2 border-red-400' : '',
      availabilityWarning.hasConflict ? 'border-r-2 border-amber-400' : '',
    ]"
    :style="{
      left: `${displayLayout.leftPx}px`,
      width: `${displayLayout.widthPx}px`,
      top: `${displayLayout.topPx}px`,
      height: `${displayLayout.heightPx}px`,
      transform: displayLayout.offsetY ? `translateY(${displayLayout.offsetY}px)` : undefined,
    }"
    @pointerdown="onPointerDown"
    @click.stop="onClick"
    @dblclick.stop="onDoubleClick"
    @contextmenu="onContextMenu"
    :title="availabilityWarning.hasConflict ? availabilityWarning.message : undefined"
  >
    <div
      class="absolute inset-0 rounded-md overflow-hidden"
      :style="{ backgroundColor: blockColor }"
    >
      <div
        v-if="availabilityWarning.hasConflict"
        class="absolute top-0.5 right-0.5 z-20 text-amber-200 pointer-events-none"
        title="Niet beschikbaar"
      >
        <UIcon name="i-lucide-triangle-alert" class="size-3.5" />
      </div>
      <div
        v-if="selected"
        class="absolute inset-0 bg-white/25 pointer-events-none rounded-md"
      />

      <div
        v-if="canManage"
        data-resize="start"
        class="absolute left-0 top-0 bottom-0 w-1.5 cursor-ew-resize opacity-0 group-hover:opacity-100 bg-white/40 z-10"
        @pointerdown.stop="startResize($event, record, 'start', layout)"
      />

      <div
        v-if="canManage"
        data-resize="end"
        class="absolute right-0 top-0 bottom-0 w-1.5 cursor-ew-resize opacity-0 group-hover:opacity-100 bg-white/40 z-10"
        @pointerdown.stop="startResize($event, record, 'end', layout)"
      />
    </div>

    <div
      class="sticky z-25 w-max max-w-48 px-2 py-0.5 text-white h-full flex flex-col justify-center gap-px pointer-events-none rounded-l-md"
      :style="{ left: `${rowLabelWidth}px`, top: `${headerHeight}px`, backgroundColor: blockColor }"
    >
      <p class="text-xs font-semibold truncate leading-tight shrink-0">
        {{ record.title }}
      </p>
      <!-- <p
        v-if="displayLayout.widthPx > 80 && displayLayout.heightPx >= 28"
        class="text-[10px] leading-tight opacity-90 truncate shrink-0"
      >
        {{ record.customerName || record.assignedUserName }}
      </p> -->
      <p
        v-if="displayLayout.widthPx > 120 && displayLayout.heightPx >= 38"
        class="text-[10px] leading-tight opacity-75 truncate shrink-0"
      >
        {{ formatTimeRange(record.startUtc, record.endUtc) }}
      </p>
    </div>
  </div>
</template>
