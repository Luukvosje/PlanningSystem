<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning';
import { formatAgendaDayHeader, getIntlLocale, toDateKey } from '~/utils/planning/dateUtils';

/**
 * The shifts belonging to one entity, grouped per day — the Planning tab of a customer or an
 * employee detail page.
 */
const props = withDefaults(defineProps<{
  customerId?: string
  userId?: string
}>(), {
  customerId: undefined,
  userId: undefined,
});

const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));

const entityId = computed(() => props.customerId ?? props.userId);

const { shifts, isLoading, error } = useEntityPlanning({
  kind: props.customerId ? 'customer' : 'user',
  id: entityId,
});

const days = computed(() => {
  const groups = new Map<string, { label: string, shifts: PlanningRecord[] }>();

  for (const shift of shifts.value) {
    const start = new Date(shift.startUtc);
    const key = toDateKey(start);
    const group = groups.get(key) ?? { label: formatAgendaDayHeader(start, intlLocale.value), shifts: [] };

    group.shifts.push(shift);
    groups.set(key, group);
  }

  return [...groups.values()];
});
</script>

<template>
	<LayoutSection :title="t('planning.entityShifts.title')">
		<UiQueryState
			:error="error"
			:loading="isLoading"
			:loading-label="t('planning.entityShifts.loading')"
		>
			<UiEmptyState
				v-if="!days.length"
				icon="i-lucide-calendar-x"
				:title="t('planning.entityShifts.empty')"
				:description="t('planning.entityShifts.emptyDescription')"
			/>

			<div
				v-else
				class="flex flex-col gap-4"
			>
				<div
					v-for="day in days"
					:key="day.label"
					class="flex flex-col gap-2"
				>
					<div class="flex items-center gap-2">
						<p class="text-xs font-semibold uppercase tracking-wider text-muted">
							{{ day.label }}
						</p>
						<div class="h-px flex-1 bg-default" />
					</div>

					<div
						v-for="shift in day.shifts"
						:key="shift.id"
						class="flex items-center gap-3"
					>
						<PlanningShiftItem
							:shift="shift"
							class="min-w-0 flex-1"
						/>
						<PlanningCardsStatusBadge :status="shift.status" />
					</div>
				</div>
			</div>
		</UiQueryState>
	</LayoutSection>
</template>
