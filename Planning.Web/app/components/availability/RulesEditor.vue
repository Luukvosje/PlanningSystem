<script setup lang="ts">
import type { AvailabilityRule } from '~/types/availability';
import { formatOneTimeRuleLabel, formatWeeklyRuleLabel } from '~/utils/planning/availabilityMath';
import { getIntlLocale } from '~/utils/planning/dateUtils';

const props = defineProps<{
  employeeId: string
  canSelectUser?: boolean
}>();

const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));
const { data: users } = useUsers();
const api = useAvailabilityApi();

const selectedEmployeeId = ref(props.employeeId);

watch(() => props.employeeId, (value) => {
  selectedEmployeeId.value = value;
});

const { data: rulesData, isLoading } = useAvailabilityRules({
  employeeId: selectedEmployeeId,
});

const weeklyRules = computed(() =>
  (rulesData.value?.items ?? []).filter((rule) => rule.type === 'Weekly'),
);

const oneTimeRules = computed(() =>
  (rulesData.value?.items ?? [])
    .filter((rule) => rule.type === 'OneTime')
    .sort((a, b) => (a.date ?? '').localeCompare(b.date ?? '')),
);

const employeeOptions = computed(() =>
  (users.value ?? [])
    .filter((user) => user.isActive !== false)
    .map((user) => ({
      label: `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || t('planning.fields.employee'),
      value: user.id!,
    })),
);

const weeklySheetOpen = ref(false);
const oneTimeSheetOpen = ref(false);
const editingRule = ref<AvailabilityRule | null>(null);

function openWeeklyCreate() {
  editingRule.value = null;
  weeklySheetOpen.value = true;
}

function openWeeklyEdit(rule: AvailabilityRule) {
  editingRule.value = rule;
  weeklySheetOpen.value = true;
}

function openOneTimeCreate() {
  editingRule.value = null;
  oneTimeSheetOpen.value = true;
}

function openOneTimeEdit(rule: AvailabilityRule) {
  editingRule.value = rule;
  oneTimeSheetOpen.value = true;
}

async function removeRule(rule: AvailabilityRule) {
  await api.remove.mutateAsync(rule.id);
}
</script>

<template>
	<div class="space-y-8">
		<UAlert
			v-if="canSelectUser"
			color="info"
			variant="subtle"
			:title="t('availability.managingOther')"
		/>

		<UFormField
			v-if="canSelectUser"
			:label="t('planning.fields.employee')"
		>
			<USelect
				v-model="selectedEmployeeId"
				:items="employeeOptions"
				value-key="value"
				label-key="label"
				class="w-full max-w-md"
			/>
		</UFormField>

		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('availability.loading')"
		/>

		<template v-else>
			<section class="flex flex-col gap-3">
				<div class="flex items-center justify-between gap-3">
					<div>
						<h2 class="text-lg font-medium">
							{{ t('availability.weeklyBlocks') }}
						</h2>
						<p class="text-sm text-muted">
							{{ t('availability.weeklyBlocksDescription') }}
						</p>
					</div>
					<UButton
						icon="i-lucide-plus"
						:label="t('common.actions.add')"
						@click="openWeeklyCreate"
					/>
				</div>

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
						@edit="openWeeklyEdit(rule)"
						@remove="removeRule(rule)"
					/>
				</div>
			</section>

			<section class="flex flex-col gap-3">
				<div class="flex items-center justify-between gap-3">
					<div>
						<h2 class="text-lg font-medium">
							{{ t('availability.oneTimeExceptions') }}
						</h2>
						<p class="text-sm text-muted">
							{{ t('availability.oneTimeExceptionsDescription') }}
						</p>
					</div>
					<UButton
						icon="i-lucide-plus"
						:label="t('common.actions.add')"
						@click="openOneTimeCreate"
					/>
				</div>

				<UiEmptyState
					v-if="oneTimeRules.length === 0"
					:title="t('availability.noOneTimeExceptions')"
				/>

				<div
					v-else
					class="flex flex-col gap-2"
				>
					<AvailabilityRuleRow
						v-for="rule in oneTimeRules"
						:key="rule.id"
						:label="formatOneTimeRuleLabel(rule.date!, rule.startTime, rule.endTime, t, intlLocale, rule.reason)"
						:removing="api.remove.isPending.value"
						@edit="openOneTimeEdit(rule)"
						@remove="removeRule(rule)"
					/>
				</div>
			</section>
		</template>

		<AvailabilityRuleSheet
			v-model:open="weeklySheetOpen"
			:employee-id="selectedEmployeeId"
			type="Weekly"
			:rule="editingRule?.type === 'Weekly' ? editingRule : null"
		/>

		<AvailabilityRuleSheet
			v-model:open="oneTimeSheetOpen"
			:employee-id="selectedEmployeeId"
			type="OneTime"
			:rule="editingRule?.type === 'OneTime' ? editingRule : null"
		/>
	</div>
</template>
