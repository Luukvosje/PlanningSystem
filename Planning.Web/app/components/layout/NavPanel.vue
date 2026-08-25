<script setup lang="ts">
/**
 * The navigation panel next to the icon rail: the name of the module you are in plus its pages.
 * Rendered inline in the sidebar when the panel stays open, and as a floating panel over the page
 * while you hover the rail in automatic mode.
 */
const emit = defineEmits<{
  controlActive: [active: boolean]
}>();

withDefaults(defineProps<{
  label?: string
  items: Array<{ label: string, to: string }>
}>(), {
  label: undefined,
});
</script>

<template>
	<div class="flex h-full w-48 min-h-0 flex-col bg-default">
		<div class="flex min-h-(--ui-header-height) shrink-0 items-center px-4">
			<p class="truncate text-xs font-semibold tracking-wider text-muted uppercase">
				{{ label }}
			</p>
		</div>

		<div class="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto px-2">
			<UNavigationMenu
				:items="items"
				orientation="vertical"
				class="w-full"
				:ui="{ link: 'px-2.5 py-2 overflow-hidden', separator: 'hidden' }"
			/>
		</div>

		<div class="flex shrink-0 flex-col gap-3 p-4">
			<ControlsColorModeSwitch />
			<ControlsOrganizationSwitch @active-change="(active: boolean) => emit('controlActive', active)" />
		</div>
	</div>
</template>
