<script setup lang="ts">
import type { DateValue } from '@internationalized/date';
import { fromDate, getLocalTimeZone, toCalendarDate } from '@internationalized/date';

withDefaults(defineProps<{
  label: string
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
}>(), {
  size: 'md',
});

const emit = defineEmits<{
  previous: []
  next: []
}>();

const selectedDate = defineModel<Date>({ required: true });

const { t } = useI18n();
const calendarOpen = ref(false);

const calendarDate = computed<DateValue | undefined>({
  get: () => toCalendarDate(fromDate(selectedDate.value, getLocalTimeZone())),
  set: (value) => {
    if (!value) {
      return;
    }
    selectedDate.value = value.toDate(getLocalTimeZone());
    calendarOpen.value = false;
  },
});

function goToTodayAndClose() {
  selectedDate.value = new Date();
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

		<UPopover v-model:open="calendarOpen">
			<UButton
				variant="outline"
				color="neutral"
				icon="i-lucide-calendar"
				trailing-icon="i-lucide-chevron-down"
				class="min-w-0 font-medium"
				:size="size"
			>
				<span class="truncate">{{ label }}</span>
			</UButton>

			<template #content>
				<div class="flex flex-col gap-2 p-2">
					<UCalendar
						v-model="calendarDate"
						color="brand"
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
