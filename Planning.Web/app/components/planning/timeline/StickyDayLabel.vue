<script setup lang="ts">
defineProps<{
  label: string
  isWeekend?: boolean
  variant?: 'header' | 'block'
}>()

const { rowLabelWidth } = useTimeline()
</script>

<template>
  <div
    class="sticky z-25 w-max whitespace-nowrap pointer-events-none"
    :class="[
      variant === 'block'
        ? 'px-1.5 py-0.5 text-[10px] font-semibold text-white bg-black/45 backdrop-blur-sm rounded-r'
        : 'px-2 py-2 text-xs font-medium border-r border-default/50 bg-default/95 backdrop-blur',
      variant !== 'block' && isWeekend ? 'bg-muted/40' : '',
    ]"
    :style="{ left: `${rowLabelWidth}px` }"
  >
    {{ label }}
  </div>
</template>
