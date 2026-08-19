<script setup lang="ts">
import { applyImportantWorkTime } from '~/utils/planning/planningSettings';
import type { ImportantWorkTimeResponse } from '~/generated/models';

withDefaults(defineProps<{
  disabled?: boolean
}>(), {
  disabled: false,
});

const start = defineModel<Date>('start', { required: true });
const end = defineModel<Date>('end', { required: true });

const { data: organization } = useCurrentOrganization();

const entries = computed(() => organization.value?.importantWorkTimes ?? []);

function chipLabel(entry: ImportantWorkTimeResponse): string {
  return entry.label || entry.startTime;
}

function apply(entry: ImportantWorkTimeResponse) {
  const { start: newStart, end: newEnd } = applyImportantWorkTime(
    { startTime: entry.startTime },
    start.value,
    end.value,
  );
  start.value = newStart;
  end.value = newEnd;
}
</script>

<template>
	<div
		v-if="entries.length > 0"
		class="flex flex-wrap gap-1.5"
	>
		<UButton
			v-for="(entry, index) in entries"
			:key="`${entry.label}-${entry.startTime}-${index}`"
			:label="chipLabel(entry)"
			variant="soft"
			color="neutral"
			size="xs"
			:disabled="disabled"
			@click="apply(entry)"
		/>
	</div>
</template>
