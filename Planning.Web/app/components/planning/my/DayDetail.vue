<script setup lang="ts">
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import { formatAgendaDayHeader, formatCompactDayHeader } from '~/utils/planning/dateUtils';

const props = withDefaults(defineProps<{
  day?: MyPlanningDay | null
  loading?: boolean
  compact?: boolean
  flush?: boolean
}>(), {
  day: null,
  loading: false,
  compact: false,
  flush: false,
});

const compactHeader = computed(() =>
  props.day ? formatCompactDayHeader(props.day.date) : null,
);

const shiftCountLabel = computed(() => {
  if (!props.day) {
    return '';
  }

  if (!props.day.hasShifts) {
    return 'Geen diensten';
  }

  return `${props.day.shifts.length} ${props.day.shifts.length === 1 ? 'dienst' : 'diensten'}`;
});
</script>

<template>
	<div
		class="flex h-full min-h-0 w-full min-w-0 flex-col overflow-hidden bg-default"
		:class="flush
			? (day?.isToday ? 'ring-1 ring-inset ring-secondary/40' : '')
			: (day?.isToday ? 'rounded-xl border border-secondary/50' : 'rounded-xl border border-default')"
	>
		<template v-if="loading">
			<div class="flex shrink-0 items-center border-b border-default bg-muted/20 px-4 py-3.5">
				<div class="space-y-2">
					<USkeleton class="h-4 w-36" />
					<USkeleton class="h-3 w-24" />
				</div>
			</div>
			<div class="min-h-0 flex-1 overflow-auto">
				<div
					v-for="index in 3"
					:key="index"
					class="space-y-2 border-b border-default px-4 py-3.5 last:border-b-0"
				>
					<USkeleton class="h-4 w-28" />
					<USkeleton class="h-3 w-40" />
					<USkeleton class="h-3 w-52" />
				</div>
			</div>
		</template>

		<template v-else-if="day">
			<div
				class="flex shrink-0 items-center justify-between gap-3 border-b border-default px-4 py-3.5"
				:class="day.isToday ? 'bg-secondary/10' : 'bg-muted/20'"
			>
				<div class="min-w-0">
					<p
						class="truncate text-sm font-semibold tracking-wide uppercase"
						:class="day.hasShifts ? 'text-default' : 'text-muted'"
					>
						<template v-if="compact && compactHeader">
							{{ compactHeader.weekday }}
							<span class="ml-1 tabular-nums">{{ compactHeader.day }}</span>
						</template>
						<template v-else>
							{{ formatAgendaDayHeader(day.date) }}
						</template>
					</p>
					<p class="mt-1 truncate text-xs text-muted">
						{{ shiftCountLabel }}
					</p>
				</div>

				<UBadge
					v-if="day.isToday"
					color="secondary"
					variant="subtle"
					size="sm"
					label="Vandaag"
					class="shrink-0"
				/>
			</div>

			<div class="min-h-0 flex-1 overflow-auto">
				<div
					v-if="day.hasShifts"
					class="divide-y divide-accented"
				>
					<PlanningMyShiftItem
						v-for="shift in day.shifts"
						:key="shift.id"
						:shift="shift"
						flush
					/>
				</div>

				<div
					v-else
					class="flex h-full min-h-32 flex-col items-center justify-center gap-2.5 px-4 py-8 text-center"
				>
					<UIcon
						name="i-lucide-calendar-off"
						class="size-8 text-muted"
					/>
					<p class="text-sm text-muted">
						Geen dienst gepland
					</p>
				</div>
			</div>
		</template>
	</div>
</template>
