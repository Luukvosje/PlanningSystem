<script setup lang="ts">
import type { ImportantWorkTimeRow } from '~/utils/planning/planningSettings';
import { toTimeStrings } from '~/utils/planning/planningSettings';

withDefaults(defineProps<{
  disabled?: boolean
}>(), {
  disabled: false,
});

const model = defineModel<ImportantWorkTimeRow[]>({ required: true });
const { t } = useI18n();

const previewTimes = computed(() => toTimeStrings(model.value));

function addRow() {
  model.value.push({ label: '', startTime: '09:00' });
}

function removeRow(index: number) {
  model.value.splice(index, 1);
}
</script>

<template>
	<div class="space-y-3">
		<div class="space-y-2">
			<div
				v-for="(entry, index) in model"
				:key="index"
				class="flex items-center gap-2"
			>
				<UInput
					v-model="entry.label"
					:placeholder="t('organizations.planningSettings.importantTimeLabel')"
					:disabled="disabled"
					class="w-40"
				/>
				<UInput
					v-model="entry.startTime"
					type="time"
					:disabled="disabled"
					class="w-36"
				/>
				<UButton
					icon="i-lucide-trash-2"
					color="neutral"
					variant="ghost"
					size="sm"
					:disabled="disabled"
					@click="removeRow(index)"
				/>
			</div>
		</div>

		<UButton
			icon="i-lucide-plus"
			:label="t('organizations.planningSettings.addTime')"
			variant="outline"
			size="sm"
			:disabled="disabled"
			@click="addRow"
		/>

		<OrganizationsImportantWorkTimesPreview :important-work-times="previewTimes" />
	</div>
</template>
