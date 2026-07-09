<script setup lang="ts">
import type { DayPart } from '~/types/availability'
import { addDays, getMonday } from '~/utils/planning/dateUtils'

const props = defineProps<{
  userId: string
  canSelectUser?: boolean
}>()

const auth = useAuthStore()
const api = useAvailabilityApi()
const toast = useToast()

const weekDate = ref(getMonday(new Date()))
const selectedUserId = ref(props.userId)

const { data: users } = useUsers()
const { data: weekData, isLoading } = useAvailabilityWeek({
  weekDate,
  userId: computed(() => selectedUserId.value),
})

const userOptions = computed(() =>
  (users.value ?? []).map(user => ({
    label: `${user.firstName} ${user.lastName}`.trim(),
    value: user.id!,
  })),
)

const weekDays = computed(() =>
  Array.from({ length: 7 }, (_, index) => addDays(getMonday(weekDate.value), index)),
)

const entries = computed(() => weekData.value?.items ?? [])

function previousWeek() {
  weekDate.value = addDays(weekDate.value, -7)
}

function nextWeek() {
  weekDate.value = addDays(weekDate.value, 7)
}

function thisWeek() {
  weekDate.value = getMonday(new Date())
}

const weekLabel = computed(() => {
  const start = getMonday(weekDate.value)
  const end = addDays(start, 6)
  const formatter = new Intl.DateTimeFormat('nl-NL', { day: 'numeric', month: 'short' })
  return `${formatter.format(start)} – ${formatter.format(end)}`
})

async function onDayPartChange(day: Date, dayPart: DayPart, isAvailable: boolean) {
  const dateKey = `${day.getFullYear()}-${String(day.getMonth() + 1).padStart(2, '0')}-${String(day.getDate()).padStart(2, '0')}`
  try {
    await api.upsertDayPart.mutateAsync({
      userId: selectedUserId.value,
      date: dateKey,
      dayPart,
      isAvailable,
    })
    if (!isAvailable) {
      toast.add({ title: 'Onbeschikbaarheid opgeslagen', color: 'success' })
    }
  }
  catch {
    // Toast handled in mutation
  }
}

async function onAddTimeBlock(day: Date, startTime: string, endTime: string) {
  const dateKey = `${day.getFullYear()}-${String(day.getMonth() + 1).padStart(2, '0')}-${String(day.getDate()).padStart(2, '0')}`
  try {
    await api.upsertTimeBlock.mutateAsync({
      userId: selectedUserId.value,
      date: dateKey,
      startTime: `${startTime}:00`,
      endTime: `${endTime}:00`,
    })
    toast.add({ title: 'Tijdsblok toegevoegd', color: 'success' })
  }
  catch {
    // Toast handled in mutation
  }
}

async function onRemoveTimeBlock(id: string) {
  try {
    await api.remove.mutateAsync(id)
    toast.add({ title: 'Tijdsblok verwijderd', color: 'success' })
  }
  catch {
    // Toast handled in mutation
  }
}

watch(() => props.userId, (value) => {
  selectedUserId.value = value
})
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center gap-3 justify-between">
      <div class="flex items-center gap-2">
        <UButton variant="outline" icon="i-lucide-chevron-left" @click="previousWeek" />
        <UButton variant="outline" label="Deze week" @click="thisWeek" />
        <UButton variant="outline" icon="i-lucide-chevron-right" @click="nextWeek" />
        <span class="text-sm font-medium text-muted">{{ weekLabel }}</span>
      </div>

      <USelect
        v-if="canSelectUser"
        v-model="selectedUserId"
        :items="userOptions"
        value-key="value"
        label-key="label"
        class="w-full sm:w-64"
        placeholder="Selecteer medewerker"
      />
    </div>

    <UAlert
      v-if="selectedUserId !== auth.currentUser?.userId && canSelectUser"
      color="info"
      variant="subtle"
      title="Beheerdersmodus"
      description="Wijzigingen worden opgeslagen als beheerder en overschrijven de opgave van de medewerker."
    />

    <div v-if="isLoading" class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
      <USkeleton v-for="index in 7" :key="index" class="h-48" />
    </div>

    <div v-else class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
      <AvailabilityDayCard
        v-for="day in weekDays"
        :key="day.toISOString()"
        :date="day"
        :user-id="selectedUserId"
        :entries="entries"
        @day-part-change="(dayPart, isAvailable) => onDayPartChange(day, dayPart, isAvailable)"
        @add-time-block="(start, end) => onAddTimeBlock(day, start, end)"
        @remove-time-block="onRemoveTimeBlock"
      />
    </div>
  </div>
</template>
