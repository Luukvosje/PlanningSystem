<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning';
import { CONCEPT_BLOCK_STYLE, formatTimeRange, getBlockColor } from '~/utils/planning/dateUtils';

const props = withDefaults(defineProps<{
  shift: PlanningRecord
  /** Colored filled card (week board) vs list row with accent bar. */
  variant?: 'block' | 'list'
  flush?: boolean
}>(), {
  variant: 'list',
  flush: false,
});

const isConcept = computed(() => props.shift.status === 'Planned');
const blockColor = computed(() =>
  isConcept.value
    ? CONCEPT_BLOCK_STYLE.backgroundColor
    : getBlockColor(props.shift.color, props.shift.status),
);

const blockStyle = computed(() =>
  isConcept.value
    ? { ...CONCEPT_BLOCK_STYLE, color: '#fff' }
    : { backgroundColor: blockColor.value, color: '#fff' },
);

const timeLabel = computed(() => formatTimeRange(props.shift.startUtc, props.shift.endUtc));
</script>

<template>
	<div
		v-if="variant === 'block'"
		class="rounded-md px-2.5 py-2 shadow-sm"
		:style="blockStyle"
	>
		<p class="truncate text-sm font-semibold leading-tight text-white">
			{{ shift.title || 'Dienst' }}
		</p>
		<p class="mt-0.5 truncate text-xs leading-tight text-white/90 tabular-nums">
			{{ timeLabel }}
		</p>
		<p
			v-if="shift.customerName"
			class="mt-0.5 truncate text-[11px] leading-tight text-white/80"
		>
			{{ shift.customerName }}
		</p>
	</div>

	<div
		v-else
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
					{{ timeLabel }}
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
