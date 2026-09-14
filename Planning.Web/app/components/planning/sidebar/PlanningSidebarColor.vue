<script setup lang="ts">
const color = defineModel<string>('color', { required: true });

const { t } = useI18n();
const { canManage } = usePlanning();
</script>

<template>
	<UiColorPicker
		v-model="color"
		:disabled="!canManage"
	>
		<template #trigger>
			<UTooltip
				:title="t('common.color.choose')"
			>
				<UButton
					variant="ghost"
					color="neutral"
					class="group size-7 rounded-sm transition-transform hover:scale-110 p-0 m-0 flex items-center justify-center"
					:style="{ backgroundColor: color }"
					:disabled="!canManage"
					:ui="{ leadingIcon: 'group-hover:block hidden bg-default' }"
					leading-icon="i-lucide-palette"
				/>
			</UTooltip>
		</template>
	</UiColorPicker>
</template>
