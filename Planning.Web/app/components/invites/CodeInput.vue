<script setup lang="ts">
const model = defineModel<string>({ required: true });
const { t } = useI18n();
</script>

<template>
	<UInput
		:model-value="model"
		:placeholder="t('invites.codePlaceholder')"
		class="w-full uppercase font-mono tracking-widest"
		@update:model-value="model = ($event ?? '').toUpperCase()"
	/>
</template>
