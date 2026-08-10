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
    class="sticky z-25 pointer-events-none"
    :class="[
      variant === 'block'
        ? 'w-max px-1.5 py-0.5 text-[10px] font-semibold text-white bg-black/45 backdrop-blur-sm rounded-r whitespace-nowrap'
        : 'inset-y-0 flex w-full flex-col items-center justify-center gap-0.5 px-1 py-1.5 text-xs font-medium leading-none',
      variant !== 'block' && isWeekend ? 'bg-muted/40' : variant !== 'block' ? 'bg-default/95 backdrop-blur' : '',
    ]"
    :style="{ left: `${rowLabelWidth}px` }"
  >
    <span class="uppercase">{{ weekday }}</span>
    <span class="font-semibold">{{ day }}</span>
  </div>
</template>
