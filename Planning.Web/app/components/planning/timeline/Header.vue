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
  flatImportantGridLines,
  showTimeSlots,
  isZoomedOut,
} = useTimeline();

const { t } = useI18n();
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
		class="sticky top-0 z-100 glass border-b border-default"
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
					<!--
						Sticky needs room to travel, so the label is `w-max` rather than filling the
						cell: it rides against the resource column while its period scrolls past, then
						the next period's label pushes it out.
					-->
					<div
						class="sticky w-max max-w-full"
						:style="{ left: `${rowLabelWidth + 8}px` }"
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
		</div>

		<div
			class="flex"
			:class="showTimeSlots ? 'border-b border-default' : ''"
		>
			<div
				class="sticky left-0 z-30 flex shrink-0 items-center border-r border-default bg-default px-3 py-2"
				:style="{ width: `${rowLabelWidth}px` }"
			>
				<span class="text-xs font-medium text-muted uppercase tracking-wide">{{ store.rowMode === 'resource' ? t('nav.team') : t('planning.fields.customer') }}</span>
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
					<div
						class="sticky w-max max-w-full"
						:style="{ left: `${rowLabelWidth + 8}px` }"
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
		</div>

		<div
			v-if="showTimeSlots"
			class="flex"
		>
			<div
				class="sticky left-0 z-30 shrink-0 border-r border-default bg-default "
				:style="{ width: `${rowLabelWidth}px` }"
			/>

			<div class="relative flex">
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
					<div
						v-if="!isZoomedOut"
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
						:title="line.label"
					/>
				</div>

				<!--
					Flat overlay for important-time labels at zoomed-out levels (e.g. "dagdeel"): rendered
					once across the full ruler width instead of nested per day column, so a label near a
					day boundary can never be visually clipped by the next day column painting over it.
					Colliding labels (too close together) are hidden here too; their vertical line above
					still shows the exact time via the native title-tooltip on hover.
				-->
				<div
					v-if="isZoomedOut"
					class="absolute inset-0 pointer-events-none"
				>
					<span
						v-for="(line, index) in flatImportantGridLines"
						v-show="line.showLabel && line.label"
						:key="`flat-important-${index}-${line.leftPx}`"
						class="absolute top-1.5 rounded-full bg-default/90 ring ring-default px-1.5 py-0.5 text-[10px] font-semibold text-default leading-none whitespace-nowrap"
						:style="{ left: `${line.leftPx + 4}px` }"
						:title="line.label"
					>
						{{ line.label }}
					</span>
				</div>
			</div>
		</div>
	</div>

	<PlanningContextMenu />
</template>
