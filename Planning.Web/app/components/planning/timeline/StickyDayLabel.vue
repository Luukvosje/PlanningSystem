<script setup lang="ts">
defineProps<{
  weekday: string
  day: string
  isWeekend?: boolean
  variant?: 'header' | 'block'
}>()

const { rowLabelWidth } = useTimeline()
</script>

<template>
  <div
    class="sticky z-25 w-max pointer-events-none"
    :class="[
      variant === 'block'
        ? 'px-1.5 py-0.5 text-[10px] font-semibold text-white bg-black/45 backdrop-blur-sm rounded-r whitespace-nowrap'
        : 'px-2 py-1.5 text-xs font-medium border-r border-default/50 bg-default/95 backdrop-blur flex flex-col items-center leading-none gap-0.5',
      variant !== 'block' && isWeekend ? 'bg-muted/40' : '',
    ]"
    :style="{ left: `${rowLabelWidth}px` }"
  >
    <span class="uppercase">{{ weekday }}</span>
    <span class="font-semibold">{{ day }}</span>
  </div>
</template>
