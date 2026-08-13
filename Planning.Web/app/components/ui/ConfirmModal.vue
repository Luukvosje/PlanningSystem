<script setup lang="ts">
defineProps<{
  title: string
  description?: string
  confirmLabel?: string
  loading?: boolean
}>();

const open = defineModel<boolean>('open', { required: true });

const emit = defineEmits<{
  confirm: []
}>();

const { t } = useI18n();
</script>

<template>
	<UModal
		v-model:open="open"
		:title="title"
		:description="description"
		:ui="{ content: 'sm:max-w-md' }"
	>
		<template #footer>
			<div class="flex justify-end gap-2">
				<UButton
					variant="outline"
					@click="() => { open = false }"
				>
					{{ t('common.actions.cancel') }}
				</UButton>
				<UButton
					color="error"
					:loading="loading"
					@click="() => { emit('confirm') }"
				>
					{{ confirmLabel ?? t('common.actions.confirm') }}
				</UButton>
			</div>
		</template>
	</UModal>
</template>
