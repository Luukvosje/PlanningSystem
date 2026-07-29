<script setup lang="ts">
import type { Weekday } from '~/types/availability'
import { WEEKDAY_LABELS, WEEKDAY_OPTIONS } from '~/types/availability'
import {
  DEFAULT_IMPORTANT_WORK_TIMES,
  normalizeTimeValue,
  uniqueImportantTimes,
} from '~/utils/planning/planningSettings'

interface OpeningHoursRow {
  day: Weekday
  enabled: boolean
  openTime: string
  closeTime: string
}

const { data: organization, isLoading } = useCurrentOrganization()
const { updatePlanningSettings } = useOrganizationSettingsApi()

const importantWorkTimes = ref<string[]>([...DEFAULT_IMPORTANT_WORK_TIMES])
const openingHoursRows = ref<OpeningHoursRow[]>(
  WEEKDAY_OPTIONS.map(option => ({
    day: option.value,
    enabled: false,
    openTime: '06:00',
    closeTime: '22:00',
  })),
)

watch(
  organization,
  (org) => {
    if (!org) return

    importantWorkTimes.value = org.importantWorkTimes?.length
      ? uniqueImportantTimes(org.importantWorkTimes.map(normalizeTimeValue))
      : [...DEFAULT_IMPORTANT_WORK_TIMES]

    openingHoursRows.value = WEEKDAY_OPTIONS.map((option) => {
      const existing = org.openingHours?.find(entry => entry.day === option.value)
      return {
        day: option.value,
        enabled: !!existing,
        openTime: normalizeTimeValue(existing?.openTime ?? '06:00'),
        closeTime: normalizeTimeValue(existing?.closeTime ?? '22:00'),
      }
    })
  },
  { immediate: true },
)

function addImportantTime() {
  importantWorkTimes.value.push('09:00')
}

function removeImportantTime(index: number) {
  importantWorkTimes.value.splice(index, 1)
}

function save() {
  updatePlanningSettings.mutate({
    importantWorkTimes: uniqueImportantTimes(importantWorkTimes.value),
    openingHours: openingHoursRows.value
      .filter(row => row.enabled)
      .map(row => ({
        day: row.day,
        openTime: normalizeTimeValue(row.openTime),
        closeTime: normalizeTimeValue(row.closeTime),
      })),
  })
}
</script>

<template>
  <UCard>
    <template #header>
      <div>
        <h3 class="font-semibold">
          Planninginstellingen
        </h3>
        <p class="text-sm text-muted">
          Belangrijke werktijden en openingstijden voor de planningweergave.
        </p>
      </div>
    </template>

    <UiLoadingIndicator v-if="isLoading" label="Planninginstellingen laden..." />

    <div v-else class="space-y-8">
      <section class="space-y-3">
        <div>
          <h4 class="text-sm font-semibold">
            Belangrijke werktijden
          </h4>
          <p class="text-sm text-muted">
            Deze tijden worden benadrukt op de planning en gebruikt voor smart snap bij nieuwe diensten.
          </p>
        </div>

        <div class="space-y-2">
          <div
            v-for="(time, index) in importantWorkTimes"
            :key="`${time}-${index}`"
            class="flex items-center gap-2"
          >
            <UInput
              v-model="importantWorkTimes[index]"
              type="time"
              class="w-36"
            />
            <UButton
              icon="i-lucide-trash-2"
              color="neutral"
              variant="ghost"
              size="sm"
              :disabled="importantWorkTimes.length <= 1"
              @click="removeImportantTime(index)"
            />
          </div>
        </div>

        <UButton
          icon="i-lucide-plus"
          label="Tijd toevoegen"
          variant="outline"
          size="sm"
          @click="addImportantTime"
        />

        <OrganizationsImportantWorkTimesPreview :important-work-times="importantWorkTimes" />
      </section>

      <section class="space-y-3">
        <div>
          <h4 class="text-sm font-semibold">
            Openingstijden
          </h4>
          <p class="text-sm text-muted">
            Uren buiten openingstijden worden subtiel donkerder weergegeven. Plannen blijft overal mogelijk.
          </p>
        </div>

        <div class="space-y-2">
          <div
            v-for="row in openingHoursRows"
            :key="row.day"
            class="grid grid-cols-[1fr_auto_auto_auto] items-center gap-2"
          >
            <div class="flex items-center gap-2">
              <USwitch v-model="row.enabled" size="sm" />
              <span class="text-sm capitalize">{{ WEEKDAY_LABELS[row.day] }}</span>
            </div>
            <UInput
              v-model="row.openTime"
              type="time"
              class="w-32"
              :disabled="!row.enabled"
            />
            <span class="text-sm text-muted">–</span>
            <UInput
              v-model="row.closeTime"
              type="time"
              class="w-32"
              :disabled="!row.enabled"
            />
          </div>
        </div>
      </section>

      <UButton
        label="Planninginstellingen opslaan"
        :loading="updatePlanningSettings.isPending.value"
        @click="save"
      />
    </div>
  </UCard>
</template>
