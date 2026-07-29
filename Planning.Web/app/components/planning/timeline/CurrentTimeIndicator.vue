<script setup lang="ts">
import type { TimelineDayHeader } from '~/composables/planning/useTimeline';
import {
  getOutsideOpeningOverlays,
  getOutsideOpeningOverlaysCompact,
} from '~/utils/planning/planningSettings';

const {
  currentTimeIndicator,
  timelineWidth,
  rowLabelWidth,
  dayHeaders,
  timelineGridLines,
  importantGridLines,
  showTimeSlots,
  isZoomedOut,
  isCompactMode,
} = useTimeline();
const { openingHours } = usePlanningSettings();

function openingOverlaysForDay(day: TimelineDayHeader) {
  if (isCompactMode.value && day.window) {
    return getOutsideOpeningOverlaysCompact(day.date, day.width, openingHours.value, day.window);
  }

  return getOutsideOpeningOverlays(day.date, day.width, openingHours.value);
}

function dayBorderClass(day: TimelineDayHeader) {
  return day.isPrimaryBorderEnd ?
    'border-r-2 border-default/70' :
    'border-r border-default/15';
}
</script>

<template>
	<div
		v-if="currentTimeIndicator"
		class="absolute top-0 bottom-0 w-0.5 bg-red-500 z-10 pointer-events-none"
		:style="{ left: `${rowLabelWidth + currentTimeIndicator.px}px` }"
	>
		<div class="absolute -top-1 -left-1.5 size-3 rounded-full bg-red-500 animate-pulse" />
		<div
			v-if="currentTimeIndicator.clamped === 'before'"
			class="absolute top-3 -left-2 text-[10px] font-bold text-red-500 leading-none"
			aria-hidden="true"
		>
			◀
		</div>
		<div
			v-else-if="currentTimeIndicator.clamped === 'after'"
			class="absolute top-3 -left-2 text-[10px] font-bold text-red-500 leading-none"
			aria-hidden="true"
		>
			▶
		</div>
	</div>

	<div
		class="absolute inset-0 pointer-events-none flex"
		:style="{ marginLeft: `${rowLabelWidth}px`, width: `${timelineWidth}px` }"
	>
		<div
			v-for="(day, dayIndex) in dayHeaders"
			:key="`bg-${day.label}`"
			class="shrink-0 h-full relative"
			:class="[
				dayBorderClass(day),
				day.isWeekend ? 'bg-muted/20' : 'bg-secondary/5',
			]"
			:style="{ width: `${day.width}px` }"
		>
			<div
				v-for="(overlay, overlayIndex) in openingOverlaysForDay(day)"
				:key="`closed-${dayIndex}-${overlayIndex}`"
				class="absolute inset-y-0 bg-black/10 dark:bg-black/25"
				:style="{ left: `${overlay.left}px`, width: `${overlay.width}px` }"
			/>

			<template v-if="showTimeSlots && isZoomedOut">
				<div
					v-for="line in (day.importantGridLines ?? importantGridLines)"
					:key="`${day.label}-important-${line.leftPx}`"
					class="absolute inset-y-0 border-l-2 border-default/60"
					:style="{ left: `${line.leftPx}px` }"
				/>
			</template>

			<div
				v-else-if="showTimeSlots"
				class="flex h-full"
			>
				<div
					v-for="line in (day.gridLines ?? timelineGridLines)"
					:key="`${day.label}-grid-${line.leftPx}`"
					class="shrink-0 h-full border-l border-default/15"
					:style="{ width: `${line.width}px` }"
				/>
			</div>

			<div
				v-for="line in (day.importantGridLines ?? importantGridLines)"
				v-show="showTimeSlots && !isZoomedOut"
				:key="`${day.label}-accent-${line.leftPx}`"
				class="absolute inset-y-0 border-l-2 border-default/60 pointer-events-none"
				:style="{ left: `${line.leftPx}px` }"
			/>
		</div>
	</div>
</template>
