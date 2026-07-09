<script setup lang="ts">
import type { AvailabilityEntry, DayPart } from '~/types/availability'
import {
  DAY_PART_LABELS,
  getDayPartAvailability,
  getTimeBlocksForDay,
} from '~/utils/planning/availabilityMath'

const props = defineProps<{
  date: Date
  userId: string
  entries: AvailabilityEntry[]
  disabled?: boolean
}>()

const emit = defineEmits<{
  dayPartChange: [dayPart: DayPart, isAvailable: boolean]
  addTimeBlock: [startTime: string, endTime: string]
  removeTimeBlock: [id: string]
}>()

const dayParts: DayPart[] = ['Morning', 'Afternoon', 'Evening']

const dateKey = computed(() => {
  const year = props.date.getFullYear()
  const month = String(props.date.getMonth() + 1).padStart(2, '0')
  const day = String(props.date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
})

const dayLabel = computed(() =>
  new Intl.DateTimeFormat('nl-NL', { weekday: 'long', day: 'numeric', month: 'short' }).format(props.date),
)

const timeBlocks = computed(() => getTimeBlocksForDay(props.entries, props.userId, dateKey.value))

const showTimeForm = ref(false)
const newStartTime = ref('19:00')
const newEndTime = ref('22:00')

function isDayPartUnavailable(dayPart: DayPart) {
  return Boolean(getDayPartAvailability(props.entries, props.userId, dateKey.value, dayPart))
}

function dayPartEntry(dayPart: DayPart) {
  return getDayPartAvailability(props.entries, props.userId, dateKey.value, dayPart)
}

function toggleDayPart(dayPart: DayPart, unavailable: boolean) {
  emit('dayPartChange', dayPart, !unavailable)
}

function submitTimeBlock() {
  if (!newStartTime.value || !newEndTime.value) return
  emit('addTimeBlock', newStartTime.value, newEndTime.value)
  showTimeForm.value = false
  newStartTime.value = '19:00'
  newEndTime.value = '22:00'
}
</script>

<template>
  <UCard :ui="{ body: 'p-4 space-y-4' }">
    <div class="flex items-center justify-between gap-2">
      <h3 class="text-sm font-semibold capitalize">
        {{ dayLabel }}
      </h3>
      <UBadge
        v-if="date.getDay() === 0 || date.getDay() === 6"
        color="neutral"
        variant="subtle"
        size="xs"
      >
        Weekend
      </UBadge>
    </div>

    <div class="space-y-2">
      <div
        v-for="dayPart in dayParts"
        :key="dayPart"
        class="flex items-center justify-between gap-3 rounded-lg border border-default/60 px-3 py-2"
      >
        <div class="min-w-0">
          <p class="text-sm font-medium">
            {{ DAY_PART_LABELS[dayPart] }}
          </p>
          <p
            v-if="dayPartEntry(dayPart)?.source === 'Manager'"
            class="text-xs text-warning mt-0.5"
          >
            Aangepast door {{ dayPartEntry(dayPart)?.lastModifiedByName }}
          </p>
        </div>
        <USwitch
          :model-value="!isDayPartUnavailable(dayPart)"
          :disabled="disabled"
          @update:model-value="(value) => toggleDayPart(dayPart, !value)"
        />
      </div>
    </div>

    <div class="space-y-2 pt-1 border-t border-default/60">
      <div class="flex items-center justify-between gap-2">
        <p class="text-sm font-medium">
          Specifieke tijd
        </p>
        <UButton
          v-if="!showTimeForm"
          size="xs"
          variant="ghost"
          icon="i-lucide-plus"
          label="Toevoegen"
          :disabled="disabled"
          @click="showTimeForm = true"
        />
      </div>

      <div
        v-for="block in timeBlocks"
        :key="block.id"
        class="flex items-center justify-between gap-2 rounded-lg bg-muted/40 px-3 py-2"
      >
        <div class="min-w-0">
          <p class="text-sm">
            {{ block.startTime?.slice(0, 5) }} – {{ block.endTime?.slice(0, 5) }} · Niet beschikbaar
          </p>
          <p
            v-if="block.source === 'Manager'"
            class="text-xs text-warning"
          >
            Aangepast door {{ block.lastModifiedByName }}
          </p>
        </div>
        <UButton
          size="xs"
          variant="ghost"
          color="error"
          icon="i-lucide-trash-2"
          :disabled="disabled"
          @click="emit('removeTimeBlock', block.id)"
        />
      </div>

      <div v-if="showTimeForm" class="grid grid-cols-2 gap-2">
        <UFormField label="Van">
          <UInput v-model="newStartTime" type="time" :disabled="disabled" />
        </UFormField>
        <UFormField label="Tot">
          <UInput v-model="newEndTime" type="time" :disabled="disabled" />
        </UFormField>
        <div class="col-span-2 flex gap-2 justify-end">
          <UButton size="sm" variant="ghost" label="Annuleer" @click="showTimeForm = false" />
          <UButton size="sm" label="Opslaan" :disabled="disabled" @click="submitTimeBlock" />
        </div>
      </div>
    </div>
  </UCard>
</template>
