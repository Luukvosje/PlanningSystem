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

		<UiQueryState
			:loading="isLoading"
			:loading-label="t('availability.loading')"
		>
			<UiEmptyState
				v-if="weeklyRules.length === 0"
				:title="t('availability.noWeeklyBlocks')"
			/>

			<div
				v-else
				class="flex flex-col gap-2"
			>
				<AvailabilityRuleRow
					v-for="rule in weeklyRules"
					:key="rule.id"
					:label="formatWeeklyRuleLabel(rule.weekday!, rule.startTime, rule.endTime, t)"
					:description="rule.reason"
					:removing="api.remove.isPending.value"
					@edit="openEdit(rule)"
					@remove="removeRule(rule)"
				/>
			</div>
		</UiQueryState>

		<AvailabilityRuleSheet
			v-model:open="sheetOpen"
			:employee-id="employeeId"
			type="Weekly"
			:rule="editingRule"
		/>
	</LayoutCard>
</template>
