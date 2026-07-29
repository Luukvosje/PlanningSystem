<script setup lang="ts">
withDefaults(defineProps<{
  label?: string
}>(), {
  label: 'Planning laden...',
})

const rows = [
  {
    labelWidth: '4.5rem',
    blocks: [
      { left: '8%', width: '14%', delay: '0ms' },
      { left: '38%', width: '18%', delay: '120ms' },
      { left: '72%', width: '12%', delay: '240ms' },
    ],
  },
  {
    labelWidth: '5.5rem',
    blocks: [
      { left: '18%', width: '22%', delay: '80ms' },
      { left: '55%', width: '16%', delay: '200ms' },
    ],
  },
  {
    labelWidth: '3.75rem',
    blocks: [
      { left: '5%', width: '11%', delay: '40ms' },
      { left: '28%', width: '20%', delay: '160ms' },
      { left: '62%', width: '24%', delay: '280ms' },
    ],
  },
  {
    labelWidth: '6rem',
    blocks: [
      { left: '42%', width: '15%', delay: '100ms' },
      { left: '78%', width: '10%', delay: '220ms' },
    ],
  },
  {
    labelWidth: '4rem',
    blocks: [
      { left: '12%', width: '17%', delay: '60ms' },
      { left: '48%', width: '21%', delay: '180ms' },
    ],
  },
]
</script>

<template>
  <div
    class="relative flex h-full min-h-72 flex-col overflow-hidden rounded-xl border border-default bg-default shadow-sm"
    role="status"
    aria-live="polite"
  >
    <div class="flex shrink-0 border-b border-default">
      <div class="w-40 shrink-0 border-r border-default px-3 py-3">
        <div class="h-3 w-16 rounded bg-muted animate-pulse" />
      </div>
      <div class="flex min-w-0 flex-1">
        <div
          v-for="day in 7"
          :key="day"
          class="min-w-0 flex-1 border-r border-default px-2 py-2 last:border-r-0"
        >
          <div
            class="mb-1 h-2.5 w-10 rounded bg-muted animate-pulse"
            :style="{ animationDelay: `${day * 40}ms` }"
          />
          <div
            class="h-2 w-6 rounded bg-muted/70 animate-pulse"
            :style="{ animationDelay: `${day * 40 + 20}ms` }"
          />
        </div>
      </div>
    </div>

    <div class="relative min-h-0 flex-1">
      <div
        v-for="(row, rowIndex) in rows"
        :key="rowIndex"
        class="flex h-14 border-b border-default last:border-b-0"
      >
        <div class="flex w-40 shrink-0 items-center border-r border-default px-3">
          <div
            class="h-3 rounded bg-muted animate-pulse"
            :style="{ width: row.labelWidth, animationDelay: `${rowIndex * 50}ms` }"
          />
        </div>
        <div class="relative min-w-0 flex-1">
          <div class="pointer-events-none absolute inset-0 flex">
            <div
              v-for="day in 7"
              :key="day"
              class="min-w-0 flex-1 border-r border-default/40 last:border-r-0"
            />
          </div>
          <div
            v-for="(block, blockIndex) in row.blocks"
            :key="blockIndex"
            class="absolute top-2.5 bottom-2.5 rounded-md bg-primary/25 animate-pulse"
            :style="{
              left: block.left,
              width: block.width,
              animationDelay: block.delay,
            }"
          />
        </div>
      </div>

      <div class="absolute inset-0 flex items-center justify-center bg-default/50 backdrop-blur-[1px]">
        <div class="flex flex-col items-center gap-3 rounded-xl border border-default bg-default/90 px-6 py-5 shadow-sm">
          <UIcon name="i-lucide-loader-circle" class="size-8 text-primary animate-spin" />
          <p class="text-sm text-muted">
            {{ label }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
