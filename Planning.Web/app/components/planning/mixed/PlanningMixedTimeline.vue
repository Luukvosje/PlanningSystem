<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning'
import type { UnavailablePeriod } from '~/types/availability'
import MixedTimelineBlock from './MixedTimelineBlock.vue'
import MixedTimelineHeader from './MixedTimelineHeader.vue'
import AvailabilityOverlay from '../timeline/AvailabilityOverlay.vue'
import { MIXED_END_HOUR, MIXED_HOUR_WIDTH, MIXED_START_HOUR } from '~/utils/planning/mixedTimelineMath'
import { getUnavailableOverlaysForMixed } from '~/utils/planning/availabilityMath'
import { timeToMixedPx } from '~/utils/planning/mixedTimelineMath'

const props = defineProps<{
  records: PlanningRecord[]
  isLoading?: boolean
  availabilityPeriods?: UnavailablePeriod[]
}>()

const store = usePlanningStore()

const {
  timelineWidth,
  weekDays,
  dayRecords,
  contentHeight,
  currentTimePx,
  getBlockLayout,
  isSelectedDay,
  selectDay,
} = useMixedTimeline(toRef(() => props.records))

const dayTabFormatter = new Intl.DateTimeFormat('nl-NL', { weekday: 'short', day: 'numeric' })

const mixedAvailabilityOverlays = computed(() => {
  if (!props.availabilityPeriods?.length) return []

  const userIds = [...new Set(dayRecords.value.map(record => record.assignedUserId).filter(Boolean))]
  return userIds.flatMap(userId =>
    getUnavailableOverlaysForMixed(
      [],
      userId,
      store.mixedSelectedDay,
      timeToMixedPx,
      props.availabilityPeriods,
    ),
  )
})
</script>

<template>
  <div class="flex flex-col gap-3 h-full min-h-0">
    <div class="flex flex-wrap gap-1.5 shrink-0">
      <UButton
        v-for="day in weekDays"
        :key="day.toISOString()"
        size="sm"
        :variant="isSelectedDay(day) ? 'solid' : 'outline'"
        :label="dayTabFormatter.format(day)"
        :class="day.getDay() === 0 || day.getDay() === 6 ? 'opacity-80' : ''"
        @click="selectDay(day)"
      />
    </div>

    <div class="relative flex-1 min-h-0 overflow-auto rounded-xl border border-default bg-default shadow-sm">
      <PlanningLoading v-if="isLoading" label="Diensten laden..." />

      <div v-else-if="dayRecords.length === 0" class="p-12 text-center">
        <UIcon name="i-lucide-calendar-off" class="size-10 text-muted mx-auto mb-3" />
        <p class="text-muted">
          Geen diensten op deze dag.
        </p>
      </div>

      <div v-else class="min-w-max">
        <MixedTimelineHeader />

        <div
          class="relative"
          data-timeline-row
          data-row-id="__mixed__"
          :style="{ minWidth: `${timelineWidth}px`, height: `${contentHeight}px` }"
        >
          <!-- Hour grid lines -->
          <div
            v-for="hour in (MIXED_END_HOUR - MIXED_START_HOUR)"
            :key="hour"
            class="absolute top-0 bottom-0 border-r border-default/30 pointer-events-none"
            :style="{ left: `${hour * MIXED_HOUR_WIDTH}px`, width: `${MIXED_HOUR_WIDTH}px` }"
          />

          <!-- Current time indicator -->
          <div
            v-if="currentTimePx !== null"
            class="absolute top-0 bottom-0 w-0.5 bg-red-500 z-10 pointer-events-none"
            :style="{ left: `${currentTimePx}px` }"
          >
            <div class="absolute -top-1 -left-1.5 size-3 rounded-full bg-red-500 animate-pulse" />
          </div>

          <AvailabilityOverlay :overlays="mixedAvailabilityOverlays" />

          <MixedTimelineBlock
            v-for="record in dayRecords"
            :key="record.id"
            :record="record"
            :layout="getBlockLayout(record)!"
            :selected="store.selectedPlanningId === record.id"
            :availability-periods="availabilityPeriods"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<!-- TODO: week view with multiple day columns side-by-side can extend this layout -->
