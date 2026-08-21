<script setup lang="ts">
import type { MyPlanningDay } from '~/composables/planning/useMyPlanningView';
import type { AvailabilityRule } from '~/types/availability';
import { formatAgendaDayHeader, getIntlLocale } from '~/utils/planning/dateUtils';
import { formatOneTimeRuleLabel } from '~/utils/planning/availabilityMath';

const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));
const auth = useAuthStore();
const api = useAvailabilityApi();

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

const shiftCountLabel = computed(() => {
  if (!props.day) {
    return '';
  }

  if (!props.day.hasShifts) {
    return t('planning.noShifts');
  }

  return props.day.shifts.length === 1 ?
    t('planning.shiftCountSingular', { count: props.day.shifts.length }) :
    t('planning.shiftCountPlural', { count: props.day.shifts.length });
});

const employeeId = computed(() => auth.currentUser?.userId ?? '');
const sheetOpen = ref(false);
const editingRule = ref<AvailabilityRule | null>(null);

function openCreateException() {
  editingRule.value = null;
  sheetOpen.value = true;
}

function openEditException(rule: AvailabilityRule) {
  editingRule.value = rule;
  sheetOpen.value = true;
}

async function removeException(rule: AvailabilityRule) {
  await api.remove.mutateAsync(rule.id);
}
</script>

<template>
	<div
		class="flex h-full min-h-0 w-full min-w-0 flex-col overflow-hidden bg-default"
		:class="[
			flush
				? ''
				: (day?.isToday ? 'rounded-xl border border-brand/50' : 'rounded-xl border border-default'),
			compact && day?.isToday ? 'bg-brand/5' : '',
		]"
	>
		<template v-if="loading">
			<div
				class="flex shrink-0 items-center px-3 py-3"
				:class="compact ? '' : 'border-b border-default bg-muted/20 px-4 py-3.5'"
			>
				<div class="space-y-2">
					<USkeleton class="h-3 w-28" />
					<USkeleton
						v-if="!compact"
						class="h-3 w-20"
					/>
				</div>
			</div>
			<div
				class="min-h-0 flex-1 overflow-auto"
				:class="compact ? 'space-y-2 px-2 pb-3' : ''"
			>
				<div
					v-for="index in compact ? 2 : 3"
					:key="index"
					:class="compact
						? ''
						: 'space-y-2 border-b border-default px-4 py-3.5 last:border-b-0'"
				>
					<USkeleton
						class="w-full"
						:class="compact ? 'h-14 rounded-md' : 'h-4 w-28'"
					/>
					<USkeleton
						v-if="!compact"
						class="h-3 w-40"
					/>
					<USkeleton
						v-if="!compact"
						class="h-3 w-52"
					/>
				</div>
			</div>
		</template>

		<template v-else-if="day">
			<!-- Week column header -->
			<div
				v-if="compact"
				class="flex shrink-0 items-center justify-between gap-2 px-3 pb-2 pt-3"
			>
				<p
					class="w-full truncate text-center text-xs font-semibold uppercase tracking-wide text-muted"
					:class="day.isToday ? 'text-brand' : ''"
				>
					{{ formatAgendaDayHeader(day.date, intlLocale) }}
				</p>
			</div>

			<!-- Day view header -->
			<div
				v-else
				class="flex shrink-0 items-center justify-between gap-3 border-b border-default px-4 py-3.5"
				:class="day.isToday ? 'bg-default' : 'bg-muted/20'"
			>
				<div class="min-w-0">
					<p
						class="truncate text-sm font-semibold tracking-wide uppercase"
						:class="day.hasShifts ? 'text-default' : 'text-muted'"
					>
						{{ formatAgendaDayHeader(day.date, intlLocale) }}
					</p>
					<p class="mt-1 truncate text-xs text-muted">
						{{ shiftCountLabel }}
					</p>
				</div>

				<UBadge
					v-if="day.isToday"
					color="brand"
					variant="subtle"
					size="sm"
					:label="t('dashboard.today')"
					class="shrink-0"
				/>
			</div>

			<div class="min-h-0 flex-1 overflow-auto bg-accented/40">
				<div
					v-if="day.hasShifts"
					:class="compact
						? 'flex flex-col gap-2 px-2 pb-3'
						: 'divide-y divide-accented'"
				>
					<PlanningShiftItem
						v-for="shift in day.shifts"
						:key="shift.id"
						:shift="shift"
						:variant="compact ? 'block' : 'list'"
						:flush="!compact"
					/>
				</div>

				<div
					v-else-if="!compact"
					class="flex h-full min-h-32 flex-col items-center justify-center gap-2.5 px-4 py-8 text-center"
				>
					<UIcon
						name="i-lucide-calendar-off"
						class="size-8 text-muted"
					/>
					<p class="text-sm text-muted">
						{{ t('planning.noShiftPlanned') }}
					</p>
				</div>

				<div
					v-if="!compact"
					class="space-y-2 border-t border-default px-4 py-3.5"
				>
					<div class="flex items-center justify-between gap-3">
						<p class="text-xs font-semibold uppercase tracking-wide text-muted">
							{{ t('availability.oneTimeExceptions') }}
						</p>
						<UButton
							icon="i-lucide-plus"
							:label="t('common.actions.add')"
							variant="ghost"
							size="xs"
							@click="openCreateException"
						/>
					</div>

					<p
						v-if="!day.exceptions.length"
						class="text-sm text-muted"
					>
						{{ t('availability.noOneTimeExceptions') }}
					</p>

					<div
						v-else
						class="space-y-1.5"
					>
						<div
							v-for="rule in day.exceptions"
							:key="rule.id"
							class="flex items-start justify-between gap-2 rounded-md border border-default px-2.5 py-1.5"
						>
							<p class="min-w-0 truncate text-sm">
								{{ formatOneTimeRuleLabel(rule.date!, rule.startTime, rule.endTime, t, intlLocale, rule.reason) }}
							</p>
							<div class="flex shrink-0 gap-0.5">
								<UButton
									icon="i-lucide-pencil"
									variant="ghost"
									color="neutral"
									size="xs"
									@click="openEditException(rule)"
								/>
								<UButton
									icon="i-lucide-trash-2"
									variant="ghost"
									color="error"
									size="xs"
									:loading="api.remove.isPending.value"
									@click="removeException(rule)"
								/>
							</div>
						</div>
					</div>
				</div>
			</div>
		</template>

		<AvailabilityRuleSheet
			v-if="day"
			v-model:open="sheetOpen"
			:employee-id="employeeId"
			type="OneTime"
			:rule="editingRule"
			:default-date="day.dateKey"
		/>
	</div>
</template>
