<script setup lang="ts">
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import { formatAgendaDayHeader } from '~/utils/planning/dateUtils';

defineProps<{
  day: MyPlanningDay
  selected?: boolean
}>();

const emit = defineEmits<{
  select: []
}>();
</script>

<template>
	<button
		type="button"
		class="w-full rounded-xl border px-4 py-3 text-left transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-secondary"
		:class="[
			selected
				? 'border-secondary bg-secondary/10 ring-1 ring-secondary/40'
				: day.isToday
					? 'border-secondary/40 bg-secondary/10'
					: 'border-default bg-muted/20 hover:bg-muted/40',
		]"
		:aria-pressed="selected"
		@click="emit('select')"
	>
		<div class="flex items-center justify-between gap-3">
			<p
				class="text-sm font-semibold tracking-wide uppercase"
				:class="day.hasShifts ? 'text-default' : 'text-muted'"
			>
				{{ formatAgendaDayHeader(day.date) }}
			</p>

			<div class="flex shrink-0 items-center gap-2">
				<UBadge
					v-if="day.isToday"
					color="secondary"
					variant="subtle"
					size="sm"
					label="Vandaag"
				/>
				<UIcon
					name="i-lucide-chevron-right"
					class="size-4"
					:class="day.hasShifts ? 'text-default' : 'text-muted'"
				/>
			</div>
		</div>
	</button>
</template>
