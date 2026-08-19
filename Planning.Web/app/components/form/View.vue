<script setup lang="ts">
import { isRef, unref, type ComponentPublicInstance, type MaybeRef } from 'vue';
import type { Form as UFormInstance } from '#ui/types';
import type { FormControl, FormSubmitConfig } from '~/lib/form/control-types';
import type { Form } from '~/lib/form/Form';

const props = defineProps<{
  // eslint-disable-next-line @typescript-eslint/no-explicit-any -- matches Control.vue's existing Form<any> prop; the class's generic doesn't unify soundly with UForm's own generic slots.
  form: Form<any>
}>();

const visibleControls = computed(() =>
  (unref(props.form.controls as MaybeRef<FormControl[]>) ?? []).filter((control) => !control.hidden),
);

const submitConfig = computed(() => unref(props.form.submitConfig as MaybeRef<FormSubmitConfig>));
const submitError = computed(() => unref(props.form.submitError));
const isSubmitting = computed(() => unref(props.form.isSubmitting));
const isDirty = computed(() => unref(props.form.isDirty));

function setFormRef(instance: Element | ComponentPublicInstance | null) {
  if (!isRef(props.form.formRef)) {
    return;
  }

  props.form.formRef.value = instance as UFormInstance<Record<string, unknown>> | null;
}
</script>

<template>
	<UForm
		:ref="setFormRef"
		:schema="form.schema"
		:state="form.state"
		:validate-on="form.validateOn"
		:loading-auto="false"
		:class="form.grid ? 'flex h-full min-h-0 flex-1 flex-col' : 'flex flex-col gap-3'"
		@submit="form.handleSubmit"
	>
		<div :class="form.grid ? 'min-h-0 grow divide-y divide-default overflow-auto' : 'contents'">
			<component
				:is="form.header"
				v-if="form.header"
				:form="form"
			/>

			<slot
				name="before"
				:form="form"
			/>

			<template
				v-for="control in visibleControls"
				:key="control.name"
			>
				<slot
					:name="`control-${control.name}`"
					:form="form"
					:control="control"
				>
					<div
						v-if="form.grid"
						class="grid grid-cols-1 items-start gap-x-4 gap-y-1 py-3 sm:grid-cols-[200px_1fr]"
					>
						<p class="text-muted text-sm sm:pt-2">
							{{ control.label }}<span v-if="control.required"> *</span>
						</p>
						<UFormField
							:name="control.name"
							:description="control.description"
							v-bind="control.fieldProps"
						>
							<FormControl
								:form="form"
								:control="control"
							/>
						</UFormField>
					</div>

					<UFormField
						v-else
						:label="control.label"
						:name="control.name"
						:required="control.required"
						:description="control.description"
						v-bind="control.fieldProps"
					>
						<FormControl
							:form="form"
							:control="control"
						/>
					</UFormField>
				</slot>
			</template>

			<slot
				name="after"
				:form="form"
			/>

			<slot
				name="error"
				:form="form"
				:submit-error="submitError"
			>
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
					v-if="!form.grid && !submitConfig.hidden"
					type="submit"
					:block="submitConfig.block"
					:loading="isSubmitting"
					v-bind="submitConfig.props"
				>
					{{ submitConfig.label }}
				</UButton>
			</slot>
		</div>

		<div
			v-if="$slots.footer"
			:class="isDirty ? 'sticky bottom-0 z-10 bg-default' : undefined"
		>
			<slot
				name="footer"
				:form="form"
				:is-dirty="isDirty"
				:is-submitting="isSubmitting"
				:submit-config="submitConfig"
			/>
		</div>
	</UForm>
</template>
