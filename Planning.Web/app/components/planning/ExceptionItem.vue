<script setup lang="ts">
import { ApprovalStatus } from '~/generated/models';
import type { AvailabilityRule } from '~/types/availability';
import { getRequestStatusLabel } from '~/types/requests';
import { formatRuleTimeRange } from '~/utils/planning/availabilityMath';
import { UNAVAILABLE_BAR_STYLE, UNAVAILABLE_EDGE_STYLE } from '~/utils/planning/dateUtils';

/**
 * An exception inside a day, built on PlanningShiftItem's footprints so a day reads as one list.
 * It carries no fill of its own: the slate is down to a dashed edge and the bar that stands where
 * a shift carries its colour, which is enough to say "not work" without another surface competing
 * with the shifts above it.
 */
const props = withDefaults(defineProps<{
  rule: AvailabilityRule
  /** Hatched block (week column) vs list row with a hatched surface and a grey bar. */
  variant?: 'block' | 'list'
  flush?: boolean
  removing?: boolean
}>(), {
  variant: 'list',
  flush: false,
  removing: false,
});

const emit = defineEmits<{
  edit: []
  remove: []
}>();

const { t } = useI18n();
const timeLabel = computed(() => formatRuleTimeRange(props.rule.startTime, props.rule.endTime, t));

/**
 * An approved blockade needs no words - it simply is not available. A requested or refused one does,
 * and it takes the place of that line rather than adding a second one: this row is two lines wide in
 * a week column.
 */
const statusLabel = computed(() =>
  props.rule.approvalStatus === ApprovalStatus.Approved ?
    t('availability.unavailable') :
    getRequestStatusLabel(props.rule.approvalStatus, t));

const statusClass = computed(() => {
  switch (props.rule.approvalStatus) {
    case ApprovalStatus.Pending:
      return 'text-warning';
    case ApprovalStatus.Rejected:
      return 'text-error';
    default:
      return 'text-muted';
  }
});
</script>

<template>
	<!--
		The whole block is the edit button, so the pencil is a plain icon rather than a nested
		UButton: interactive content inside a <button> is invalid, and it only has to say that this
		block can be clicked.
	-->
	<button
		v-if="variant === 'block'"
		type="button"
		class="group flex w-full cursor-pointer items-start gap-1 overflow-hidden rounded-md border border-dashed px-2.5 py-1.5 text-left transition-colors hover:bg-elevated"
		:style="UNAVAILABLE_EDGE_STYLE"
		:aria-label="t('common.actions.edit')"
		@click="emit('edit')"
	>
		<span class="min-w-0 flex-1">
			<span class="flex items-center gap-1 truncate text-xs font-semibold leading-tight tabular-nums">
				<UIcon
					name="i-lucide-calendar-off"
					class="size-3 shrink-0"
				/>
				{{ timeLabel }}
			</span>
			<span
				class="mt-0.5 block truncate text-[11px] font-medium uppercase leading-tight tracking-wide"
				:class="statusClass"
			>
				{{ statusLabel }}
			</span>
			<span
				v-if="rule.reason"
				class="mt-0.5 block truncate text-[11px] leading-tight text-muted"
			>
				{{ rule.reason }}
			</span>
		</span>

		<UIcon
			name="i-lucide-pencil"
			class="mt-0.5 size-3 shrink-0 text-dimmed transition-colors group-hover:text-default"
		/>
	</button>

	<div
		v-else
		class="overflow-hidden"
		:class="flush
			? ''
			: 'rounded-lg border border-dashed'"
		:style="UNAVAILABLE_EDGE_STYLE"
	>
		<div class="flex min-w-0 items-start">
			<div
				class="w-1.5 shrink-0 self-stretch"
				:style="UNAVAILABLE_BAR_STYLE"
			/>

			<div
				class="min-w-0 flex-1 space-y-1.5"
				:class="flush ? 'px-4 py-3.5' : 'px-3 py-2.5'"
			>
				<p class="flex items-center gap-1.5 text-sm font-semibold tabular-nums leading-tight">
					<UIcon
						name="i-lucide-calendar-off"
						class="size-4 shrink-0 text-muted"
					/>
					{{ timeLabel }}
				</p>
				<p
					class="truncate text-xs font-medium uppercase leading-tight tracking-wide"
					:class="statusClass"
				>
					{{ statusLabel }}
				</p>
				<p
					v-if="rule.reason"
					class="truncate text-sm leading-tight"
				>
					{{ rule.reason }}
				</p>
			</div>

			<div
				class="flex shrink-0 gap-0.5"
				:class="flush ? 'px-3 py-3' : 'px-2 py-2'"
			>
				<UButton
					icon="i-lucide-pencil"
					variant="ghost"
					color="neutral"
					size="sm"
					:aria-label="t('common.actions.edit')"
					@click="emit('edit')"
				/>
				<UButton
					icon="i-lucide-trash-2"
					variant="ghost"
					color="error"
					size="sm"
					:loading="removing"
					:aria-label="t('common.actions.delete')"
					@click="emit('remove')"
				/>
			</div>
		</div>
	</div>
</template>
