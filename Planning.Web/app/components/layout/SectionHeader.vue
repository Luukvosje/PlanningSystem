<script setup lang="ts">
/**
 * The heading of a section: title and description on the left, actions on the right.
 *
 * It is never the page's heading — the app chrome already shows that next to the page icon
 * (see layouts/default.vue and useAppNavigation), and repeating it meant every admin screen
 * carried the same string twice, in two sizes, ~24px apart.
 *
 * The default slot replaces `title` when the left side needs something richer than a string.
 */
defineProps<{
	title?: string
	description?: string
}>();
</script>

<template>
	<div
		v-if="title || description || $slots.default || $slots.actions"
		class="flex items-start justify-between gap-4"
	>
		<div class="flex min-w-0 flex-1 flex-col gap-0.5">
			<slot>
				<p
					v-if="title"
					class="min-w-0 truncate text-base font-semibold text-highlighted"
				>
					{{ title }}
				</p>
			</slot>
			<p
				v-if="description"
				class="min-w-0 text-sm text-muted"
				:class="title ? 'truncate' : 'max-w-3xl leading-relaxed'"
			>
				{{ description }}
			</p>
		</div>

		<div
			v-if="$slots.actions"
			class="flex shrink-0 items-center gap-2 pt-0.5"
		>
			<slot name="actions" />
		</div>
	</div>
</template>
