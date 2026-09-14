<script setup lang="ts">
const { t } = useI18n();
const store = usePlanningStore();
const { canManage, records } = usePlanning();
const { duplicateRecord, deleteRecord } = usePlanning();
const { confirmDelete } = useDeleteConfirm();

const menu = computed(() => store.contextMenu);
const _record = computed(() =>
  records.value.find((r) => r.id === menu.value?.recordId),
);

function close() {
  store.closeContextMenu();
}

async function onDuplicate() {
  if (!menu.value) {
    return;
  }
  await duplicateRecord(menu.value.recordId);
  close();
}

async function onDelete() {
  if (!menu.value) {
    return;
  }

  // Read the id and close the menu before asking: this menu closes itself on the next click
  // anywhere, the dialog's own button included, so by the time the answer comes back `menu` is
  // already empty.
  const { recordId } = menu.value;
  close();

  const confirmed = await confirmDelete({
    title: t('planning.deleteTitle'),
    description: t('planning.deleteDescription'),
    confirmLabel: t('common.actions.delete'),
  });

  if (!confirmed) {
    return;
  }

  await deleteRecord(recordId);
}

function onEdit() {
  if (!menu.value) {
    return;
  }
  store.openEdit(menu.value.recordId);
  close();
}

onMounted(() => {
  document.addEventListener('click', close);
});

onUnmounted(() => {
  document.removeEventListener('click', close);
});
</script>

<template>
	<Teleport to="body">
		<div
			v-if="menu && canManage"
			class="fixed z-50 min-w-40 rounded-lg border border-default bg-default shadow-lg py-1"
			:style="{ left: `${menu.x}px`, top: `${menu.y}px` }"
			@click.stop
		>
			<button
				class="w-full px-3 py-2 text-left text-sm hover:bg-muted flex items-center gap-2"
				@click="() => { onEdit() }"
			>
				<UIcon
					name="i-lucide-pencil"
					class="size-4"
				/>
				{{ t('common.actions.edit') }}
			</button>
			<button
				class="w-full px-3 py-2 text-left text-sm hover:bg-muted flex items-center gap-2"
				@click="() => { onDuplicate() }"
			>
				<UIcon
					name="i-lucide-copy"
					class="size-4"
				/>
				{{ t('planning.duplicate') }}
			</button>
			<button
				class="w-full px-3 py-2 text-left text-sm hover:bg-muted text-error flex items-center gap-2"
				@click="() => { onDelete() }"
			>
				<UIcon
					name="i-lucide-trash-2"
					class="size-4"
				/>
				{{ t('common.actions.delete') }}
			</button>
		</div>
	</Teleport>
</template>
