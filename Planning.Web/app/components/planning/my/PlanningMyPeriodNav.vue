<script setup lang="ts">
import type { DateValue } from '@internationalized/date';
import { fromDate, getLocalTimeZone, toCalendarDate } from '@internationalized/date';
import { getMonday } from '~/utils/planning/dateUtils';

const props = withDefaults(defineProps<{
  label: string
  weekStart: Date
  isCurrentWeek: boolean
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
}>(), {
  size: 'md',
});

const emit = defineEmits<{
  previous: []
  next: []
  today: []
  selectDate: [date: Date]
}>();

const { t } = useI18n();
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
  emit('selectDate', getMonday(value.toDate(getLocalTimeZone())));
  calendarOpen.value = false;
}

function goToTodayAndClose() {
  emit('today');
  calendarOpen.value = false;
}
</script>

<template>
	<div class="flex min-w-0 flex-wrap items-center gap-2">
		<UFieldGroup>
			<UButton
				variant="outline"
				color="neutral"
				icon="i-lucide-chevron-left"
				:aria-label="t('planning.previousWeek')"
				:size="size"
				@click="emit('previous')"
			/>
			<UButton
				variant="outline"
				color="neutral"
				icon="i-lucide-chevron-right"
				:aria-label="t('planning.nextWeek')"
				:size="size"
				@click="emit('next')"
			/>
		</UFieldGroup>

		<UButton
			v-if="!isCurrentWeek"
			variant="outline"
			color="neutral"
			:label="t('dashboard.today')"
			class="shrink-0"
			:size="size"
			@click="emit('today')"
		/>

		<UPopover v-model:open="calendarOpen">
			<UButton
				variant="ghost"
				color="neutral"
				class="min-w-0 px-2 font-medium text-default hover:bg-elevated"
				:size="size"
			>
				<span class="truncate">{{ label }}</span>
			</UButton>

			<template #content>
				<div class="flex flex-col gap-2 p-2">
					<UCalendar
						v-if="calendarOpen"
						:default-value="calendarDefaultDate"
						color="brand"
						@update:model-value="onCalendarDateSelect"
					/>
					<UButton
						variant="outline"
						:label="t('planning.thisWeek')"
						block
						@click="goToTodayAndClose"
					/>
				</div>
			</template>
		</UPopover>
	</div>
</template>
