<script setup lang="ts">
const store = usePlanningStore()
const { createRecord } = usePlanning()
const { data: customers } = useCustomers()
const { data: users } = useUsers()

const popover = computed(() => store.createPopover)

const { data: availabilityWeek } = useAvailabilityWeek({
  weekDate: computed(() => store.currentDate),
  userId: computed(() => popover.value?.assignedUserId ?? null),
})

const { checkAssignment } = useAvailabilityWarning(
  computed(() => availabilityWeek.value?.items ?? []),
  users,
)

const customerOptions = computed(() =>
  (customers.value ?? []).map(c => ({
    label: c.name ?? 'Onbekend',
    value: c.id!,
  })),
)

const title = ref('')
const customerId = ref<string | null>(null)
const isSaving = ref(false)

const createWarning = computed(() => {
  if (!popover.value) return null
  const result = checkAssignment(
    popover.value.assignedUserId,
    popover.value.startUtc,
    popover.value.endUtc,
  )
  return result.hasConflict ? result.message : null
})

watch(popover, (state) => {
  if (state) {
    title.value = ''
    customerId.value = state.customerId ?? null
  }
})

async function save() {
  if (!popover.value || !title.value.trim() || isSaving.value) return
  isSaving.value = true
  try {
    await createRecord({
      title: title.value.trim(),
      assignedUserId: popover.value.assignedUserId,
      customerId: customerId.value,
      startUtc: popover.value.startUtc,
      endUtc: popover.value.endUtc,
    })
    store.closeCreatePopover()
  }
  finally {
    isSaving.value = false
  }
}

function close() {
  store.closeCreatePopover()
}

onMounted(() => {
  document.addEventListener('keydown', onKeydown)
})

onUnmounted(() => {
  document.removeEventListener('keydown', onKeydown)
})

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') close()
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="popover"
      class="fixed z-50 w-72 rounded-xl border border-default bg-default shadow-xl p-4"
      :style="{ left: `${popover.x}px`, top: `${popover.y}px` }"
      @click.stop
    >
      <p class="text-sm font-medium mb-3">
        Nieuwe planning
      </p>

      <div class="space-y-3">
        <UInput
          v-model="title"
          placeholder="Titel..."
          autofocus
          class="w-full"
          @keydown.enter.prevent="save"
        />

        <USelect
          v-model="customerId"
          :items="customerOptions"
          value-key="value"
          label-key="label"
          placeholder="Klant (optioneel)"
          class="w-full"
          clearable
          :ui="{base: 'z-51', content: 'z-51'}"
        />

        <UAlert
          v-if="createWarning"
          color="warning"
          variant="subtle"
          :title="createWarning"
        />

        <div class="flex gap-2 justify-end">
          <UButton variant="ghost" label="Annuleer" size="sm" @click="() => { close() }" />
          <UButton label="Opslaan" size="sm" :loading="isSaving" :disabled="!title.trim()" @click="() => { save() }" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
