<script setup lang="ts">
import { unref, type Component, type MaybeRef } from 'vue';
import { EDIT_INJECTION_KEY, type EditFooterMode, type EditInstance } from '~/lib/form/useEdit';

const props = defineProps<{
  edit: EditInstance
}>();

const open = defineModel<boolean>('open', { default: false });

const emit = defineEmits<{
  close: []
  'after:leave': []
}>();

provide(EDIT_INJECTION_KEY, props.edit);

const { t } = useI18n();
const FormView = resolveComponent('FormView');

const title = computed(() => unref(props.edit.title as MaybeRef<string>));
const description = computed(() => unref(props.edit.description as MaybeRef<string | undefined>));
const submitLabel = computed(() => unref(props.edit.submitLabel as MaybeRef<string>));
const cancelLabel = computed(() => unref(props.edit.cancelLabel as MaybeRef<string>));
const modalUi = computed(() => unref(props.edit.modalUi as MaybeRef<Record<string, unknown>>) ?? { content: 'sm:max-w-lg' });
const bodyExtra = computed(() => unref(props.edit.bodyExtra as MaybeRef<Component | undefined>));
const form = computed(() => props.edit.form);
const isSubmitting = computed(() => unref(form.value.isSubmitting));

const showFooterSubmit = computed(() => unref(props.edit.footerMode as MaybeRef<EditFooterMode>) === 'submit');

const showInlineSubmit = computed(() => {
  if (unref(props.edit.footerMode as MaybeRef<EditFooterMode>) !== 'close-only') {
    return false;
  }

  const extensions = unref(props.edit.extensions as MaybeRef<Record<string, Ref<unknown>>>) ?? {};
  const hideSubmit = extensions.hideInlineSubmit as Ref<boolean> | undefined;
  return !hideSubmit?.value;
});

watch(open, (value) => {
  if (!value) {
    emit('close');
  }
});
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
			<component
				:is="FormView"
				:form="form"
			>
				<template
					v-if="showInlineSubmit"
					#submit
				>
					<UButton
						type="submit"
						:loading="isSubmitting"
					>
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
					@click="edit.close()"
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
					@click="edit.close()"
				>
					{{ t('common.actions.close') }}
				</UButton>
			</div>
		</template>
	</UModal>
</template>
