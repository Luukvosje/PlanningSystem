<script setup lang="ts">
/**
 * One availability rule in a list.
 *
 * The weekly and one-time lists in RulesEditor gave every rule a full LayoutCard, while
 * WeeklyPatternCard rendered the same data as a light bordered row - the same information at two
 * very different visual weights. A rule is a row, not a card.
 */
import { ApprovalStatus } from '~/generated/models';
import { getRequestStatusColor, getRequestStatusLabel } from '~/types/requests';

const props = withDefaults(defineProps<{
	label: string
	description?: string | null
	/** Only shown when it is not simply approved - a rule in force does not need a badge. */
	approvalStatus?: ApprovalStatus
	removing?: boolean
}>(), {
	description: undefined,
	approvalStatus: ApprovalStatus.Approved,
	removing: false,
});

const pendingOrRejected = computed(() => props.approvalStatus !== ApprovalStatus.Approved);

defineEmits<{ edit: []; remove: [] }>();

const { t } = useI18n();
</script>

<template>
	<div class="flex items-center justify-between gap-3 rounded-md border border-default px-3 py-2">
		<div class="min-w-0 ">
			<div class="flex min-w-0 items-center gap-2">
				<p class="truncate text-sm font-medium">
					{{ label }}
				</p>
				<UBadge
					v-if="pendingOrRejected"
					:color="getRequestStatusColor(approvalStatus)"
					variant="subtle"
					size="sm"
					class="shrink-0"
					:label="getRequestStatusLabel(approvalStatus, t)"
				/>
			</div>
			<p
				v-if="description"
				class="mt-0.5 truncate text-xs text-muted"
			>
				{{ description }}
			</p>
		</div>

		<div class="flex shrink-0 gap-1">
			<UButton
				icon="i-lucide-pencil"
				variant="ghost"
				color="neutral"
				size="sm"
				:aria-label="t('common.actions.edit')"
				@click="$emit('edit')"
			/>
			<UButton
				icon="i-lucide-trash-2"
				variant="ghost"
				color="error"
				size="sm"
				:loading="removing"
				:aria-label="t('common.actions.delete')"
				@click="$emit('remove')"
			/>
		</div>
	</div>
</template>
