<script setup lang="ts">
import { normalizeTimeValue } from '~/utils/planning/planningSettings';
import type { ImportantWorkTimeResponse } from '~/generated/models';

const props = withDefaults(defineProps<{
  /** Currently selected time (`HH:mm`), used to mark the matching quick pick. */
  current: string
  disabled?: boolean
}>(), {
  disabled: false,
});

const emit = defineEmits<{
  select: [time: string]
}>();

const { data: organization } = useCurrentOrganization();

const entries = computed(() => organization.value?.importantWorkTimes ?? []);

function chipLabel(entry: ImportantWorkTimeResponse): string {
  return entry.label || normalizeTimeValue(entry.startTime);
}

function isActive(entry: ImportantWorkTimeResponse): boolean {
  return normalizeTimeValue(entry.startTime) === props.current;
}
</script>

<template>
	<div
		v-if="entries.length > 0"
		class="flex flex-wrap items-center gap-1"
	>
		<UButton
			v-for="(entry, index) in entries"
			:key="`${entry.label}-${entry.startTime}-${index}`"
			:label="chipLabel(entry)"
			:variant="isActive(entry) ? 'solid' : 'soft'"
			:color="isActive(entry) ? 'brand' : 'neutral'"
			size="xs"
			:disabled="disabled"
			@click="emit('select', normalizeTimeValue(entry.startTime))"
		/>
	</div>
</template>
