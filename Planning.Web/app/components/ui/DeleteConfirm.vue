<script setup lang="ts">
/**
 * Mounted once in `app.vue`. Every delete in the app asks through `useDeleteConfirm`, so this is
 * the only place the question is rendered - a dialog per call site would be five copies of the
 * same thing and one of them would drift.
 */
const { isOpen, request, answer } = useDeleteConfirm();

const open = computed({
  get: () => isOpen.value,
  set: (value: boolean) => {
    if (!value) {
      answer(false);
    }
  },
});
</script>

<template>
	<UiConfirmModal
		v-model:open="open"
		:title="request?.title ?? ''"
		:description="request?.description"
		:confirm-label="request?.confirmLabel"
		@confirm="answer(true)"
	/>
</template>
