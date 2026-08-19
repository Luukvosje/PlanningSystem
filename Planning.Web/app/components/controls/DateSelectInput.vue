<script setup lang="ts">
import { CalendarDate } from '@internationalized/date';
import { DateTime } from 'luxon';
const inputDate = useTemplateRef('inputDate');
const props = defineProps<{
	disabled?: boolean;
}>();

//IsoString
const modelValue = defineModel<string>({ required: true, default: DateTime.now().toISODate() });
const date = computed(() => DateTime.fromISO(modelValue.value));

const value = computed({
    get: () => new CalendarDate(date.value.year, date.value.month, date.value.day),
    set: (value: CalendarDate) => {
        modelValue.value = DateTime.fromISO(value.toString()).toISODate() ?? '';
		return modelValue.value;
    },
});
</script>

<template>
	<UInputDate
		ref="inputDate"
		v-model="value"
		class="w-full"
		:disabled="props.disabled"
	>
		<template #trailing>
			<UPopover :reference="inputDate?.inputsRef[3]?.$el">
				<UButton
					color="neutral"
					variant="link"
					size="sm"
					icon="i-lucide-calendar"
					aria-label="Select a date"
					class="px-0"
				/>

				<template #content>
					<UCalendar
						v-model="value"
						class="p-2"
					/>
				</template>
			</UPopover>
		</template>
	</UInputDate>
</template>

