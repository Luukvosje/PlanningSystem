<script setup lang="ts">
const store = usePlanningStore()
const { canManage, selectedRecord, deleteRecord, duplicateRecord } = usePlanning()

const visible = computed(() =>
  canManage.value
  && !!store.selectedPlanningId
  && !!selectedRecord.value
  && !store.sidebarOpen,
)

function onEdit() {
  if (!store.selectedPlanningId) return
  store.openEdit(store.selectedPlanningId)
}

async function onDuplicate() {
  if (!store.selectedPlanningId) return
  await duplicateRecord(store.selectedPlanningId)
}

async function onDelete() {
  if (!store.selectedPlanningId) return
  await deleteRecord(store.selectedPlanningId)
}

const onClose = () => {
  store.selectedPlanningId = null;
}
</script>

<template>
  <Transition
    enter-active-class="transition duration-200 ease-out"
    enter-from-class="-translate-y-1 opacity-0"
    enter-to-class="translate-y-0 opacity-100"
    leave-active-class="transition duration-150 ease-in"
    leave-from-class="translate-y-0 opacity-100"
    leave-to-class="-translate-y-1 opacity-0"
  >
    <div
      v-if="visible && selectedRecord"
      class="w-full flex flex-wrap items-center justify-between gap-3 rounded-xl border border-default bg-default px-4 py-2 shadow-sm"
    >
      <p class="min-w-0 truncate text-sm font-medium text-highlighted">
        {{ selectedRecord.title }}
      </p>

      <div class="flex flex-wrap items-center gap-1 shrink-0">
        <UButton
          variant="ghost"
          size="sm"
          icon="i-lucide-pencil"
          label="Bewerken"
          @click="onEdit"
        />
        <UButton
          variant="ghost"
          size="sm"
          icon="i-lucide-copy"
          label="Dupliceren"
          @click="() => { void onDuplicate() }"
        />
        <UButton
          variant="ghost"
          size="sm"
          color="error"
          icon="i-lucide-trash-2"
          label="Verwijderen"
          @click="() => { void onDelete() }"
        />
      </div>
      <UButton
      icon="i-lucide-x"
      variant="ghost"
      color="neutral"
      size="sm"
      @click="onClose"
      />
    </div>
  </Transition>
</template>
