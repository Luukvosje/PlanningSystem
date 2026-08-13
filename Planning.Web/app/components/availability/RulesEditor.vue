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
			<section class="space-y-3">
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

				<UCard v-if="weeklyRules.length === 0">
					<p class="text-sm text-muted text-center py-2">
						{{ t('availability.noWeeklyBlocks') }}
					</p>
				</UCard>

				<div
					v-else
					class="space-y-2"
				>
					<UCard
						v-for="rule in weeklyRules"
						:key="rule.id"
						:ui="{ body: 'p-3 sm:p-4' }"
					>
						<div class="flex items-start justify-between gap-3">
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
									@click="openWeeklyEdit(rule)"
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
					</UCard>
				</div>
			</section>

			<section class="space-y-3">
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

				<UCard v-if="oneTimeRules.length === 0">
					<p class="text-sm text-muted text-center py-2">
						{{ t('availability.noOneTimeExceptions') }}
					</p>
				</UCard>

				<div
					v-else
					class="space-y-2"
				>
					<UCard
						v-for="rule in oneTimeRules"
						:key="rule.id"
						:ui="{ body: 'p-3 sm:p-4' }"
					>
						<div class="flex items-start justify-between gap-3">
							<div class="min-w-0">
								<p class="font-medium">
									{{ formatOneTimeRuleLabel(rule.date!, rule.startTime, rule.endTime, t, intlLocale, rule.reason) }}
								</p>
							</div>
							<div class="flex shrink-0 gap-1">
								<UButton
									icon="i-lucide-pencil"
									variant="ghost"
									color="neutral"
									size="sm"
									@click="openOneTimeEdit(rule)"
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
					</UCard>
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
