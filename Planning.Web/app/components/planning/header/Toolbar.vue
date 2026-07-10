<script setup lang="ts">
import type { DateValue } from '@internationalized/date'
import { fromDate, getLocalTimeZone, toCalendarDate } from '@internationalized/date'
import type { PlanningViewMode, PlanningRowMode, TimelineZoom } from '~/types/planning'
import { formatDayHeader, getMonday } from '~/utils/planning/dateUtils'
import { computeDateRange } from '~/utils/planning/timelineMath'

const store = usePlanningStore()
const { canManage } = usePlanningPermissions()

const viewModes: { label: string, value: PlanningViewMode }[] = [
  { label: 'Dag', value: 'daily' },
  { label: 'Week', value: 'weekly' },
  { label: 'Maand', value: 'monthly' },
]

const rowModes: { label: string, value: PlanningRowMode, icon: string }[] = [
  { label: 'Team', value: 'resource', icon: 'i-lucide-users' },
  { label: 'Klanten', value: 'customer', icon: 'i-lucide-building-2' },
]

const zoomOptions: { label: string, value: TimelineZoom }[] = [
  { label: '15m', value: '15m' },
  { label: '30m', value: '30m' },
  { label: '1u', value: '1h' },
  { label: '2u', value: '2h' },
  { label: '4u', value: '4h' },
]

const rangeLabel = computed(() => {
  const { start, end } = computeDateRange(store.currentDate, store.viewMode)
  if (store.viewMode === 'daily') return formatDayHeader(start)
  if (store.viewMode === 'monthly') {
    return new Intl.DateTimeFormat('nl-NL', { month: 'long', year: 'numeric' }).format(start)
  }
  const endDay = new Date(end)
  endDay.setDate(endDay.getDate() - 1)
  return `${formatDayHeader(start)} – ${formatDayHeader(endDay)}`
})

const calendarOpen = ref(false)

const calendarDefaultDate = computed(() =>
  toCalendarDate(fromDate(store.currentDate, getLocalTimeZone())),
)

function onCalendarDateSelect(
  value: DateValue | { start?: DateValue, end?: DateValue } | DateValue[] | null | undefined,
) {
  if (!value || Array.isArray(value) || !('day' in value)) return
  const date = value.toDate(getLocalTimeZone())
  if (store.viewMode === 'daily') {
    store.currentDate = date
  }
  else if (store.viewMode === 'monthly') {
    store.currentDate = new Date(date.getFullYear(), date.getMonth(), 1)
  }
  else {
    store.currentDate = getMonday(date)
  }
  calendarOpen.value = false
}

function goToTodayAndClose() {
  store.goToToday()
  calendarOpen.value = false
}

const emit = defineEmits<{
  create: []
}>()
</script>

<template>
  <div class="flex justify-between border-b border-default pb-4">
    <div class="flex flex-wrap items-center gap-2">
      <UFieldGroup>
        <UButton
          v-for="mode in viewModes"
          :key="mode.value"
          :label="mode.label"
          :variant="store.viewMode === mode.value ? 'solid' : 'outline'"
          size="sm"
          @click="() => { store.viewMode = mode.value }"
        />
      </UFieldGroup>

      <UFieldGroup>
        <UButton
          v-for="mode in rowModes"
          :key="mode.value"
          :icon="mode.icon"
          :label="mode.label"
          :variant="store.rowMode === mode.value ? 'solid' : 'outline'"
          size="sm"
          @click="() => { store.rowMode = mode.value }"
        />
      </UFieldGroup>

      <USelect
        v-model="store.zoom"
        :items="zoomOptions"
        value-key="value"
        label-key="label"
        class="w-24"
        size="sm"
      />
    </div>
    <div class="flex flex-wrap items-center gap-2">
      <UFieldGroup
        >
        <UButton variant="outline" icon="i-lucide-chevron-left" @click="() => { store.navigatePrevious() }" />
          <UPopover v-model:open="calendarOpen">
        <UButton
 variant="outline"
          :label="rangeLabel"
        />

        <template #content>
          <div class="flex flex-col gap-2 p-2">
            <UCalendar
              v-if="calendarOpen"
              :default-value="calendarDefaultDate"
              color="secondary"
              @update:model-value="onCalendarDateSelect"
            />
            <UButton
              variant="outline"
              label="Vandaag"
              block
              @click="goToTodayAndClose"
            />
          </div>
        </template>
      </UPopover>
        <UButton variant="outline" icon="i-lucide-chevron-right" @click="() => { store.navigateNext() }" />
      </UFieldGroup>
      <UButton
      v-if="canManage"
      icon="i-lucide-plus"
      label="Nieuw"
      @click="() => { emit('create') }"
      />
    </div>
  </div>
</template>
