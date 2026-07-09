<script setup lang="ts">
import { useResizeObserver } from '@vueuse/core'

const { dayHeaders, rowLabelWidth, timeSlotMarkers, showTimeSlots } = useTimeline()

const headerRef = useTemplateRef<HTMLElement>('headerRef')
const headerHeight = ref(0)

useResizeObserver(headerRef, (entries: readonly ResizeObserverEntry[]) => {
  headerHeight.value = entries[0]?.contentRect.height ?? 0
})

provide('timelineHeaderHeight', headerHeight)
</script>

<template>
  <div
    ref="headerRef"
    class="sticky top-0 z-20 bg-default/95 backdrop-blur border-b border-default"
  >
    <div class="flex border-b border-default/50">
      <div
        class="sticky left-0 z-30 shrink-0 border-r border-default bg-default px-3 py-2"
        :style="{ width: `${rowLabelWidth}px` }"
      >
        <span class="text-xs font-medium text-muted uppercase tracking-wide">Resource</span>
      </div>

      <div class="flex">
        <div
          v-for="day in dayHeaders"
          :key="day.label"
          class="shrink-0 border-r-2 border-default/70 relative"
          :class="day.isWeekend ? 'bg-muted/40' : ''"
          :style="{ width: `${day.width}px` }"
        >
          <PlanningTimelineStickyDayLabel
            :label="day.label"
            :is-weekend="day.isWeekend"
            variant="header"
          />
        </div>
      </div>
    </div>

    <div v-if="showTimeSlots" class="flex">
      <div
        class="sticky left-0 z-30 shrink-0 border-r border-default bg-default"
        :style="{ width: `${rowLabelWidth}px` }"
      />

      <div class="flex">
        <div
          v-for="day in dayHeaders"
          :key="`time-${day.label}`"
          class="shrink-0 flex divide-x divide-default/25 border-r-2 border-default/70"
          :class="day.isWeekend ? 'bg-muted/40' : ''"
          :style="{ width: `${day.width}px` }"
        >
          <div
            v-for="slot in timeSlotMarkers"
            :key="`${day.label}-${slot.leftPx}`"
            class="shrink-0 flex items-center justify-center py-2"
            :style="{ width: `${slot.width}px` }"
          >
            <span
              v-if="slot.showLabel"
              class="text-[10px] font-medium text-muted leading-none"
            >
              {{ slot.label }}
            </span>
            <UTooltip v-else :text="slot.label">
              <div class="size-full min-h-4" :aria-label="slot.label" />
            </UTooltip>
          </div>
        </div>
      </div>
    </div>
  </div>

  <PlanningContextMenu />
</template>
