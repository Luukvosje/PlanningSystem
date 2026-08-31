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

defineSlots<{
  /** Quick picks for a single boundary: `current` is `HH:mm`, `apply` writes a new one. */
  'startPresets'?: (props: { current: string, apply: (time: string) => void }) => unknown
  'endPresets'?: (props: { current: string, apply: (time: string) => void }) => unknown
}>();

const { t } = useI18n();

const {
  startTime,
  startDate,
  endTime,
  endDate,
  startTimeText,
  endTimeText,
  applyStartTime,
  applyEndTime,
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
	<div
		class="divide-y divide-default rounded-lg border border-default bg-elevated/20"
		:class="{ 'border-error/60': validationError }"
	>
		<div class="grid grid-cols-[3rem_1fr_1fr] items-center gap-x-2 gap-y-1.5 p-2">
			<span class="text-xs text-muted">{{ t('dateTimeRange.start') }}</span>
			<UInputTime
				v-model="startTime"
				:hour-cycle="24"
				:disabled="disabled"
				size="sm"
				class="w-full"
			/>
			<ControlsDateSelectInput
				v-model="startDate"
				:disabled="disabled"
				size="sm"
			/>
			<div
				v-if="$slots.startPresets"
				class="col-start-2 col-end-4"
			>
				<slot
					name="startPresets"
					:current="startTimeText"
					:apply="applyStartTime"
				/>
			</div>

			<span class="text-xs text-muted">{{ t('dateTimeRange.end') }}</span>
			<UInputTime
				v-model="endTime"
				:hour-cycle="24"
				:disabled="disabled"
				size="sm"
				class="w-full"
			/>
			<ControlsDateSelectInput
				v-model="endDate"
				:disabled="disabled"
				size="sm"
			/>
			<div
				v-if="$slots.endPresets"
				class="col-start-2 col-end-4"
			>
				<slot
					name="endPresets"
					:current="endTimeText"
					:apply="applyEndTime"
				/>
			</div>
		</div>

		<div class="flex items-center px-2 py-1.5">
			<span class="ms-auto flex items-center gap-1 whitespace-nowrap text-xs text-muted">
				<UIcon
					:name="multiDayLabel ? 'i-lucide-calendar-range' : 'i-lucide-timer'"
					class="size-3.5"
				/>
				{{ multiDayLabel ? `${durationLabel} · ${multiDayLabel}` : durationLabel }}
			</span>
		</div>

		<div
			v-if="suggestsNextDay"
			class="flex items-center justify-between gap-2 px-2 py-1.5"
		>
			<p class="text-xs text-warning">
				{{ t('dateTimeRange.endBeforeStartSuggestion') }}
			</p>
			<UButton
				size="xs"
				variant="soft"
				color="warning"
				:label="t('dateTimeRange.plusOneDay')"
				:disabled="disabled"
				@click="applyNextDayEnd"
			/>
		</div>
	</div>
</template>
