<script setup lang="ts">
type MyPlanningViewMode = 'day' | 'week';

const {
  weekLabel,
  weekRangeLabel,
  isCurrentWeek,
  days,
  weekStart,
  selectedDateKey,
  selectedDay,
  isLoading,
  navigatePrevious,
  navigateNext,
  goToToday,
  selectWeekContaining,
  selectDay,
} = useMyPlanningView();

const viewMode = ref<MyPlanningViewMode>('day');

const viewModes: { label: string, value: MyPlanningViewMode, icon: string }[] = [
  { label: 'Dag', value: 'day', icon: 'i-lucide-calendar' },
  { label: 'Week', value: 'week', icon: 'i-lucide-calendar-range' },
];

const workDayCount = computed(() => days.value.filter((day) => day.hasShifts).length);

const subtitle = computed(() =>
  workDayCount.value > 0 ?
    `${workDayCount.value} ${workDayCount.value === 1 ? 'werkdag' : 'werkdagen'} deze week` :
    'Geen diensten deze week',
);

function setViewMode(mode: MyPlanningViewMode) {
  viewMode.value = mode;
}
</script>

<template>
	<div class="flex h-full min-h-0 w-full max-w-none flex-col">
		<div class="flex shrink-0 flex-col gap-4 border-b border-default p-4 max-lg:gap-3 max-lg:p-3">
			<div class="flex flex-wrap items-center justify-between gap-3">
				<div class="flex min-w-0 flex-wrap items-center gap-3">
					<UFieldGroup>
						<UButton
							v-for="mode in viewModes"
							:key="mode.value"
							:icon="mode.icon"
							:label="mode.label"
							:variant="viewMode === mode.value ? 'solid' : 'outline'"
							:color="viewMode === mode.value ? 'secondary' : 'neutral'"
							@click="setViewMode(mode.value)"
						/>
					</UFieldGroup>

					<p class="text-sm text-muted">
						{{ subtitle }}
					</p>
				</div>
			</div>

			<PlanningMyWeekNav
				:week-label="weekLabel"
				:week-range-label="weekRangeLabel"
				:is-current-week="isCurrentWeek"
				:days="days"
				:week-start="weekStart"
				:selected-date-key="selectedDateKey"
				:show-days="viewMode === 'day'"
				@previous="navigatePrevious"
				@next="navigateNext"
				@today="goToToday"
				@select-week="selectWeekContaining"
				@select-day="selectDay"
			/>
		</div>

		<PlanningMyDayDetail
			v-if="viewMode === 'day'"
			class="min-h-0 w-full flex-1 grow"
			:day="selectedDay"
			:loading="isLoading"
			flush
		/>

		<div
			v-else
			class="grid min-h-0 w-full flex-1 grow grid-cols-1 overflow-auto md:grid-cols-2 xl:grid-cols-7 [&>*]:border-b [&>*]:border-default md:[&>*]:border-e"
		>
			<template v-if="isLoading">
				<PlanningMyDayDetail
					v-for="index in 7"
					:key="index"
					class="min-h-52 md:min-h-0 md:h-full"
					loading
					compact
					flush
				/>
			</template>

			<template v-else>
				<PlanningMyDayDetail
					v-for="day in days"
					:key="day.dateKey"
					class="min-h-52 md:min-h-0 md:h-full"
					:day="day"
					compact
					flush
				/>
			</template>
		</div>
	</div>
</template>
