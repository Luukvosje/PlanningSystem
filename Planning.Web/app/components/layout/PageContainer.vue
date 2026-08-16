<script setup lang="ts">
withDefaults(defineProps<{
  /** Fill available height without page scroll (e.g. full-height tables). */
  fill?: boolean
}>(), {
  fill: false,
});
</script>

<template>
	<div
		class="mx-auto flex h-full min-h-0 w-full flex-col gap-6 p-0.5"
		:class="fill ? 'overflow-hidden' : 'overflow-y-auto'"
	>
		<slot />
	</div>
</template>
