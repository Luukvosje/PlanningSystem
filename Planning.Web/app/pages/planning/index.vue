<script setup lang="ts">
import type { HeaderAction } from '~/types/headerActions';

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

const headerActions = computed<HeaderAction[]>(() => [
  {
    type: 'select',
    key: 'viewMode',
    label: t('planning.settings.view'),
    value: viewMode.value,
    items: viewModes.value,
    desktop: 'buttons',
    onUpdate: (value) => {
      if (value === 'day' || value === 'week') {
        setViewMode(value);
      }
    },
  },
  {
    type: 'slot',
    key: 'period',
  },
]);
</script>

<template>
	<NuxtLayout name="default">
		<template #actions>
			<LayoutHeaderActions :items="headerActions">
				<template #period>
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
				</template>
			</LayoutHeaderActions>
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
