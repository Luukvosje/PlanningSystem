<script setup lang="ts">
const props = withDefaults(defineProps<{
  label?: string | null
  size?: 'sm' | 'md' | 'lg'
}>(), {
  label: 'Laden...',
  size: 'md',
})

const iconClass = computed(() => ({
  sm: 'size-5',
  md: 'size-8',
  lg: 'size-10',
}[props.size]))

const wrapperClass = computed(() => ({
  sm: 'gap-2 py-4',
  md: 'gap-3 py-12',
  lg: 'gap-4 py-16',
}[props.size]))
</script>

<template>
  <div
    class="flex flex-col items-center justify-center"
    :class="wrapperClass"
    role="status"
    aria-live="polite"
  >
    <UIcon
      name="i-lucide-loader-circle"
      :class="[iconClass, 'text-primary animate-spin']"
    />
    <p v-if="label" class="text-sm text-muted">
      {{ label }}
    </p>
  </div>
</template>
