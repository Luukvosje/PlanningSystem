<script setup lang="ts">
import type { UnavailableOverlay } from '~/utils/planning/availabilityMath';
import { UNAVAILABLE_OVERLAY_STYLE } from '~/utils/planning/dateUtils';

defineProps<{
  overlays: UnavailableOverlay[]
}>();
</script>

<template>
	<div
		v-for="(overlay, index) in overlays"
		:key="`${overlay.leftPx}-${index}`"
		class="absolute top-1 bottom-1 z-[5] rounded-sm pointer-events-none border border-default/40"
		:style="{
			left: `${overlay.leftPx}px`,
			width: `${overlay.widthPx}px`,
			...UNAVAILABLE_OVERLAY_STYLE,
		}"
		:title="overlay.tooltip"
	/>
</template>
