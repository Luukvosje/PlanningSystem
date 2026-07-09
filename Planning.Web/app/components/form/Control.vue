<script setup lang="ts">
import type { FormControl } from '~/lib/form/control-types'
import { isBuiltInControl, isCustomControl, resolveControlProps } from '~/lib/form/control-types'
import type { Form } from '~/lib/form/Form'

const props = defineProps<{
  form: Form<any>
  control: FormControl
}>()

const model = computed({
  get: () => (props.form.state as Record<string, unknown>)[props.control.name],
  set: (value: unknown) => {
    (props.form.state as Record<string, unknown>)[props.control.name] = value
  },
})

const renderContext = computed(() => ({
  form: props.form,
  control: props.control,
}))

const controlProps = computed(() => resolveControlProps(props.control, renderContext.value))

const disabled = computed(() => unref(props.form.isSubmitting))

function builtInType(control: FormControl): 'input' | 'email' | 'password' | 'textarea' | 'select' {
  if (!isBuiltInControl(control)) {
    return 'input'
  }
  return control.type
}
</script>

<template>
  <component
    v-if="isCustomControl(control)"
    :is="control.component"
    v-model="model"
    :disabled="disabled"
    v-bind="controlProps"
  />

  <UInput
    v-else-if="builtInType(control) === 'input'"
    v-model="model as string"
    :disabled="disabled"
    class="w-full"
    v-bind="controlProps"
  />

  <UInput
    v-else-if="builtInType(control) === 'email'"
    v-model="model as string"
    type="email"
    :disabled="disabled"
    class="w-full"
    v-bind="controlProps"
  />

  <UInput
    v-else-if="builtInType(control) === 'password'"
    v-model="model as string"
    type="password"
    :disabled="disabled"
    class="w-full"
    v-bind="controlProps"
  />

  <UTextarea
    v-else-if="builtInType(control) === 'textarea'"
    v-model="model as string"
    :disabled="disabled"
    class="w-full"
    v-bind="controlProps"
  />

  <USelect
    v-else-if="builtInType(control) === 'select'"
    v-model="model"
    :disabled="disabled"
    class="w-full"
    v-bind="controlProps"
  />
</template>
