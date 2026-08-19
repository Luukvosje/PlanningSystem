<script setup lang="ts">
/**
 * The bar above a page's content: context on the left, page actions on the right.
 *
 * It deliberately renders no heading. The app chrome already shows the page title next to its
 * icon (see layouts/default.vue and useAppNavigation), and repeating it here meant every admin
 * screen carried the same string twice, in two sizes, ~24px apart.
 *
 * The default slot is the left-hand side - a back link on detail pages, for instance. Without
 * it the `subtitle` fills that space.
 */
defineProps<{
	subtitle?: string
}>();
</script>

<template>
	<div
		v-if="subtitle || $slots.default || $slots.actions"
		class="flex items-center justify-between gap-4"
	>
		<div class="flex min-w-0 items-center gap-3">
			<slot />
			<p
				v-if="subtitle"
				class="min-w-0 truncate text-sm text-muted"
			>
				{{ subtitle }}
			</p>
		</div>

		<div
			v-if="$slots.actions"
			class="flex shrink-0 items-center gap-2"
		>
			<slot name="actions" />
		</div>
	</div>
</template>
