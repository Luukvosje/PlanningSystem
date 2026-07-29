<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning';
import { formatTimeRange } from '~/utils/planning/dateUtils';

defineProps<{
  shift: PlanningRecord
}>();
</script>

<template>
	<div class="space-y-1">
		<p class="text-base font-semibold tabular-nums">
			{{ formatTimeRange(shift.startUtc, shift.endUtc) }}
		</p>
		<p
			v-if="shift.customerName"
			class="text-sm font-medium"
		>
			{{ shift.customerName }}
		</p>
		<p
			v-if="shift.title"
			class="text-sm text-muted"
		>
			{{ shift.title }}
		</p>
		<p
			v-if="shift.notes"
			class="text-sm text-muted"
		>
			<span class="font-medium text-default">Notitie:</span>
			{{ shift.notes }}
		</p>
	</div>
</template>
