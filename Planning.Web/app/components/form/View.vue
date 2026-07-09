<script setup lang="ts">
import { isRef, unref, type ComponentPublicInstance, type MaybeRef } from 'vue'
import type { Form as UFormInstance } from '#ui/types'
import type { FormControl, FormSubmitConfig } from '~/lib/form/control-types'
import type { Form } from '~/lib/form/Form'

const props = defineProps<{
  form: Form<any>
}>()

const visibleControls = computed(() =>
  (unref(props.form.controls as MaybeRef<FormControl[]>) ?? []).filter(control => !control.hidden),
)

const submitConfig = computed(() => unref(props.form.submitConfig as MaybeRef<FormSubmitConfig>))
const submitError = computed(() => unref(props.form.submitError))
const isSubmitting = computed(() => unref(props.form.isSubmitting))

function setFormRef(instance: Element | ComponentPublicInstance | null) {
  if (!isRef(props.form.formRef)) {
    return
  }

  props.form.formRef.value = instance as UFormInstance<Record<string, unknown>> | null
}
</script>

<template>
  <UForm
    :ref="setFormRef"
    :schema="form.schema"
    :state="form.state"
    :validate-on="form.validateOn"
    :loading-auto="false"
    class="space-y-4"
    @submit="form.handleSubmit"
  >
    <slot name="before" :form="form" />

    <template v-for="control in visibleControls" :key="control.name">
      <slot
        :name="`control-${control.name}`"
        :form="form"
        :control="control"
      >
        <UFormField
          :label="control.label"
          :name="control.name"
          :required="control.required"
          :description="control.description"
          v-bind="control.fieldProps"
        >
          <FormControl :form="form" :control="control" />
        </UFormField>
      </slot>
    </template>

    <slot name="after" :form="form" />

    <slot name="error" :form="form" :submit-error="submitError">
      <UAlert
        v-if="submitError"
        color="error"
        variant="subtle"
        :title="submitError"
      />
    </slot>

    <slot
      name="submit"
      :form="form"
      :is-submitting="isSubmitting"
      :submit-error="submitError"
      :submit-config="submitConfig"
    >
      <UButton
        v-if="!submitConfig.hidden"
        type="submit"
        :block="submitConfig.block"
        :loading="isSubmitting"
        v-bind="submitConfig.props"
      >
        {{ submitConfig.label }}
      </UButton>
    </slot>
  </UForm>
</template>
