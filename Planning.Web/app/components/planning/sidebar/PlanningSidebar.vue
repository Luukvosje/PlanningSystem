<script setup lang="ts">
import type { PlanningFormData } from '~/types/planning';

const store = usePlanningStore();
const { canManage, selectedRecord, createRecord, deleteRecord, duplicateRecord } = usePlanning();
const api = usePlanningApi();
const toast = useToast();

const { data: users } = useUsers();
const { data: customers } = useCustomers();

const form = reactive<PlanningFormData>({
  title: '',
  description: '',
  notes: '',
  assignedUserId: '',
  customerId: null,
  status: 'Planned',
  color: '#6366F1',
  startUtc: '',
  endUtc: '',
});

const isCreateMode = computed(() => store.sidebarMode === 'create');
const showForm = computed(() => isCreateMode.value || !!selectedRecord.value);
const titleInput = useTemplateRef<{ inputRef?: HTMLInputElement | null }>('titleInput');

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
  form.assignedUserId = record.assignedUserId;
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
  form.assignedUserId = draft.assignedUserId;
  form.customerId = draft.customerId ?? null;
  form.status = draft.status ?? store.filters.statuses[0] ?? 'Planned';
  form.color = '#6366F1';
  form.startUtc = draft.startUtc;
  form.endUtc = draft.endUtc;
}, { immediate: true });

const allStatusOptions = [
  { label: 'Gepland', value: 'Planned' as const },
  { label: 'Bevestigd', value: 'Confirmed' as const },
  { label: 'Afgerond', value: 'Completed' as const },
  { label: 'Geannuleerd', value: 'Cancelled' as const },
];

const userOptions = computed(() => {
  let list = users.value ?? [];
  if (isCreateMode.value && store.filters.userIds.length > 0) {
    list = list.filter((u) => u.id && store.filters.userIds.includes(u.id));
  }
  return list.map((u) => ({
    label: `${u.firstName} ${u.lastName}`.trim(),
    value: u.id!,
  }));
});

const customerOptions = computed(() => {
  let list = customers.value ?? [];
  if (isCreateMode.value && store.filters.customerIds.length > 0) {
    list = list.filter((c) => c.id && store.filters.customerIds.includes(c.id));
  }
  return list.map((c) => ({
    label: c.name ?? 'Onbekend',
    value: c.id!,
  }));
});

const statusOptions = computed(() => {
  if (isCreateMode.value && store.filters.statuses.length > 0) {
    return allStatusOptions.filter((option) =>
      store.filters.statuses.includes(option.value),
    );
  }
  return allStatusOptions;
});

const isSaving = ref(false);
const errorMessage = ref<string | null>(null);
const fieldErrors = ref<Record<string, string[]>>({});

const { data: availabilityRules } = useAvailabilityRules({
  employeeId: computed(() => form.assignedUserId || null),
});

const { checkAssignment } = useAvailabilityWarning(users, {
  rules: computed(() => availabilityRules.value?.items ?? []),
});

const assignmentWarning = computed(() => {
  if (!form.assignedUserId || !form.startUtc || !form.endUtc) {
    return null;
  }
  const result = checkAssignment(form.assignedUserId, form.startUtc, form.endUtc);
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

const periodFieldError = computed(() =>
  fieldError('StartUtc') ?? fieldError('EndUtc') ?? undefined,
);

function fieldError(field: string) {
  return fieldErrors.value[field]?.[0];
}

function clearErrors() {
  errorMessage.value = null;
  fieldErrors.value = {};
}

watch([selectedRecord, isCreateMode], () => {
  clearErrors();
});

async function save() {
  clearErrors();
  isSaving.value = true;
  try {
    if (isCreateMode.value) {
      await createRecord({
        title: form.title,
        description: form.description || null,
        notes: form.notes || null,
        assignedUserId: form.assignedUserId,
        customerId: form.customerId,
        startUtc: form.startUtc,
        endUtc: form.endUtc,
        color: form.color,
        status: form.status,
      });
      store.closeSidebar();
      return;
    }

    if (!selectedRecord.value) {
return;
}
    await api.update(selectedRecord.value.id, {
      title: form.title,
      description: form.description || null,
      notes: form.notes || null,
      assignedUserId: form.assignedUserId,
      customerId: form.customerId,
      status: form.status,
      color: form.color,
      startUtc: form.startUtc,
      endUtc: form.endUtc,
    });
    api.invalidatePlanning();
    toast.add({ title: 'Opgeslagen', color: 'success' });
  } catch (error) {
    const { message, validationErrors } = useApiError(error);
    errorMessage.value = message.value;
    fieldErrors.value = validationErrors.value ?? {};
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
</script>

<template>
	<USlideover
		v-model:open="store.sidebarOpen"
		:ui="{ width: 'max-w-md' }"
		@update:open="(open: boolean) => !open && store.closeSidebar()"
	>
		<template #header>
			<div class="flex items-center gap-3">
				<PlanningSidebarColor v-model:color="form.color" />
				<USeparator
					orientation="vertical"
					class="h-5 max-h-full"
				/>
				<h2 class="text-lg font-medium">
					{{ isCreateMode ? 'Nieuwe planning' : 'Planning details' }}
				</h2>
			</div>
		</template>
		<template #body>
			<div
				v-if="showForm"
				class="space-y-4"
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
						Overlap
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
					label="Titel"
					name="title"
					required
					:error="fieldError('Title')"
				>
					<UInput
						ref="titleInput"
						v-model="form.title"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					label="Medewerker"
					name="assignedUserId"
					required
					:error="fieldError('AssignedUserId')"
				>
					<USelect
						v-model="form.assignedUserId"
						:items="userOptions"
						value-key="value"
						label-key="label"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					label="Klant"
					name="customerId"
					:error="fieldError('CustomerId')"
				>
					<USelect
						v-model="form.customerId"
						:items="customerOptions"
						value-key="value"
						label-key="label"
						:disabled="!canManage"
						class="w-full"
						clearable
					/>
				</UFormField>

				<UFormField
					label="Periode"
					name="startUtc"
					:error="periodFieldError"
				>
					<DateTimeRangePicker
						v-model:start="rangeStart"
						v-model:end="rangeEnd"
						:disabled="!canManage"
					/>
				</UFormField>

				<UFormField
					label="Status"
					name="status"
					:error="fieldError('Status')"
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
					label="Beschrijving"
					name="description"
					:error="fieldError('Description')"
				>
					<UTextarea
						v-model="form.description"
						:rows="2"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					label="Notities"
					name="notes"
					:error="fieldError('Notes')"
				>
					<UTextarea
						v-model="form.notes"
						:rows="4"
						:disabled="!canManage"
						class="w-full"
					/>
				</UFormField>

				<div
					v-if="canManage"
					class="flex flex-wrap gap-2 pt-2"
				>
					<UButton
						:label="isCreateMode ? 'Aanmaken' : 'Opslaan'"
						:loading="isSaving"
						@click="() => { save() }"
					/>
					<template v-if="!isCreateMode">
						<UButton
							variant="outline"
							icon="i-lucide-copy"
							label="Dupliceer"
							@click="() => { onDuplicate() }"
						/>
						<UButton
							variant="outline"
							color="error"
							icon="i-lucide-trash-2"
							label="Verwijder"
							@click="() => { onDelete() }"
						/>
					</template>
				</div>
			</div>
		</template>
	</USlideover>
</template>
