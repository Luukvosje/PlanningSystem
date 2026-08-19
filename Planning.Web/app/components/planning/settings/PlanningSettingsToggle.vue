<script setup lang="ts">
defineProps<{
	label: string
	description?: string
}>()

const model = defineModel<boolean>()
</script>

<template>
	<div class="flex items-center justify-between px-2 py-1.5 rounded hover:bg-elevated cursor-pointer" @click="model = !model">
		<div class="flex flex-col">
			<span class="text-sm">{{ label }}</span>
			<span v-if="description" class="text-xs text-muted">{{ description }}</span>
		</div>
		<USwitch
			v-model="model"
			size="sm"
			color="brand"
			@click.stop
		/>
	</div>
</template>
