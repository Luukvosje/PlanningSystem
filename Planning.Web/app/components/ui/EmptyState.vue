<script setup lang="ts">
/**
 * One shape for "there is nothing here yet".
 *
 * The app used to have three: a centred block with a button inside tables, a whole card with
 * centred text in the availability editor, and a bare paragraph in the weekly pattern card.
 */
withDefaults(defineProps<{
	title: string
	description?: string
	icon?: string
	/** Table cells need the full height; a card section does not. */
	fill?: boolean
}>(), {
	description: undefined,
	icon: undefined,
	fill: false,
});
</script>

<template>
	<div
		class="flex flex-col items-center justify-center gap-3 px-4 py-8 text-center"
		:class="fill ? 'h-full min-h-48' : undefined"
	>
		<UIcon
			v-if="icon"
			:name="icon"
			class="size-8 text-dimmed"
		/>

		<div class="flex flex-col gap-1">
			<p class="text-sm text-muted">
				{{ title }}
			</p>
			<p
				v-if="description"
				class="text-xs text-dimmed"
			>
				{{ description }}
			</p>
		</div>

		<slot name="action" />
	</div>
</template>
