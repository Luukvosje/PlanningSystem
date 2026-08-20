<script setup lang="ts">
/**
 * A block within a page: heading plus content, separated from its neighbours by the page's
 * `gap-6` rhythm rather than by a border. The page shell is already a bordered panel, so a
 * card here would be a box inside a box.
 *
 * `LayoutCard` is for things that genuinely float on their own — auth screens, dashboard tiles.
 */
defineProps<{
	title?: string
	description?: string
}>();
</script>

<template>
	<section class="flex min-w-0 flex-col gap-4">
		<LayoutSectionHeader
			:title="title"
			:description="description"
		>
			<template
				v-if="$slots.actions"
				#actions
			>
				<slot name="actions" />
			</template>
		</LayoutSectionHeader>

		<slot />
	</section>
</template>
