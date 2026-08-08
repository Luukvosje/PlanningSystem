<script setup lang="ts">
withDefaults(defineProps<{
	compact?: boolean
}>(), {
	compact: false,
});

const colorMode = useColorMode();

function setMode(mode: 'light' | 'dark') {
	colorMode.preference = mode;
}

const isLight = computed(() => colorMode.value === 'light');
</script>

<template>
	<div
		class="flex items-center rounded-lg border border-default bg-elevated/50 p-0.5"
		:class="compact ? 'w-auto' : 'w-full'"
		role="group"
		aria-label="Kleurmodus"
	>
		<UButton
			:variant="isLight ? 'soft' : 'ghost'"
			:color="isLight ? 'secondary' : 'neutral'"
			size="xs"
			icon="i-lucide-sun"
			:label="compact ? undefined : 'Licht'"
			:square="compact"
			class="flex-1 justify-center"
			aria-label="Lichte modus"
			:aria-pressed="isLight"
			@click="() => { setMode('light') }"
		/>
		<UButton
			:variant="!isLight ? 'soft' : 'ghost'"
			:color="!isLight ? 'secondary' : 'neutral'"
			size="xs"
			icon="i-lucide-moon"
			:label="compact ? undefined : 'Donker'"
			:square="compact"
			class="flex-1 justify-center"
			aria-label="Donkere modus"
			:aria-pressed="!isLight"
			@click="() => { setMode('dark') }"
		/>
	</div>
</template>
