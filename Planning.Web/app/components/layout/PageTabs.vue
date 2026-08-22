<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui';

/**
 * The tab row that sits flush under the page header (layout `#tabs` slot). Triggers only —
 * `:content="false"` — because each tab's body is a different part of the page, not a panel.
 */
defineProps<{
	items: TabsItem[]
}>();

const active = defineModel<string>({ required: true });
</script>

<template>
	<UTabs
		v-model="active"
		:items="items"
		:content="false"
		variant="link"
		color="brand"
		:ui="{ list: 'overflow-visible border-b-0', trigger: 'px-3' }"
	/>
</template>
