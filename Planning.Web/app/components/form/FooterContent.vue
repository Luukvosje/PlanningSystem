<script setup lang="ts">
import { unref } from 'vue';
import type { Form } from '~/lib/form/Form';

const props = defineProps<{
  // eslint-disable-next-line @typescript-eslint/no-explicit-any -- matches View.vue/Control.vue's existing Form<any> prop; the class's generic doesn't unify soundly with UForm's own generic slots.
  form: Form<any>
}>();

const submitConfig = computed(() => unref(props.form.submitConfig));
</script>

<template>
	<FormFooter
		v-if="form.grid && !submitConfig.hidden"
		:form="form"
	/>
	<component
		:is="form.footer"
		v-if="form.footer"
		:form="form"
	/>
</template>
