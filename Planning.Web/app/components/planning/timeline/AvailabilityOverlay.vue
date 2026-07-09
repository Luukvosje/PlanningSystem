<script setup lang="ts">
import type { UnavailableOverlay } from '~/utils/planning/availabilityMath'

defineProps<{
  overlays: UnavailableOverlay[]
}>()
</script>

<template>
  <div
    v-for="(overlay, index) in overlays"
    :key="`${overlay.leftPx}-${index}`"
    class="absolute top-1 bottom-1 z-[5] rounded-sm pointer-events-none border border-muted-foreground/20"
    :style="{
      left: `${overlay.leftPx}px`,
      width: `${overlay.widthPx}px`,
      background: 'repeating-linear-gradient(-45deg, transparent, transparent 4px, rgba(148, 163, 184, 0.35) 4px, rgba(148, 163, 184, 0.35) 8px)',
      backgroundColor: 'rgba(148, 163, 184, 0.15)',
    }"
    :title="overlay.tooltip"
  />
</template>
