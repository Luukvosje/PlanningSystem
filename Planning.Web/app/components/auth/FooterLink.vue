<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router';

/**
 * The "already have an account? Log in" line under an auth card.
 *
 * The same four lines of markup were repeated on login, register, join and organizations/new,
 * which is how the link colour ended up inconsistent with the rest of the app.
 */
defineProps<{
	question: string
	linkLabel: string
	to: RouteLocationRaw
}>();
</script>

<template>
	<p class="text-center text-sm text-muted">
		{{ question }}
		<NuxtLink
			:to="to"
			class="font-medium text-brand"
		>
			{{ linkLabel }}
		</NuxtLink>
	</p>
</template>
