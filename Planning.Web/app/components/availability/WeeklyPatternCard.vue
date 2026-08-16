<script setup lang="ts">
import type { AvailabilityRule } from '~/types/availability';
import { formatWeeklyRuleLabel } from '~/utils/planning/availabilityMath';

const { t } = useI18n();
const auth = useAuthStore();
const api = useAvailabilityApi();

const employeeId = computed(() => auth.currentUser?.userId ?? '');

const { data: rulesData, isLoading } = useAvailabilityRules({
  employeeId,
});

const weeklyRules = computed(() =>
  (rulesData.value?.items ?? []).filter((rule) => rule.type === 'Weekly'),
);

const sheetOpen = ref(false);
const editingRule = ref<AvailabilityRule | null>(null);

function openCreate() {
  editingRule.value = null;
  sheetOpen.value = true;
}

function openEdit(rule: AvailabilityRule) {
  editingRule.value = rule;
  sheetOpen.value = true;
}

async function removeRule(rule: AvailabilityRule) {
  await api.remove.mutateAsync(rule.id);
}
</script>

<template>
	<LayoutCard
		:title="t('availability.weeklyBlocks')"
		:description="t('availability.weeklyBlocksDescription')"
	>
		<template #actions>
			<UButton
				icon="i-lucide-plus"
				:label="t('common.actions.add')"
				size="sm"
				@click="openCreate"
			/>
		</template>

		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('availability.loading')"
		/>

		<p
			v-else-if="weeklyRules.length === 0"
			class="text-sm text-muted text-center py-2"
		>
			{{ t('availability.noWeeklyBlocks') }}
		</p>

		<div
			v-else
			class="space-y-2"
		>
			<div
				v-for="rule in weeklyRules"
				:key="rule.id"
				class="flex items-start justify-between gap-3 rounded-md border border-default px-3 py-2"
			>
				<div class="min-w-0">
					<p class="font-medium">
						{{ formatWeeklyRuleLabel(rule.weekday!, rule.startTime, rule.endTime, t) }}
					</p>
					<p
						v-if="rule.reason"
						class="text-sm text-muted mt-0.5"
					>
						{{ rule.reason }}
					</p>
				</div>
				<div class="flex shrink-0 gap-1">
					<UButton
						icon="i-lucide-pencil"
						variant="ghost"
						color="neutral"
						size="sm"
						@click="openEdit(rule)"
					/>
					<UButton
						icon="i-lucide-trash-2"
						variant="ghost"
						color="error"
						size="sm"
						:loading="api.remove.isPending.value"
						@click="removeRule(rule)"
					/>
				</div>
			</div>
		</div>

		<AvailabilityRuleSheet
			v-model:open="sheetOpen"
			:employee-id="employeeId"
			type="Weekly"
			:rule="editingRule"
		/>
	</LayoutCard>
</template>
