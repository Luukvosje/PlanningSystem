<script setup lang="ts">
import type { Weekday } from '~/types/availability';
import { getWeekdayLabel } from '~/types/availability';

export interface OpeningHoursRow {
  day: Weekday
  enabled: boolean
  openTime: string
  closeTime: string
}

withDefaults(defineProps<{
  disabled?: boolean
}>(), {
  disabled: false,
});

const model = defineModel<OpeningHoursRow[]>({ required: true });
const { t } = useI18n();
</script>

<template>
	<div class="space-y-2">
		<div
			v-for="row in model"
			:key="row.day"
			class="grid grid-cols-[1fr_auto_auto_auto] items-center gap-2"
		>
			<div class="flex items-center gap-2">
				<USwitch
					v-model="row.enabled"
					:disabled="disabled"
					size="sm"
				/>
				<span class="text-sm">{{ getWeekdayLabel(row.day, t) }}</span>
			</div>
			<UInput
				v-model="row.openTime"
				type="time"
				class="w-32"
				:disabled="disabled || !row.enabled"
			/>
			<span class="text-sm text-muted">–</span>
			<UInput
				v-model="row.closeTime"
				type="time"
				class="w-32"
				:disabled="disabled || !row.enabled"
			/>
		</div>
	</div>
</template>
