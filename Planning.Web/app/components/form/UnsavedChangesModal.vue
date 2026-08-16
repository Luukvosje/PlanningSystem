<script setup lang="ts">
const open = defineModel<boolean>('open', { default: false });

const emit = defineEmits<{
	close: [choice: 'save' | 'discard' | 'cancel']
}>();

const { t } = useI18n();

function choose(choice: 'save' | 'discard' | 'cancel') {
	open.value = false;
	emit('close', choice);
}
</script>

<template>
	<UModal
		v-model:open="open"
		:title="t('common.unsavedChanges.title')"
		:description="t('common.unsavedChanges.description')"
		:dismissible="false"
		:ui="{ content: 'sm:max-w-md' }"
		@close:prevent="choose('cancel')"
	>
		<template #footer>
			<div class="flex justify-end gap-2">
				<UButton
					variant="outline"
					@click="choose('cancel')"
				>
					{{ t('common.unsavedChanges.cancel') }}
				</UButton>
				<UButton
					color="error"
					variant="outline"
					@click="choose('discard')"
				>
					{{ t('common.unsavedChanges.discard') }}
				</UButton>
				<UButton @click="choose('save')">
					{{ t('common.unsavedChanges.save') }}
				</UButton>
			</div>
		</template>
	</UModal>
</template>
