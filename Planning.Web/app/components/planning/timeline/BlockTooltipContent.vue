<script setup lang="ts">
import type { PlanningRecord } from '~/types/planning'
import { formatTimeRange } from '~/utils/planning/dateUtils'

const props = defineProps<{
  record: PlanningRecord
  interactionHint: string
}>()

const location = computed(() => props.record.customerName?.trim() || null)
const role = computed(() => props.record.title?.trim() || null)
const notes = computed(() => props.record.notes?.trim() || null)
const project = computed(() => props.record.description?.trim() || null)
</script>

<template>
  <div class="max-w-56 space-y-2 py-0.5 text-xs">
    <div class="space-y-0.5">
      <p class="font-semibold text-highlighted">
        {{ record.assignedUserName }}
      </p>
      <p class="text-muted">
        {{ formatTimeRange(record.startUtc, record.endUtc) }}
      </p>
    </div>

    <div v-if="location" class="space-y-0.5">
      <p class="font-medium text-highlighted">
        Locatie:
      </p>
      <p class="text-muted">
        {{ location }}
      </p>
    </div>

    <div v-if="role" class="space-y-0.5">
      <p class="font-medium text-highlighted">
        Functie:
      </p>
      <p class="text-muted">
        {{ role }}
      </p>
    </div>

    <div v-if="project" class="space-y-0.5">
      <p class="font-medium text-highlighted">
        Project:
      </p>
      <p class="text-muted whitespace-pre-wrap">
        {{ project }}
      </p>
    </div>

    <div v-if="notes" class="space-y-0.5">
      <p class="font-medium text-highlighted">
        Notitie:
      </p>
      <p class="text-muted whitespace-pre-wrap">
        {{ notes }}
      </p>
    </div>

    <p class="border-t border-default/60 pt-2 text-[10px] text-muted">
      {{ interactionHint }}
    </p>
  </div>
</template>
