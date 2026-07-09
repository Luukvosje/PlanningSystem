<script setup lang="ts">
import { unref, type Component, type MaybeRef } from 'vue'
import { CREATE_INJECTION_KEY, type CreateFooterMode, type CreateInstance } from '~/lib/form/useCreate'

const props = defineProps<{
  create: CreateInstance
}>()

const open = defineModel<boolean>('open', { default: false })

const emit = defineEmits<{
  close: []
  'after:leave': []
}>()

provide(CREATE_INJECTION_KEY, props.create)

const FormView = resolveComponent('FormView')

const title = computed(() => unref(props.create.title as MaybeRef<string>))
const description = computed(() => unref(props.create.description as MaybeRef<string | undefined>))
const submitLabel = computed(() => unref(props.create.submitLabel as MaybeRef<string>))
const cancelLabel = computed(() => unref(props.create.cancelLabel as MaybeRef<string>))
const modalUi = computed(() => unref(props.create.modalUi as MaybeRef<Record<string, unknown>>) ?? { content: 'sm:max-w-lg' })
const bodyExtra = computed(() => unref(props.create.bodyExtra as MaybeRef<Component | undefined>))
const form = computed(() => props.create.form)
const isSubmitting = computed(() => unref(form.value.isSubmitting))

const showFooterSubmit = computed(() => unref(props.create.footerMode as MaybeRef<CreateFooterMode>) === 'submit')

const showInlineSubmit = computed(() => {
  if (unref(props.create.footerMode as MaybeRef<CreateFooterMode>) !== 'close-only') {
    return false
  }

  const extensions = unref(props.create.extensions as MaybeRef<Record<string, Ref<unknown>>>) ?? {}
  const createdInvite = extensions.createdInvite as Ref<unknown> | undefined
  return !createdInvite?.value
})

watch(open, (value) => {
  if (!value) {
    emit('close')
  }
})
</script>

<template>
  <UModal
    v-model:open="open"
    :title="title"
    :description="description"
    :ui="modalUi"
    @after:leave="emit('after:leave')"
  >
    <template #body>
      <component :is="FormView" :form="form">
        <template v-if="showInlineSubmit" #submit>
          <UButton type="submit" :loading="isSubmitting">
            {{ submitLabel }}
          </UButton>
        </template>
      </component>

      <component
        :is="bodyExtra"
        v-if="bodyExtra"
        class="mt-4"
      />
    </template>

    <template #footer>
      <div class="flex justify-end gap-2">
        <UButton
          v-if="showFooterSubmit"
          variant="outline"
          :disabled="isSubmitting"
          @click="create.close()"
        >
          {{ cancelLabel }}
        </UButton>

        <UButton
          v-if="showFooterSubmit"
          :loading="isSubmitting"
          @click="form.submit()"
        >
          {{ submitLabel }}
        </UButton>

        <UButton
          v-else
          variant="outline"
          @click="create.close()"
        >
          Sluiten
        </UButton>
      </div>
    </template>
  </UModal>
</template>
