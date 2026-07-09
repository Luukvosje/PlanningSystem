<script setup lang="ts">
const model = defineModel<string>({ required: true })
</script>

<template>
  <UInput
    :model-value="model"
    placeholder="Bijv. AB12CD34"
    class="w-full uppercase font-mono tracking-widest"
    @update:model-value="model = ($event ?? '').toUpperCase()"
  />
</template>
