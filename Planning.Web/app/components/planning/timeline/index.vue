<script setup lang="ts">
import type { TimelineRow } from '~/types/planning'
import type { AvailabilityEntry } from '~/types/availability'
import { useResizeObserver } from '@vueuse/core'

const props = defineProps<{
  rows: TimelineRow[]
  isLoading?: boolean
  availabilityEntries?: AvailabilityEntry[]
}>()

const store = usePlanningStore()

const containerRef = useTemplateRef<HTMLElement>('containerRef')
const containerWidth = ref(0)

useResizeObserver(containerRef, (entries: readonly ResizeObserverEntry[]) => {
  containerWidth.value = entries[0]?.contentRect.width ?? 0
})

provide('timelineContainerWidth', containerWidth)

useTimelineWheelZoom(containerRef)
</script>

<template>
  <div
    ref="containerRef"
    class="relative overflow-auto rounded-xl border border-default bg-default shadow-sm h-full"
  >
    <div v-if="isLoading" class="p-8 space-y-3">
      <USkeleton class="h-12 w-full" />
      <USkeleton class="h-16 w-full" />
      <USkeleton class="h-16 w-full" />
    </div>

    <div v-else-if="rows.length === 0" class="p-12 text-center">
      <UIcon name="i-lucide-calendar-off" class="size-10 text-muted mx-auto mb-3" />
      <p class="text-muted">
        Geen resources om weer te geven.
      </p>
    </div>

    
    <div v-else class="w-max min-w-full flex flex-col">
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
          :availability-entries="availabilityEntries"
          />
        </div>
      </div>
    </div>
</template>
