<script setup lang="ts">
import type { AvailabilityRule } from '~/types/availability';
import type { HeaderAction } from '~/types/headerActions';
import { toDateKey } from '~/utils/planning/dateUtils';

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
const auth = useAuthStore();
const viewMode = ref<MyPlanningViewMode>('week');

/**
 * One sheet for the whole page, so the header action and the buttons inside a day open the same
 * thing. The date it starts on is the day you have selected, and the sheet shows that date as an
 * editable field - in week mode there is no other way to tell which day you are about to block.
 */
const exceptionSheetOpen = ref(false);
const editingException = ref<AvailabilityRule | null>(null);
const exceptionDate = ref('');
const employeeId = computed(() => auth.currentUser?.userId ?? '');

function openCreateException(dateKey: string) {
  editingException.value = null;
  exceptionDate.value = dateKey || toDateKey(new Date());
  exceptionSheetOpen.value = true;
}

function openEditException(rule: AvailabilityRule) {
  editingException.value = rule;
  exceptionDate.value = rule.date ?? '';
  exceptionSheetOpen.value = true;
}

const viewModes = computed<{ label: string, value: MyPlanningViewMode, icon: string }[]>(() => [
  { label: t('planning.viewMode.day'), value: 'day', icon: 'i-lucide-calendar' },
  { label: t('planning.viewMode.week'), value: 'week', icon: 'i-lucide-calendar-range' },
]);

const periodLabel = computed(() => `${weekLabel.value} · ${weekRangeLabel.value}`);

function setViewMode(mode: MyPlanningViewMode) {
  viewMode.value = mode;
}

const todayAction: HeaderAction = {
  type: 'button',
  key: 'today',
  label: t('dashboard.today'),
  icon: 'i-lucide-calendar-check',
  onSelect: goToToday,
};

const headerActions = computed<HeaderAction[]>(() => [
  ...(isCurrentWeek.value ? [] : [todayAction]),
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
  {
    type: 'button',
    key: 'addException',
    label: t('availability.exception'),
    icon: 'i-lucide-calendar-off',
    onSelect: () => {
      openCreateException(selectedDateKey.value);
    },
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
				@create-exception="openCreateException"
				@edit-exception="openEditException"
			/>
		</div>

		<AvailabilityRuleSheet
			v-model:open="exceptionSheetOpen"
			:employee-id="employeeId"
			type="OneTime"
			:rule="editingException"
			:default-date="exceptionDate"
		/>
	</NuxtLayout>
</template>
