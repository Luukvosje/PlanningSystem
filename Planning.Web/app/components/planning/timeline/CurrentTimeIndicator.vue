<script setup lang="ts">
const { currentTimePx, timelineWidth, rowLabelWidth, dayHeaders, timeSlotMarkers, showTimeSlots } = useTimeline()
</script>

<template>
  <div
    v-if="currentTimePx !== null"
    class="absolute top-0 bottom-0 w-0.5 bg-red-500 z-10 pointer-events-none"
    :style="{ left: `${rowLabelWidth + currentTimePx}px` }"
  >
    <div class="absolute -top-1 -left-1.5 size-3 rounded-full bg-red-500 animate-pulse" />
  </div>

  <div
    class="absolute inset-0 pointer-events-none flex"
    :style="{ marginLeft: `${rowLabelWidth}px`, width: `${timelineWidth}px` }"
  >
    <div
      v-for="day in dayHeaders"
      :key="`bg-${day.label}`"
      class="shrink-0 h-full border-r-2 border-default/70"
      :class="day.isWeekend ? 'bg-muted/20' : 'bg-secondary/5'"
      :style="{ width: `${day.width}px` }"
    >
      <div
        v-if="showTimeSlots"
        class="flex h-full divide-x divide-default/25"
      >
        <div
          v-for="slot in timeSlotMarkers"
          :key="`${day.label}-grid-${slot.leftPx}`"
          class="shrink-0 h-full"
          :style="{ width: `${slot.width}px` }"
        />
      </div>
    </div>
  </div>
</template>
