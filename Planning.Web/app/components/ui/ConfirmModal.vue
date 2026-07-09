<script setup lang="ts">
defineProps<{
  title: string
  description?: string
  confirmLabel?: string
  loading?: boolean
}>()

const open = defineModel<boolean>('open', { required: true })

const emit = defineEmits<{
  confirm: []
}>()
</script>

<template>
  <UModal
    v-model:open="open"
    :title="title"
    :description="description"
    :ui="{ content: 'sm:max-w-md' }"
  >
    <template #footer>
      <div class="flex justify-end gap-2">
        <UButton variant="outline" @click="() => { open = false }">
          Annuleren
        </UButton>
        <UButton
          color="error"
          :loading="loading"
          @click="() => { emit('confirm') }"
        >
          {{ confirmLabel ?? 'Bevestigen' }}
        </UButton>
      </div>
    </template>
  </UModal>
</template>
