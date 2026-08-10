<script setup lang="ts">
import type { PlanningStatus } from '~/types/planning'
import { getStatusLabel } from '~/utils/planning/dateUtils'

defineProps<{
  status: PlanningStatus
}>()

const colorMap: Record<PlanningStatus, 'neutral' | 'info' | 'success' | 'error'> = {
  Planned: 'neutral',
  Confirmed: 'success',
  Completed: 'neutral',
  Cancelled: 'error',
}
</script>

<template>
  <UBadge
    :color="colorMap[status]"
    variant="subtle"
    size="sm"
  >
    {{ getStatusLabel(status) }}
  </UBadge>
</template>
