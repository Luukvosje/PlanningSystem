<script setup lang="ts">
const props = withDefaults(defineProps<{
  disabled?: boolean
  autoAdvanceEndDate?: boolean
}>(), {
  disabled: false,
  autoAdvanceEndDate: true,
});

const start = defineModel<Date>('start', { required: true });
const end = defineModel<Date>('end', { required: true });

const {
  startTime,
  startDate,
  endTime,
  endDate,
  isMultiDay,
  multiDayLabel,
  durationLabel,
  validationError,
  suggestsNextDay,
  applyNextDayEnd,
} = useDateTimeRange(start, end, {
  autoAdvanceEndDate: props.autoAdvanceEndDate,
});
</script>

<template>
	<div class="space-y-4">
		<div
			class="flex flex-wrap items-center gap-2 rounded-lg border border-default/60 bg-elevated/30 px-3 py-2"
			:class="{ 'border-error/40 bg-error/5': validationError }"
		>
			<div class="flex items-center gap-1.5 text-sm font-medium">
				<UIcon
					name="i-lucide-timer"
					class="size-4 text-muted"
				/>
				<span>{{ durationLabel }}</span>
			</div>

			<UBadge
				v-if="isMultiDay"
				color="info"
				variant="subtle"
				size="sm"
				icon="i-lucide-calendar-range"
			>
				Meerdere dagen
			</UBadge>

			<span
				v-if="multiDayLabel"
				class="text-xs text-muted"
			>
				{{ multiDayLabel }}
			</span>
		</div>

		<div class="space-y-2">
			<p class="text-sm font-medium text-highlighted">
				Start
			</p>
			<div class="grid grid-cols-2 gap-2">
				<UFormField
					label="Tijd"
					name="startTime"
				>
					<UInputTime
						v-model="startTime"
						:hour-cycle="24"
						:disabled="disabled"
						icon="i-lucide-clock"
						class="w-full"
					/>
				</UFormField>
				<UFormField
					label="Datum"
					name="startDate"
				>
					<UInputDate
						v-model="startDate"
						:disabled="disabled"
						icon="i-lucide-calendar"
						class="w-full"
					/>
				</UFormField>
			</div>
		</div>

		<div class="space-y-2">
			<p class="text-sm font-medium text-highlighted">
				Einde
			</p>
			<div class="grid grid-cols-2 gap-2">
				<UFormField
					label="Tijd"
					name="endTime"
				>
					<UInputTime
						v-model="endTime"
						:hour-cycle="24"
						:disabled="disabled"
						icon="i-lucide-clock"
						class="w-full"
					/>
				</UFormField>
				<UFormField
					label="Datum"
					name="endDate"
				>
					<UInputDate
						v-model="endDate"
						:disabled="disabled"
						icon="i-lucide-calendar"
						class="w-full"
					/>
				</UFormField>
			</div>
		</div>

		<div
			v-if="suggestsNextDay"
			class="flex items-center justify-between gap-2 rounded-lg border border-warning/30 bg-warning/5 px-3 py-2"
		>
			<p class="text-xs text-warning">
				Eindtijd ligt vóór de starttijd. Wil je de volgende dag gebruiken?
			</p>
			<UButton
				size="xs"
				variant="soft"
				color="warning"
				label="+1 dag"
				:disabled="disabled"
				@click="applyNextDayEnd"
			/>
		</div>

		<p
			v-if="validationError"
			class="text-xs text-error"
		>
			{{ validationError }}
		</p>
	</div>
</template>
