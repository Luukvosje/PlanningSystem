<script setup lang="ts">
import type { PlanningViewMode, PlanningRowMode, TimelineZoom } from '~/types/planning'
import { formatDayHeader } from '~/utils/planning/dateUtils'
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

const emit = defineEmits<{
  create: []
}>()
</script>

<template>
  <div class="flex flex-col gap-3 border-b border-default pb-4">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-xl font-semibold">
          Planning
        </h1>
        <p class="text-sm text-muted">
          {{ rangeLabel }}
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <UButton
          v-if="canManage"
          icon="i-lucide-plus"
          label="Nieuw"
          @click="() => { emit('create') }"
        />
        <UButton variant="outline" icon="i-lucide-chevron-left" @click="() => { store.navigatePrevious() }" />
        <UButton variant="outline" label="Vandaag" @click="() => { store.goToToday() }" />
        <UButton variant="outline" icon="i-lucide-chevron-right" @click="() => { store.navigateNext() }" />
      </div>
    </div>

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
  </div>
</template>
