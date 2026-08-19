<script setup lang="ts">
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import { formatCompactDayHeader, getIntlLocale } from '~/utils/planning/dateUtils';

const props = defineProps<{
  days: MyPlanningDay[]
  selectedDateKey: string
}>();

const emit = defineEmits<{
  selectDay: [dateKey: string]
}>();

const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));

const dayItems = computed(() =>
  props.days.map((day) => {
    const header = formatCompactDayHeader(day.date, intlLocale.value);
    return {
      day,
      weekday: header.weekday,
      dayNumber: header.day,
      selected: props.selectedDateKey === day.dateKey,
    };
  }),
);

function dayButtonClass(day: MyPlanningDay, selected: boolean) {
  const textClass = day.hasShifts ? 'text-default' : 'text-muted/30';
  const selectedClass = selected ? 'bg-default border-b border-b-transparent' : 'hover:bg-muted/30 border-b border-default';

  return `${textClass} ${selectedClass} ${selected ? 'text-brand' : ''}`;
}
</script>

<template>
	<div
		class="grid grid-cols-7 divide-x divide-default"
		role="tablist"
		:aria-label="t('planning.daysOfWeek')"
	>
		<button
			v-for="{ day, weekday, dayNumber, selected } in dayItems"
			:key="day.dateKey"
			type="button"
			role="tab"
			class="relative bg-accented/40 flex min-h-14 flex-col items-center justify-center gap-0.5 px-1 py-2.5 text-center transition-colors focus-visible:z-10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-brand sm:min-h-16"
			:class="dayButtonClass(day, selected)"
			:aria-selected="selected"
			:aria-label="`${weekday} ${dayNumber}`"
			@click="emit('selectDay', day.dateKey)"
		>
			<span class="text-[10px] font-semibold uppercase tracking-wide sm:text-xs">
				{{ weekday }}
			</span>
			<span class="text-base font-semibold tabular-nums sm:text-lg">
				{{ dayNumber }}
			</span>

			<!-- <span
				class="mt-0.5 size-1 rounded-full"
				:class="day.hasShifts ? 'bg-brand' : 'bg-transparent'"
				aria-hidden="true"
			/> -->

		</button>
	</div>
</template>
