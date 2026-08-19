<script setup lang="ts">
import { unref } from 'vue';
import type { Form } from '~/lib/form/Form';

const props = defineProps<{
  // eslint-disable-next-line @typescript-eslint/no-explicit-any -- matches View.vue; the class's generic doesn't unify soundly with UForm's own generic slots.
  form: Form<any>
}>();

const { t } = useI18n();

const isDirty = computed(() => unref(props.form.isDirty));
const isSubmitting = computed(() => unref(props.form.isSubmitting));
const submitConfig = computed(() => unref(props.form.submitConfig));
</script>

<template>
	<div
		class="flex shrink-0 items-center justify-between gap-4"
	>
		<div class="min-w-0">
			<slot>
				<p
					v-if="isDirty"
					class="text-muted text-sm"
				>
					{{ t('common.unsavedChanges.text') }}
				</p>
				<span v-else />
			</slot>
		</div>

		<div class="flex items-center gap-2">
			<slot name="actions">
				<UButton
					v-if="isDirty"
					type="button"
					variant="outline"
					:disabled="isSubmitting"
					@click="form.discard()"
				>
					{{ t('common.actions.cancel') }}
				</UButton>
				<UButton
					type="submit"
					:disabled="!isDirty"
					:loading="isSubmitting"
					v-bind="submitConfig.props"
				>
					{{ submitConfig.label ?? t('common.actions.save') }}
				</UButton>
			</slot>
		</div>
	</div>
</template>
