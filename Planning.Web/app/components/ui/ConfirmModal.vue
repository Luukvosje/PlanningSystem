<script setup lang="ts">
withDefaults(defineProps<{
  title: string
  description?: string
  confirmLabel?: string
  /** Deleting is red; confirming something that is merely irreversible is not. */
  confirmColor?: 'error' | 'brand'
  loading?: boolean
}>(), {
  description: undefined,
  confirmLabel: undefined,
  confirmColor: 'error',
  loading: false,
});

const open = defineModel<boolean>('open', { required: true });

const emit = defineEmits<{
  confirm: []
}>();

const { t } = useI18n();
</script>

<template>
	<!--
		Explicit z-index: a slideover and this dialog both sit at `auto`, so which one painted on top
		came down to DOM order - and the shared delete dialog is mounted from page load, so it lost to
		every panel opened later. A question you have to answer belongs above whatever asked it.
	-->
	<UModal
		v-model:open="open"
		:title="title"
		:description="description"
		:ui="{ overlay: 'z-[70]', content: 'z-[70] sm:max-w-md' }"
	>
		<template #footer>
			<div class="flex ml-auto gap-2">
				<UButton
					variant="outline"
					@click="() => { open = false }"
				>
					{{ t('common.actions.cancel') }}
				</UButton>
				<UButton
					:color="confirmColor"
					:loading="loading"
					@click="() => { emit('confirm') }"
				>
					{{ confirmLabel ?? t('common.actions.confirm') }}
				</UButton>
			</div>
		</template>
	</UModal>
</template>
