<script setup lang="ts">
import type { FormControl } from '~/lib/form/control-types';
import { isBuiltInControl, isCustomControl, resolveControlProps } from '~/lib/form/control-types';
import type { Form } from '~/lib/form/Form';

const props = defineProps<{
  // eslint-disable-next-line @typescript-eslint/no-explicit-any -- the control is schema-agnostic
  form: Form<any>
  control: FormControl
}>();

const model = computed({
  get: () => (props.form.state as Record<string, unknown>)[props.control.name],
  set: (value: unknown) => {
    (props.form.state as Record<string, unknown>)[props.control.name] = value;
  },
});

/**
 * Typed views on the same field. The template used to cast inside v-model, which
 * is not a valid assignment target - esbuild rejects it and the production build failed, even
 * though the dev server tolerated it. Casting in a computed keeps the write path assignable.
 */
const textModel = computed<string>({
  get: () => (model.value == null ? '' : String(model.value)),
  set: (value) => {
    model.value = value;
  },
});

const selectModel = computed<string | number | undefined>({
  get: () => model.value as string | number | undefined,
  set: (value) => {
    model.value = value;
  },
});

const renderContext = computed(() => ({
  form: props.form,
  control: props.control,
}));

const controlProps = computed(() => resolveControlProps(props.control, renderContext.value));

const disabled = computed(() => unref(props.form.isSubmitting));

function builtInType(control: FormControl): 'input' | 'email' | 'password' | 'textarea' | 'select' {
  if (!isBuiltInControl(control)) {
    return 'input';
  }
  return control.type;
}
</script>

<template>
	<component
		:is="control.component"
		v-if="isCustomControl(control)"
		v-model="model"
		:disabled="disabled"
		v-bind="controlProps"
	/>

	<UInput
		v-else-if="builtInType(control) === 'input'"
		v-model="textModel"
		:disabled="disabled"
		class="w-full"
		v-bind="controlProps"
	/>

	<UInput
		v-else-if="builtInType(control) === 'email'"
		v-model="textModel"
		type="email"
		:disabled="disabled"
		class="w-full"
		v-bind="controlProps"
	/>

	<UInput
		v-else-if="builtInType(control) === 'password'"
		v-model="textModel"
		type="password"
		:disabled="disabled"
		class="w-full"
		v-bind="controlProps"
	/>

	<UTextarea
		v-else-if="builtInType(control) === 'textarea'"
		v-model="textModel"
		:disabled="disabled"
		class="w-full"
		v-bind="controlProps"
	/>

	<USelect
		v-else-if="builtInType(control) === 'select'"
		v-model="selectModel"
		:disabled="disabled"
		class="w-full"
		v-bind="controlProps"
	/>
</template>
