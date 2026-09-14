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
  coarseGridLines,
  showTimeSlots,
  isZoomedOut,
} = useTimeline();
const { openingHours } = usePlanningSettings();
const store = usePlanningStore();

function openingOverlaysForDay(day: TimelineDayHeader) {
  // Blocks are positioned with compact math whenever the day has a window, so the shading has
  // to follow the same rule. Gating this on the row layout instead made the grey bands miss the
  // blocks in the default setup (spacious rows + showFullDay false).
  if (day.window) {
    return getOutsideOpeningOverlaysCompact(day.date, day.width, openingHours.value, day.window);
  }

  return getOutsideOpeningOverlays(day.date, day.width, openingHours.value);
}

/**
 * The grid reads as one field of lines when every line has the same weight, so each kind gets
 * its own: week or month boundary heaviest, then the day, then the important work times, then
 * the hour, and the sub-hour slot lines barely there. A day boundary used to be drawn at the
 * same strength as a 15-minute slot.
 */
function dayBorderClass(day: TimelineDayHeader) {
  return day.isPrimaryBorderEnd ?
    'border-r-2 border-default/70' :
    'border-r border-default/40';
}
</script>

<template>
	<div
		v-if="currentTimeIndicator"
		class="absolute top-0 bottom-0 w-0.5 bg-error z-10 pointer-events-none"
		:style="{ left: `${rowLabelWidth + currentTimeIndicator.px}px` }"
	>
		<div class="absolute -top-1 -left-1.5 size-3 rounded-full bg-error animate-pulse" />
		<div
			v-if="currentTimeIndicator.clamped === 'before'"
			class="absolute top-3 -left-2 text-[10px] font-semibold text-error leading-none"
			aria-hidden="true"
		>
			◀
		</div>
		<div
			v-else-if="currentTimeIndicator.clamped === 'after'"
			class="absolute top-3 -left-2 text-[10px] font-semibold text-error leading-none"
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
				day.isWeekend && store.showWeekends ? 'bg-muted/30' : '',
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
					class="absolute inset-y-0 border-l border-default/50"
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
					class="shrink-0 h-full border-l"
					:class="line.isHourStart ? 'border-default/25' : 'border-default/8'"
					:style="{ width: `${line.width}px` }"
				/>
			</div>

			<div
				v-for="line in (day.importantGridLines ?? importantGridLines)"
				v-show="showTimeSlots && !isZoomedOut"
				:key="`${day.label}-accent-${line.leftPx}`"
				class="absolute inset-y-0 border-l border-default/50 pointer-events-none"
				:style="{ left: `${line.leftPx}px` }"
			/>

			<!-- Coarse grid lines for day / week / month zoom (always visible) -->
			<template v-if="!showTimeSlots && coarseGridLines.length > 0">
				<div
					v-for="line in coarseGridLines"
					:key="`${day.label}-coarse-${line.leftPx}`"
					class="absolute inset-y-0 border-l border-default/20 pointer-events-none"
					:style="{ left: `${line.leftPx}px` }"
				/>
			</template>
		</div>
	</div>
</template>
