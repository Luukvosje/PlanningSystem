<script setup lang="ts">
/**
 * A block within a page: heading plus content, separated from its neighbours by the page's
 * `gap-6` rhythm rather than by a border. For read-only content — a list, a chart, an empty
 * state — where a border would only add a box inside the page's own panel.
 *
 * `LayoutCard` is for things that stand on their own: auth screens, dashboard tiles, and every
 * block you can type into — see `FormEditableSection`, which is the standard for entity fields.
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
