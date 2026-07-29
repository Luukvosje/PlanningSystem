<script setup lang="ts">
import type { PlanningRowLayout } from '~/utils/planning/boardPreferences'

const model = defineModel<PlanningRowLayout>()

const options: { value: PlanningRowLayout; label: string; icon: string }[] = [
	{ value: 'compact', label: 'Compact', icon: 'i-lucide-align-justify' },
	{ value: 'spacious', label: 'Ruim', icon: 'i-lucide-layout-list' },
]
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
			:color="model === option.value ? 'secondary' : 'neutral'"
			@click="model = option.value"
		/>
	</UFieldGroup>
</template>
