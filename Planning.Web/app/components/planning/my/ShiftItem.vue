<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning';
import { CONCEPT_BLOCK_STYLE, formatTimeRange, getBlockColor } from '~/utils/planning/dateUtils';

const props = withDefaults(defineProps<{
  shift: PlanningRecord
  flush?: boolean
}>(), {
  flush: false,
});

const isConcept = computed(() => props.shift.status === 'Planned');
const blockColor = computed(() =>
  isConcept.value
    ? CONCEPT_BLOCK_STYLE.backgroundColor
    : getBlockColor(props.shift.color, props.shift.status),
);
</script>

<template>
	<div
		class="overflow-hidden"
		:class="flush
			? 'bg-transparent'
			: 'rounded-lg border border-default bg-muted/10'"
	>
		<div class="flex min-w-0">
			<div
				class="w-1.5 shrink-0 self-stretch"
				:style="isConcept
					? { ...CONCEPT_BLOCK_STYLE }
					: { backgroundColor: blockColor }"
			/>

			<div
				class="min-w-0 flex-1 space-y-1.5"
				:class="flush ? 'px-4 py-3.5' : 'px-3 py-2.5'"
			>
				<p class="text-sm font-semibold tabular-nums leading-tight">
					{{ formatTimeRange(shift.startUtc, shift.endUtc) }}
				</p>
				<p
					v-if="shift.title"
					class="truncate text-sm font-medium leading-tight"
				>
					{{ shift.title }}
				</p>
				<p
					v-if="shift.customerName"
					class="truncate text-xs text-muted leading-tight"
				>
					{{ shift.customerName }}
				</p>
				<p
					v-if="shift.notes"
					class="text-xs text-muted leading-snug"
				>
					<span class="font-medium text-default">Notitie:</span>
					{{ shift.notes }}
				</p>
			</div>
		</div>
	</div>
</template>
