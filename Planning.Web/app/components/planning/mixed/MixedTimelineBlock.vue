<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning'
import type { UnavailablePeriod } from '~/types/availability'
import { CONCEPT_BLOCK_STYLE, formatTimeRange, getBlockColor } from '~/utils/planning/dateUtils'
import { isOpenShift } from '~/utils/planning/mixedTimelineMath'

const props = defineProps<{
  record: PlanningRecord
  layout: { leftPx: number, widthPx: number, topPx: number, heightPx: number }
  selected?: boolean
  availabilityPeriods?: UnavailablePeriod[]
}>()

const store = usePlanningStore()
const { data: users } = useUsers()
const { checkRecord } = useAvailabilityWarning(users, {
  planningPeriods: computed(() => props.availabilityPeriods ?? []),
})

const isOpen = computed(() => isOpenShift(props.record))
const isConcept = computed(() => props.record.status === 'Planned')
const blockColor = computed(() => {
  if (isOpen.value) return undefined
  if (isConcept.value) return CONCEPT_BLOCK_STYLE.backgroundColor
  return getBlockColor(props.record.color, props.record.status)
})

const tooltipText = computed(() => {
  const parts = [props.record.title]
  if (props.record.assignedUserName && !isOpen.value) {
    parts.push(props.record.assignedUserName)
  }
  parts.push(formatTimeRange(props.record.startUtc, props.record.endUtc))
  if (props.record.notes) parts.push(props.record.notes)
  const warning = checkRecord(props.record)
  if (warning.hasConflict && warning.message) parts.push(warning.message)
  return parts.join(' · ')
})

const availabilityWarning = computed(() => checkRecord(props.record))

function onClick() {
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
    class="absolute rounded-md shadow-sm cursor-pointer overflow-hidden transition-shadow hover:shadow-md"
    :class="[
      selected ? 'ring-2 ring-secondary ring-offset-1 z-30' : '',
      record.hasOverlap ? 'border-l-2 border-red-400' : '',
      availabilityWarning.hasConflict ? 'border-r-2 border-amber-400' : '',
      isOpen
        ? 'border border-dashed border-muted-foreground/50 bg-[repeating-linear-gradient(-45deg,transparent,transparent_4px,rgba(148,163,184,0.25)_4px,rgba(148,163,184,0.25)_8px)] bg-muted/60'
        : '',
    ]"
    :style="{
      left: `${layout.leftPx}px`,
      width: `${layout.widthPx}px`,
      top: `${layout.topPx}px`,
      height: `${layout.heightPx}px`,
      ...(isOpen
        ? {}
        : isConcept
          ? { ...CONCEPT_BLOCK_STYLE }
          : { backgroundColor: blockColor }),
    }"
    :title="tooltipText"
    @click.stop="onClick"
    @contextmenu="onContextMenu"
  >
    <div
      v-if="selected"
      class="absolute inset-0 bg-white/25 pointer-events-none rounded-md"
    />

    <div
      v-if="availabilityWarning.hasConflict"
      class="absolute top-0.5 right-0.5 z-20 text-amber-500 pointer-events-none"
    >
      <UIcon name="i-lucide-triangle-alert" class="size-3.5" />
    </div>

    <div
      class="relative px-2 py-0.5 min-w-0 h-full flex flex-col justify-center gap-px pointer-events-none"
      :class="isOpen ? 'text-foreground' : 'text-white'"
    >
      <p class="text-xs font-semibold truncate leading-tight shrink-0">
        {{ isOpen ? 'Open dienst' : record.assignedUserName }}
      </p>
      <p
        v-if="layout.widthPx > 60"
        class="text-[10px] leading-tight opacity-90 truncate shrink-0"
      >
        {{ record.title }}
      </p>
      <p
        v-if="layout.widthPx > 90 && layout.heightPx >= 38"
        class="text-[10px] leading-tight opacity-75 truncate shrink-0"
      >
        {{ formatTimeRange(record.startUtc, record.endUtc) }}
      </p>
    </div>
  </div>
</template>

<!-- TODO: drag-to-reschedule support can reuse useDragPlanning once mixed lane targets exist -->
