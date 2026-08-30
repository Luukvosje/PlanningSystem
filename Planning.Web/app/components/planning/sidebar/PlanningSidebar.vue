<script setup lang="ts">
import type { Form as UFormInstance, FormSubmitEvent } from '#ui/types';
import type { PlanningFormData } from '~/types/planning';
import { DEFAULT_PLANNING_COLOR } from '~/types/planning';
import { OPEN_SHIFT_SELECT_VALUE } from '~/utils/planning/constants';
import { createPlanningRecordSchema, type PlanningRecordSchema } from '~/schemas/planning.schema';
import { toFormErrors } from '~/lib/form/types';

const { t } = useI18n();
const store = usePlanningStore();
const { canManage, selectedRecord, createRecord, deleteRecord, duplicateRecord } = usePlanning();
const api = usePlanningApi();
const toast = useToast();

const { data: users } = useUsers();

const form = reactive<PlanningFormData>({
  title: '',
  description: '',
  notes: '',
  assignedUserId: OPEN_SHIFT_SELECT_VALUE,
  customerId: null,
  status: 'Confirmed',
  color: DEFAULT_PLANNING_COLOR,
  startUtc: '',
  endUtc: '',
});

const schema = computed(() => createPlanningRecordSchema(t));

const isCreateMode = computed(() => store.sidebarMode === 'create');
const showForm = computed(() => isCreateMode.value || !!selectedRecord.value);
const titleInput = useTemplateRef<{ inputRef?: HTMLInputElement | null }>('titleInput');
const planningForm = useTemplateRef<UFormInstance<PlanningRecordSchema>>('planningForm');

watch(
  () => [store.sidebarOpen, store.sidebarMode, store.createDraft, store.selectedPlanningId] as const,
  async ([open]) => {
    if (!open) {
      return;
    }
    await nextTick();
    requestAnimationFrame(() => {
      titleInput.value?.inputRef?.focus();
    });
  },
);

watch(selectedRecord, (record) => {
  if (!record || isCreateMode.value) {
    return;
  }
  form.title = record.title;
  form.description = record.description ?? '';
  form.notes = record.notes ?? '';
  form.assignedUserId = record.assignedUserId ?? OPEN_SHIFT_SELECT_VALUE;
  form.customerId = record.customerId ?? null;
  form.status = record.status;
  form.color = record.color;
  form.startUtc = record.startUtc;
  form.endUtc = record.endUtc;
}, { immediate: true });

watch(() => store.createDraft, (draft) => {
  if (!draft || !isCreateMode.value) {
    return;
  }
  form.title = '';
  form.description = '';
  form.notes = '';
  form.assignedUserId = draft.assignedUserId ?? OPEN_SHIFT_SELECT_VALUE;
  form.customerId = draft.customerId ?? null;
  form.status = draft.status ?? store.filters.statuses[0] ?? 'Confirmed';
  form.color = DEFAULT_PLANNING_COLOR;
  form.startUtc = draft.startUtc;
  form.endUtc = draft.endUtc;
}, { immediate: true });

// While creating, the board's own filters narrow the choice: a row you cannot see is a row you
// did not mean to plan on. Editing an existing record keeps the full list, or you could not move
// a shift to someone the filter hides.
const userRestriction = computed(() =>
  (isCreateMode.value ? store.filters.userIds : []));

const customerRestriction = computed(() =>
  (isCreateMode.value ? store.filters.customerIds : []));

const openShiftOption = computed(() =>
  [{ id: OPEN_SHIFT_SELECT_VALUE, label: t('planning.openShift') }]);

// The select models an empty choice as null; this form spells it as the open-shift sentinel.
const assignedUserSelection = computed<string | null>({
  get: () => form.assignedUserId,
  set: (next) => {
    form.assignedUserId = next ?? OPEN_SHIFT_SELECT_VALUE;
  },
});

const baseStatusOptions = computed(() => [
  { label: t('planning.status.planned'), value: 'Planned' },
  { label: t('planning.status.completed'), value: 'Completed' },
  { label: t('planning.status.cancelled'), value: 'Cancelled' },
]);

// 'Confirmed' is only reached via the Confirm action, not via this free list.
// If a record is already confirmed, the option stays visible so the current status is correct.
const statusOptions = computed(() =>
  form.status === 'Confirmed' ?
    [{ label: t('planning.status.confirmed'), value: 'Confirmed' }, ...baseStatusOptions.value] :
    baseStatusOptions.value,
);

const isSaving = ref(false);
const errorMessage = ref<string | null>(null);

const assignedEmployeeId = computed(() =>
  form.assignedUserId === OPEN_SHIFT_SELECT_VALUE ? null : form.assignedUserId || null,
);

const { data: availabilityRules } = useAvailabilityRules({
  employeeId: assignedEmployeeId,
});

const { checkAssignment } = useAvailabilityWarning(users, {
  rules: computed(() => availabilityRules.value?.items ?? []),
});

const assignmentWarning = computed(() => {
  if (!assignedEmployeeId.value || !form.startUtc || !form.endUtc) {
    return null;
  }
  const result = checkAssignment(assignedEmployeeId.value, form.startUtc, form.endUtc);
  return result.hasConflict ? result.message : null;
});

const rangeStart = computed({
  get: () => new Date(form.startUtc),
  set: (value: Date) => {
 form.startUtc = value.toISOString();
},
});

const rangeEnd = computed({
  get: () => new Date(form.endUtc),
  set: (value: Date) => {
 form.endUtc = value.toISOString();
},
});

function clearErrors() {
  errorMessage.value = null;
  planningForm.value?.clear();
}

watch([selectedRecord, isCreateMode], () => {
  clearErrors();
});

function toRequest(data: PlanningRecordSchema) {
  return {
    title: data.title,
    description: data.description || null,
    notes: data.notes || null,
    assignedUserId: data.assignedUserId === OPEN_SHIFT_SELECT_VALUE ? null : data.assignedUserId || null,
    customerId: data.customerId,
    status: data.status,
    color: data.color,
    startUtc: data.startUtc,
    endUtc: data.endUtc,
  };
}

function applyError(error: unknown) {
  const { message, validationErrors } = useApiError(error);
  if (validationErrors.value) {
    planningForm.value?.setErrors(toFormErrors(validationErrors.value));
    return;
  }
  errorMessage.value = message.value;
}

async function onSubmit(event: FormSubmitEvent<PlanningRecordSchema>) {
  clearErrors();
  isSaving.value = true;
  try {
    if (isCreateMode.value) {
      await createRecord(toRequest(event.data));
      store.closeSidebar();
      return;
    }

    if (!selectedRecord.value) {
      return;
    }
    await api.update(selectedRecord.value.id, toRequest(event.data));
    api.invalidatePlanning();
    toast.add({ title: t('planning.saved'), color: 'success' });
  } catch (error) {
    applyError(error);
  } finally {
    isSaving.value = false;
  }
}

async function onDelete() {
  if (!selectedRecord.value) {
    return;
  }
  await deleteRecord(selectedRecord.value.id);
}

async function onDuplicate() {
  if (!selectedRecord.value) {
    return;
  }
  await duplicateRecord(selectedRecord.value.id);
}

const confirmMutation = api.useConfirmMutation();

async function onConfirm() {
  if (!selectedRecord.value) {
    return;
  }
  try {
    await confirmMutation.mutateAsync(selectedRecord.value.id);
    toast.add({ title: t('planning.bookingConfirmed'), color: 'success' });
  } catch (error) {
    const { message } = useApiError(error);
    errorMessage.value = message.value;
  }
}
</script>

<template>
	<USlideover
		v-model:open="store.sidebarOpen"
		:close="false"
		:ui="{ width: 'max-w-md' }"
		@update:open="(open: boolean) => !open && store.closeSidebar()"
	>
		<template #header="{ close }">
			<div class="flex w-full items-center">
				<div class="flex min-w-0 flex-1 items-center gap-3">
					<PlanningSidebarColor v-model:color="form.color" />
					<USeparator
						orientation="vertical"
						class="h-5 max-h-full"
					/>
					<h2 class="truncate text-lg font-medium">
						{{ isCreateMode ? t('planning.sidebar.createTitle') : t('planning.sidebar.detailsTitle') }}
					</h2>
				</div>
				<UButton
					icon="i-lucide-x"
					color="neutral"
					variant="ghost"
					:aria-label="t('common.actions.close')"
					@click="close()"
				/>
			</div>
		</template>
		<template #body>
			<UForm
				v-if="showForm"
				id="planning-record-form"
				ref="planningForm"
				:schema="schema"
				:state="form"
				:validate-on="['blur', 'change']"
				class="space-y-4"
				@submit="onSubmit"
			>
				<div
					v-if="selectedRecord && !isCreateMode"
					class="flex items-center gap-2"
				>
					<PlanningCardsStatusBadge :status="selectedRecord.status" />
					<UBadge
						v-if="selectedRecord.hasOverlap"
						color="error"
						variant="subtle"
						size="sm"
					>
						{{ t('planning.overlap') }}
					</UBadge>
				</div>

				<UAlert
					v-if="errorMessage"
					color="error"
					variant="subtle"
					:title="errorMessage"
				/>

				<UAlert
					v-if="assignmentWarning"
					color="warning"
					variant="subtle"
					:title="assignmentWarning"
				/>

				<UFormField
					:label="t('planning.fields.title')"
					name="title"
					required
				>
					<UInput
						ref="titleInput"
						v-model="form.title"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					:label="t('planning.fields.employee')"
					name="assignedUserId"
				>
					<UiEntitySelect
						v-model:value="assignedUserSelection"
						kind="user"
						:disabled="!canManage"
						:restrict-to="userRestriction"
						:pinned="openShiftOption"
					/>
				</UFormField>

				<UFormField
					:label="t('planning.fields.customer')"
					name="customerId"
				>
					<UiEntitySelect
						v-model:value="form.customerId"
						kind="customer"
						:disabled="!canManage"
						:restrict-to="customerRestriction"
						clearable
					/>
				</UFormField>

				<UFormField
					:label="t('planning.fields.period')"
					name="endUtc"
				>
					<ControlsDateTimeRangePicker
						v-model:start="rangeStart"
						v-model:end="rangeEnd"
						:disabled="!canManage"
					>
						<template #startPresets="{ current, apply }">
							<PlanningSidebarImportantTimes
								:current="current"
								:disabled="!canManage"
								@select="apply"
							/>
						</template>
						<template #endPresets="{ current, apply }">
							<PlanningSidebarImportantTimes
								:current="current"
								:disabled="!canManage"
								@select="apply"
							/>
						</template>
					</ControlsDateTimeRangePicker>
				</UFormField>

				<UFormField
					:label="t('users.columns.status')"
					name="status"
				>
					<USelect
						v-model="form.status"
						:items="statusOptions"
						value-key="value"
						label-key="label"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					:label="t('planning.fields.description')"
					name="description"
				>
					<UTextarea
						v-model="form.description"
						:rows="2"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					:label="t('planning.fields.notes')"
					name="notes"
				>
					<UTextarea
						v-model="form.notes"
						:rows="4"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>
			</UForm>
		</template>
		<template
			v-if="canManage && showForm"
			#footer
		>
			<div class="flex w-full items-center gap-2">
				<template v-if="!isCreateMode">
					<UTooltip :text="t('planning.duplicate')">
						<UButton
							icon="i-lucide-copy"
							color="neutral"
							variant="ghost"
							:aria-label="t('planning.duplicate')"
							@click="() => { onDuplicate() }"
						/>
					</UTooltip>
					<UTooltip :text="t('common.actions.delete')">
						<UButton
							icon="i-lucide-trash-2"
							color="error"
							variant="ghost"
							:aria-label="t('common.actions.delete')"
							@click="() => { onDelete() }"
						/>
					</UTooltip>
				</template>

				<div class="ms-auto flex items-center gap-2">
					<UButton
						v-if="!isCreateMode && selectedRecord?.status === 'Planned'"
						icon="i-lucide-check"
						color="success"
						variant="soft"
						:label="t('common.actions.confirm')"
						:loading="confirmMutation.isPending.value"
						@click="() => { onConfirm() }"
					/>
					<UButton
						type="submit"
						form="planning-record-form"
						:label="isCreateMode ? t('common.actions.create') : t('common.actions.save')"
						:loading="isSaving"
					/>
				</div>
			</div>
		</template>
	</USlideover>
</template>
