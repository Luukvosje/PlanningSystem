<script setup lang="ts">
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import type { AvailabilityRule } from '~/types/availability';

export type MyPlanningViewMode = 'day' | 'week';

const props = defineProps<{
  viewMode: MyPlanningViewMode
  days: MyPlanningDay[]
  selectedDateKey: string
  selectedDay: MyPlanningDay | null
  isLoading: boolean
}>();

const emit = defineEmits<{
  selectDay: [dateKey: string]
  createException: [dateKey: string]
  editException: [rule: AvailabilityRule]
}>();

const isDayMode = computed(() => props.viewMode === 'day');

const weekColumnClass = 'min-h-52 border-b border-default sm:min-h-0 sm:h-full sm:border-b-0 sm:border-e lg:last:border-e-0';
</script>

<template>
	<div class="flex h-full min-h-0 w-full max-w-none flex-col">
		<!-- Day mode -->
		<template v-if="isDayMode">
			<div class="shrink-0">
				<PlanningMyDayStrip
					:days="days"
					:selected-date-key="selectedDateKey"
					@select-day="emit('selectDay', $event)"
				/>
			</div>

			<PlanningMyDayDetail
				class="min-h-0 w-full flex-1 grow"
				:day="selectedDay"
				:loading="isLoading"
				flush
				@create-exception="emit('createException', $event)"
				@edit-exception="emit('editException', $event)"
			/>
		</template>

		<!-- Week mode -->
		<div
			v-else
			class="grid min-h-0 w-full flex-1 grow grid-cols-1 overflow-auto sm:grid-cols-2 lg:grid-cols-7 lg:overflow-hidden"
		>
			<template v-if="isLoading">
				<PlanningMyDayDetail
					v-for="index in 7"
					:key="index"
					:class="weekColumnClass"
					loading
					compact
					flush
				/>
			</template>

			<template v-else>
				<PlanningMyDayDetail
					v-for="day in days"
					:key="day.dateKey"
					:class="weekColumnClass"
					:day="day"
					compact
					flush
					@create-exception="emit('createException', $event)"
					@edit-exception="emit('editException', $event)"
				/>
			</template>
		</div>
	</div>
</template>
