<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { unref, type MaybeRef } from 'vue'
import { CREATE_INJECTION_KEY, type CreateInstance } from '~/lib/form/useCreate'

interface CreatedInvite {
  code: string
  expiresAtUtc: string
}

const create = inject(CREATE_INJECTION_KEY) as CreateInstance

const createdInvite = computed(() => {
  const extensions = unref(create.extensions as MaybeRef<Record<string, Ref<CreatedInvite | null> | undefined>>) ?? {}
  return unref(extensions.createdInvite as MaybeRef<CreatedInvite | null> | undefined) ?? null
})

const { copy, copied } = useClipboard({
  source: computed(() => createdInvite.value?.code ?? ''),
})

const toast = useToast()

function copyCode() {
  if (createdInvite.value?.code) {
    copy(createdInvite.value.code)
    toast.add({ title: 'Code gekopieerd', color: 'success' })
  }
}
</script>

<template>
  <UAlert
    v-if="createdInvite"
    color="success"
    variant="subtle"
    title="Uitnodigingscode"
    :description="`Geldig tot ${new Date(createdInvite.expiresAtUtc).toLocaleString('nl-NL')}`"
  >
    <template #actions>
      <div class="flex items-center gap-2 mt-2">
        <code class="text-lg font-mono font-bold tracking-widest">{{ createdInvite.code }}</code>
        <UButton size="sm" variant="outline" icon="i-lucide-copy" @click="copyCode">
          {{ copied ? 'Gekopieerd' : 'Kopieer' }}
        </UButton>
      </div>
    </template>
  </UAlert>
</template>
