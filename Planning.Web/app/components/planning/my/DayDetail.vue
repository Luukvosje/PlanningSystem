<script setup lang="ts">
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import { formatAgendaDayHeader } from '~/utils/planning/dateUtils';

defineProps<{
  day: MyPlanningDay
}>();
</script>

<template>
	<UCard
		:class="day.isToday
			? 'ring-1 ring-secondary/40 bg-secondary/10'
			: 'bg-muted/20'"
		class="w-full lg:w-full"
		:ui="{ body: 'p-4 sm:p-5' }"
	>
		<div class="space-y-4">
			<div class="flex items-start justify-between gap-3">
				<p
					class="text-sm font-semibold tracking-wide uppercase"
					:class="day.hasShifts ? 'text-default' : 'text-muted'"
				>
					{{ formatAgendaDayHeader(day.date) }}
				</p>

				<UBadge
					v-if="day.isToday"
					color="secondary"
					variant="subtle"
					size="sm"
					label="Vandaag"
				/>
			</div>

			<div
				v-if="day.hasShifts"
				class="space-y-4 divide-y divide-default/60"
			>
				<div
					v-for="(shift, index) in day.shifts"
					:key="shift.id"
					:class="index > 0 ? 'pt-4' : ''"
				>
					<PlanningMyShiftItem :shift="shift" />
				</div>
			</div>

			<p
				v-else
				class="text-sm text-muted"
			>
				Geen dienst gepland
			</p>
		</div>
	</UCard>
</template>
