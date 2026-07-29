<script setup lang="ts">
import type { DateValue } from '@internationalized/date';
import { fromDate, getLocalTimeZone, toCalendarDate } from '@internationalized/date';
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import { formatCompactDayHeader, getMonday } from '~/utils/planning/dateUtils';

const props = defineProps<{
  weekLabel: string
  weekRangeLabel: string
  isCurrentWeek: boolean
  days: MyPlanningDay[]
  weekStart: Date
  selectedDateKey: string
}>();

const emit = defineEmits<{
  previous: []
  next: []
  today: []
  selectWeek: [date: Date]
  selectDay: [dateKey: string]
}>();

const calendarOpen = ref(false);

const calendarDefaultDate = computed(() =>
  toCalendarDate(fromDate(props.weekStart, getLocalTimeZone())),
);

function onCalendarDateSelect(
  value: DateValue | { start?: DateValue, end?: DateValue } | DateValue[] | null | undefined,
) {
  if (!value || Array.isArray(value) || !('day' in value)) {
    return;
  }
  emit('selectWeek', getMonday(value.toDate(getLocalTimeZone())));
  calendarOpen.value = false;
}

function goToTodayAndClose() {
  emit('today');
  calendarOpen.value = false;
}

function dayButtonClass(day: MyPlanningDay) {
  const selected = props.selectedDateKey === day.dateKey;
  const textClass = day.hasShifts ? 'text-default' : 'text-muted';

  if (selected) {
    return 'bg-secondary text-inverted shadow-sm';
  }

  if (day.isToday) {
    return `bg-secondary/15 ring-1 ring-secondary/40 ${textClass}`;
  }

  return `bg-muted/30 hover:bg-muted/50 ${textClass}`;
}
</script>

<template>
	<div class="space-y-3">
		<div class="flex flex-wrap items-center gap-2">
			<UFieldGroup class="min-w-0 flex-1 sm:flex-none">
				<UButton
					variant="outline"
					icon="i-lucide-chevron-left"
					aria-label="Vorige week"
					size="md"
					@click="emit('previous')"
				/>

				<UPopover v-model:open="calendarOpen">
					<UButton
						variant="outline"
						size="md"
						class="min-w-0 flex-1 justify-center sm:min-w-40 sm:flex-none"
					>
						<span class="truncate font-semibold">{{ weekLabel }}</span>
					</UButton>

					<template #content>
						<div class="flex flex-col gap-2 p-2">
							<UCalendar
								v-if="calendarOpen"
								:default-value="calendarDefaultDate"
								color="secondary"
								@update:model-value="onCalendarDateSelect"
							/>
							<UButton
								variant="outline"
								label="Deze week"
								block
								@click="goToTodayAndClose"
							/>
						</div>
					</template>
				</UPopover>

				<UButton
					variant="outline"
					icon="i-lucide-chevron-right"
					aria-label="Volgende week"
					size="md"
					@click="emit('next')"
				/>
			</UFieldGroup>

			<UButton
				v-if="!isCurrentWeek"
				variant="soft"
				color="secondary"
				size="md"
				label="Deze week"
				class="shrink-0"
				@click="emit('today')"
			/>
		</div>

		<p class="text-sm text-muted">
			{{ weekRangeLabel }}
		</p>

		<div class="grid grid-cols-7 gap-1.5 sm:gap-2">
			<button
				v-for="day in days"
				:key="day.dateKey"
				type="button"
				class="flex min-h-16 flex-col items-center justify-center gap-1 rounded-xl px-1 py-2.5 text-center transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-secondary sm:min-h-20"
				:class="dayButtonClass(day)"
				:aria-pressed="selectedDateKey === day.dateKey"
				:aria-label="`${formatCompactDayHeader(day.date).weekday} ${formatCompactDayHeader(day.date).day}`"
				@click="emit('selectDay', day.dateKey)"
			>
				<span class="text-[10px] font-semibold uppercase tracking-wide sm:text-xs">
					{{ formatCompactDayHeader(day.date).weekday }}
				</span>
				<span class="text-base font-semibold tabular-nums sm:text-lg">
					{{ formatCompactDayHeader(day.date).day }}
				</span>
			</button>
		</div>
	</div>
</template>
