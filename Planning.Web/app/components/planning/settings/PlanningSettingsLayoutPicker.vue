<script setup lang="ts">
import type { PlanningRowLayout } from '~/utils/planning/boardPreferences';

const model = defineModel<PlanningRowLayout>();
const { t } = useI18n();

const options = computed<{ value: PlanningRowLayout, label: string, icon: string }[]>(() => [
	{ value: 'compact', label: t('planning.settings.layoutCompact'), icon: 'i-lucide-align-justify' },
	{ value: 'spacious', label: t('planning.settings.layoutSpacious'), icon: 'i-lucide-layout-list' },
]);
</script>

<template>
	<UFieldGroup class="w-full">
		<UButton
			v-for="option in options"
			:key="option.value"
			class="flex-1 justify-start"
			:icon="option.icon"
			:label="option.label"
			:variant="model === option.value ? 'solid' : 'outline'"
			:color="model === option.value ? 'brand' : 'neutral'"
			@click="model = option.value"
		/>
	</UFieldGroup>
</template>
