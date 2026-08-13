<script setup lang="ts">
type MyPlanningViewMode = 'day' | 'week';

definePageMeta({ layout: false });

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

const { t } = useI18n();
const viewMode = ref<MyPlanningViewMode>('week');

const viewModes = computed<{ label: string, value: MyPlanningViewMode, icon: string }[]>(() => [
  { label: t('planning.viewMode.day'), value: 'day', icon: 'i-lucide-calendar' },
  { label: t('planning.viewMode.week'), value: 'week', icon: 'i-lucide-calendar-range' },
]);

const periodLabel = computed(() => `${weekLabel.value} · ${weekRangeLabel.value}`);

function setViewMode(mode: MyPlanningViewMode) {
  viewMode.value = mode;
}
</script>

<template>
	<NuxtLayout name="default">
		<template #actions>
			<div class="flex min-w-0 flex-wrap items-center justify-end gap-2 max-lg:gap-1.5">
				<UFieldGroup>
					<UButton
						v-for="mode in viewModes"
						:key="mode.value"
						:icon="mode.icon"
						:label="mode.label"
						:variant="viewMode === mode.value ? 'solid' : 'outline'"
						:color="viewMode === mode.value ? 'secondary' : 'neutral'"
						size="sm"
						@click="setViewMode(mode.value)"
					/>
				</UFieldGroup>

				<PlanningMyPeriodNav
					:label="periodLabel"
					:week-start="weekStart"
					:is-current-week="isCurrentWeek"
					size="sm"
					@previous="navigatePrevious"
					@next="navigateNext"
					@today="goToToday"
					@select-date="selectWeekContaining"
				/>
			</div>
		</template>

		<div class="flex h-full min-h-0 w-full max-w-none flex-col overflow-hidden">
			<PlanningMyView
				:view-mode="viewMode"
				:days="days"
				:selected-date-key="selectedDateKey"
				:selected-day="selectedDay"
				:is-loading="isLoading"
				@select-day="selectDay"
			/>
		</div>
	</NuxtLayout>
</template>
