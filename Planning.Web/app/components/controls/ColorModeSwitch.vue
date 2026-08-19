<script setup lang="ts">
withDefaults(defineProps<{
	compact?: boolean
}>(), {
	compact: false,
});

const colorMode = useColorMode();
const { t } = useI18n();

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
		:aria-label="t('common.colorMode.group')"
	>
		<UButton
			:variant="isLight ? 'soft' : 'ghost'"
			:color="isLight ? 'brand' : 'neutral'"
			size="xs"
			icon="i-lucide-sun"
			:label="compact ? undefined : t('common.colorMode.light')"
			:square="compact"
			class="flex-1 justify-center"
			:aria-label="t('common.colorMode.lightMode')"
			:aria-pressed="isLight"
			@click="() => { setMode('light') }"
		/>
		<UButton
			:variant="!isLight ? 'soft' : 'ghost'"
			:color="!isLight ? 'brand' : 'neutral'"
			size="xs"
			icon="i-lucide-moon"
			:label="compact ? undefined : t('common.colorMode.dark')"
			:square="compact"
			class="flex-1 justify-center"
			:aria-label="t('common.colorMode.darkMode')"
			:aria-pressed="!isLight"
			@click="() => { setMode('dark') }"
		/>
	</div>
</template>
