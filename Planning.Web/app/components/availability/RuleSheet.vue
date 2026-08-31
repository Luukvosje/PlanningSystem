<script setup lang="ts">
import type { AvailabilityRule, AvailabilityRuleType, Weekday } from '~/types/availability';
import { WHOLE_DAY_END, WHOLE_DAY_START, canManageAvailabilityRule, getWeekdayOptions } from '~/types/availability';

const props = defineProps<{
  employeeId: string
  type: AvailabilityRuleType
  rule?: AvailabilityRule | null
  defaultDate?: string
}>();

const emit = defineEmits<{
  saved: []
  close: []
}>();

const open = defineModel<boolean>('open', { default: false });

const { t } = useI18n();
const auth = useAuthStore();
const weekdayOptions = computed(() => getWeekdayOptions(t));
const api = useAvailabilityApi();
const isSaving = computed(() => api.create.isPending.value || api.update.isPending.value);
const isDeleting = computed(() => api.remove.isPending.value);

/** Nothing to delete on a new rule, and only your own absence is yours to remove. */
const canDelete = computed(() =>
  !!props.rule && canManageAvailabilityRule(props.rule, auth.currentUser));

const confirmDeleteOpen = ref(false);

const deleteTitle = computed(() =>
  props.type === 'Weekly' ?
    t('availability.deleteWeeklyRule') :
    t('availability.deleteException'));

const weekday = ref<Weekday>('Monday');
const date = ref('');
const timePreset = ref<'whole_day' | 'until' | 'after' | 'custom'>('whole_day');
const customStart = ref('09:00');
const customEnd = ref('17:00');
const boundaryTime = ref('18:00');
const reason = ref('');

const timePresetOptions = computed(() => [
  { label: t('availability.timePreset.wholeDay'), value: 'whole_day' },
  { label: t('availability.timePreset.until'), value: 'until' },
  { label: t('availability.timePreset.after'), value: 'after' },
  { label: t('availability.timePreset.custom'), value: 'custom' },
]);

const title = computed(() => {
  if (props.rule) {
    return props.type === 'Weekly' ? t('availability.editWeeklyRule') : t('availability.editException');
  }
  return props.type === 'Weekly' ? t('availability.newWeeklyRule') : t('availability.newException');
});

function toTimeValue(value: string): string {
  return value.length === 5 ? `${value}:00` : value;
}

function resolveTimes(): { startTime: string, endTime: string } {
  switch (timePreset.value) {
    case 'whole_day':
      return { startTime: WHOLE_DAY_START, endTime: WHOLE_DAY_END };
    case 'until':
      return { startTime: WHOLE_DAY_START, endTime: toTimeValue(boundaryTime.value) };
    case 'after':
      return { startTime: toTimeValue(boundaryTime.value), endTime: WHOLE_DAY_END };
    case 'custom':
      return { startTime: toTimeValue(customStart.value), endTime: toTimeValue(customEnd.value) };
  }
}

function fromTimes(startTime: string, endTime: string) {
  const start = startTime.slice(0, 5);
  const end = endTime.slice(0, 5);

  if (start === '00:00' && (end === '23:59' || end === '24:00')) {
    timePreset.value = 'whole_day';
    return;
  }
  if (start === '00:00') {
    timePreset.value = 'until';
    boundaryTime.value = end;
    return;
  }
  if (end === '23:59' || end === '24:00') {
    timePreset.value = 'after';
    boundaryTime.value = start;
    return;
  }

  timePreset.value = 'custom';
  customStart.value = start;
  customEnd.value = end;
}

function resetForm() {
  weekday.value = 'Monday';
  date.value = '';
  timePreset.value = 'whole_day';
  customStart.value = '09:00';
  customEnd.value = '17:00';
  boundaryTime.value = '18:00';
  reason.value = '';

  if (props.rule) {
    weekday.value = props.rule.weekday ?? 'Monday';
    date.value = props.rule.date ?? '';
    reason.value = props.rule.reason ?? '';
    fromTimes(props.rule.startTime, props.rule.endTime);
  } else if (props.type === 'OneTime') {
    if (props.defaultDate) {
      date.value = props.defaultDate;
    } else {
      const today = new Date();
      date.value = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
    }
  }
}

watch(open, (isOpen) => {
  if (isOpen) {
    resetForm();
  } else {
    confirmDeleteOpen.value = false;
  }
});

watch(() => props.rule, () => {
  if (open.value) {
    resetForm();
  }
});

async function save() {
  const { startTime, endTime } = resolveTimes();

  if (props.rule) {
    await api.update.mutateAsync({
      id: props.rule.id,
      request: {
        weekday: props.type === 'Weekly' ? weekday.value : undefined,
        date: props.type === 'OneTime' ? date.value : null,
        startTime,
        endTime,
        status: 'Unavailable',
        reason: reason.value.trim() || null,
      },
    });
  } else {
    await api.create.mutateAsync({
      employeeId: props.employeeId,
      type: props.type,
      weekday: props.type === 'Weekly' ? weekday.value : undefined,
      date: props.type === 'OneTime' ? date.value : null,
      startTime,
      endTime,
      status: 'Unavailable',
      reason: reason.value.trim() || null,
    });
  }

  emit('saved');
  open.value = false;
}

async function remove() {
  if (!props.rule) {
    return;
  }

  confirmDeleteOpen.value = false;
  await api.remove.mutateAsync(props.rule.id);
  open.value = false;
}
</script>

<template>
	<USlideover
		v-model:open="open"
		:title="title"
		:ui="{ width: 'max-w-md' }"
		@update:open="(value) => !value && emit('close')"
	>
		<template #body>
			<div class="space-y-4">
				<UFormField
					v-if="type === 'Weekly'"
					:label="t('availability.day')"
					required
				>
					<USelect
						v-model="weekday"
						:items="weekdayOptions"
						value-key="value"
						label-key="label"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					v-else
					:label="t('availability.date')"
					required
				>
					<ControlsDateSelectInput
						v-model="date"
						:disabled="isSaving"
					/>
				</UFormField>

				<UFormField
					:label="t('planning.fields.period')"
					required
				>
					<USelect
						v-model="timePreset"
						:items="timePresetOptions"
						value-key="value"
						label-key="label"
						class="w-full"
					/>
				</UFormField>

				<UFormField
					v-if="timePreset === 'until' || timePreset === 'after'"
					:label="timePreset === 'until' ? t('availability.until') : t('availability.from')"
					required
				>
					<UInput
						v-model="boundaryTime"
						type="time"
						class="w-full"
					/>
				</UFormField>

				<div
					v-if="timePreset === 'custom'"
					class="grid grid-cols-2 gap-3"
				>
					<UFormField
						:label="t('availability.start')"
						required
					>
						<UInput
							v-model="customStart"
							type="time"
							class="w-full"
						/>
					</UFormField>
					<UFormField
						:label="t('availability.end')"
						required
					>
						<UInput
							v-model="customEnd"
							type="time"
							class="w-full"
						/>
					</UFormField>
				</div>

				<UFormField :label="t('availability.reasonOptional')">
					<UInput
						v-model="reason"
						:placeholder="t('availability.reasonPlaceholder')"
						class="w-full"
					/>
				</UFormField>

				<div class="flex gap-2">
					<UButton
						v-if="canDelete"
						color="error"
						variant="outline"
						icon="i-lucide-trash-2"
						:label="t('common.actions.delete')"
						:loading="isDeleting"
						:disabled="isSaving"
						@click="confirmDeleteOpen = true"
					/>
					<UButton
						block
						class="flex-1"
						:label="t('common.actions.save')"
						:loading="isSaving"
						:disabled="isDeleting"
						@click="save"
					/>
				</div>
			</div>
		</template>
	</USlideover>

	<UiConfirmModal
		v-model:open="confirmDeleteOpen"
		:title="deleteTitle"
		:description="t('availability.deleteConfirmDescription')"
		:confirm-label="t('common.actions.delete')"
		:loading="isDeleting"
		@confirm="remove"
	/>
</template>
