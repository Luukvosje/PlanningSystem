<script setup lang="ts">
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

const workDayCount = computed(() => days.value.filter((day) => day.hasShifts).length);
</script>

<template>
	<LayoutPageContainer>
		<div class="mx-auto w-full max-w-xl space-y-6">
			<LayoutPageHeader
				title="Mijn planning"
				:subtitle="workDayCount > 0
					? `${workDayCount} ${workDayCount === 1 ? 'werkdag' : 'werkdagen'} deze week`
					: 'Geen diensten deze week'"
			/>

			<PlanningMyWeekNav
				:week-label="weekLabel"
				:week-range-label="weekRangeLabel"
				:is-current-week="isCurrentWeek"
				:days="days"
				:week-start="weekStart"
				:selected-date-key="selectedDateKey"
				@previous="navigatePrevious"
				@next="navigateNext"
				@today="goToToday"
				@select-week="selectWeekContaining"
				@select-day="selectDay"
			/>

			<div
				v-if="isLoading"
				class="space-y-3"
			>
				<UCard :ui="{ body: 'p-5' }">
					<div class="space-y-3">
						<USkeleton class="h-4 w-28" />
						<USkeleton class="h-4 w-40" />
						<USkeleton class="h-4 w-52" />
					</div>
				</UCard>
			</div>

			<PlanningMyDayDetail
				v-else-if="selectedDay"
				:day="selectedDay"
			/>
		</div>
	</LayoutPageContainer>
</template>
