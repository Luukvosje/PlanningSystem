<script setup lang="ts">
import { useResizeObserver } from '@vueuse/core';

const {
  dayHeaders,
  periodHeaders,
  columnHeaders,
  showPeriodHeaders,
  columnMode,
  rowLabelWidth,
  timelineGridLines,
  importantGridLines,
  coarseGridLines,
  showTimeSlots,
  isZoomedOut,
} = useTimeline();

const store = usePlanningStore();

const headerRef = useTemplateRef<HTMLElement>('headerRef');
const headerHeight = ref(0);

useResizeObserver(headerRef, (entries: readonly ResizeObserverEntry[]) => {
  headerHeight.value = entries[0]?.contentRect.height ?? 0;
});

provide('timelineHeaderHeight', headerHeight);

function periodBorderClass(isPrimaryBorderEnd: boolean) {
  return isPrimaryBorderEnd ? 'border-r-2 border-default' : 'border-r border-default/40';
}

function dayBorderClass(isPrimaryBorderEnd: boolean) {
  return isPrimaryBorderEnd ? 'border-r-2 border-default/70' : 'border-r border-default/20';
}
</script>

<template>
	<div
		ref="headerRef"
		class="sticky top-0 z-100 bg-default/95 backdrop-blur border-b border-default"
	>
		<div
			v-if="showPeriodHeaders"
			class="flex border-b border-default"
		>
			<div
				class="sticky left-0 z-30 shrink-0 border-r border-default bg-default px-3 py-2"
				:style="{ width: `${rowLabelWidth}px` }"
			/>

			<div class="flex">
				<div
					v-for="period in periodHeaders"
					:key="period.key"
					class="shrink-0 px-2 py-1.5 bg-muted/30"
					:class="periodBorderClass(period.isPrimaryBorderEnd)"
					:style="{ width: `${period.width}px` }"
				>
					<p class="text-xs font-semibold text-default leading-tight truncate">
						{{ period.label }}
					</p>
					<p
						v-if="period.dateRange"
						class="text-[10px] text-muted leading-tight truncate"
					>
						{{ period.dateRange }}
					</p>
				</div>
			</div>
		</div>

		<div class="flex">
			<div
				class="sticky left-0 z-30 flex shrink-0 items-center border-r border-default bg-default px-3 py-2"
				:style="{ width: `${rowLabelWidth}px` }"
			>
				<span class="text-xs font-medium text-muted uppercase tracking-wide">{{ store.rowMode === 'resource' ? 'Team' : 'Klant' }}</span>
			</div>

			<div
				v-if="columnMode === 'day'"
				class="flex"
			>
				<div
					v-for="day in dayHeaders"
					:key="day.label"
					class="relative flex shrink-0 items-stretch"
					:class="[
						dayBorderClass(day.isPrimaryBorderEnd),
						day.isWeekend && store.showWeekends ? 'bg-muted/40' : '',
					]"
					:style="{ width: `${day.width}px` }"
				>
					<PlanningTimelineStickyDayLabel
						:weekday="day.compact.weekday"
						:day="day.compact.day"
						:is-weekend="day.isWeekend && store.showWeekends"
						variant="header"
					/>
				</div>
			</div>

			<div
				v-else
				class="flex"
			>
				<div
					v-for="column in columnHeaders"
					:key="column.key"
					class="shrink-0 px-2 py-1.5 flex flex-col justify-center"
					:class="periodBorderClass(column.isPrimaryBorderEnd)"
					:style="{ width: `${column.width}px` }"
				>
					<p class="text-xs font-semibold text-default leading-tight truncate">
						{{ column.label }}
					</p>
					<p
						v-if="column.dateRange"
						class="text-[10px] text-muted leading-tight truncate"
					>
						{{ column.dateRange }}
					</p>
				</div>
			</div>
		</div>

		<div
			v-if="showTimeSlots"
			class="flex"
		>
			<div
				class="sticky left-0 z-30 shrink-0 border-r border-default bg-default"
				:style="{ width: `${rowLabelWidth}px` }"
			/>

			<div class="flex">
				<div
					v-for="day in dayHeaders"
					:key="`time-${day.label}`"
					class="shrink-0 relative"
					:class="[
						dayBorderClass(day.isPrimaryBorderEnd),
						day.isWeekend && store.showWeekends ? 'bg-muted/40' : '',
					]"
					:style="{ width: `${day.width}px`, minHeight: isZoomedOut ? '2rem' : undefined }"
				>
					<template v-if="isZoomedOut">
						<div
							v-for="line in (day.importantGridLines ?? importantGridLines)"
							:key="`${day.label}-important-${line.leftPx}`"
							class="absolute top-0 bottom-0 border-l-2 border-default/70 pointer-events-none"
							:style="{ left: `${line.leftPx}px` }"
						>
							<span
								v-if="line.label"
								class="absolute top-2 left-1 text-[10px] font-semibold text-default leading-none whitespace-nowrap"
							>
								{{ line.label }}
							</span>
						</div>
					</template>

					<div
						v-else
						class="flex h-full"
					>
						<div
							v-for="line in (day.gridLines ?? timelineGridLines)"
							:key="`${day.label}-${line.leftPx}`"
							class="shrink-0 flex items-center justify-center py-2 border-l border-default/20"
							:style="{ width: `${line.width}px` }"
						>
							<span
								v-if="line.showLabel && line.label"
								class="text-[10px] font-medium text-muted leading-none"
							>
								{{ line.label }}
							</span>
							<UTooltip
								v-else-if="line.label"
								:text="line.label"
							>
								<div
									class="size-full min-h-4"
									:aria-label="line.label"
								/>
							</UTooltip>
						</div>
					</div>

					<div
						v-for="line in (day.importantGridLines ?? importantGridLines)"
						:key="`${day.label}-important-${line.leftPx}`"
						class="absolute top-0 bottom-0 border-l-2 border-default/70 pointer-events-none"
						:style="{ left: `${line.leftPx}px` }"
					/>

					<!-- Coarse grid lines for day / week / month zoom -->
					<template v-if="!showTimeSlots && coarseGridLines.length > 0">
						<div
							v-for="line in coarseGridLines"
							:key="`${day.label}-coarse-${line.leftPx}`"
							class="absolute top-0 bottom-0 border-l border-default/20 pointer-events-none"
							:style="{ left: `${line.leftPx}px` }"
						/>
					</template>
				</div>
			</div>
		</div>
	</div>

	<PlanningContextMenu />
</template>
