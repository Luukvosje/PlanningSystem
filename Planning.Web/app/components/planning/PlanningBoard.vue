<script setup lang="ts">
import type { TimelineRow } from '~/types/planning'

const store = usePlanningStore()
const { records, isLoading, error } = usePlanning()
const { availabilityPeriods } = usePlanningAvailability(records)
const { message } = useApiError(error)
const { data: users } = useUsers()
const { data: customers } = useCustomers()

const timelineRows = computed<TimelineRow[]>(() => {
  if (store.rowMode === 'customer') {
    const customerList = customers.value ?? []
    const rows: TimelineRow[] = customerList.map(customer => ({
      id: customer.id!,
      label: customer.name ?? 'Onbekend',
      records: records.value.filter(r => r.customerId === customer.id),
    }))

    const unassigned = records.value.filter(r => !r.customerId)
    if (unassigned.length > 0) {
      rows.unshift({
        id: '__unassigned__',
        label: 'Zonder klant',
        records: unassigned,
      })
    }

    return rows
  }

  return (users.value ?? [])
    .filter(u => u.isActive !== false)
    .map(user => ({
      id: user.id!,
      label: `${user.firstName} ${user.lastName}`.trim(),
      records: records.value.filter(r => r.assignedUserId === user.id),
    }))
})

function openCreate() {
  const firstUser = users.value?.[0]
  if (!firstUser?.id) return

  const start = new Date()
  start.setMinutes(0, 0, 0)
  const end = new Date(start)
  end.setHours(end.getHours() + 1)

  store.openCreateSidebar({
    assignedUserId: firstUser.id,
    startUtc: start.toISOString(),
    endUtc: end.toISOString(),
  })
}
</script>

<template>
  <div class="flex flex-col gap-4 h-[calc(100vh-8rem)]">
    <PlanningHeaderToolbar @create="openCreate" />
    <PlanningHeaderFilters />
    <PlanningSelectionBar />

    <UAlert v-if="error" color="error" :title="message" />

    <div class="flex-1 min-h-0 overflow-hidden grow">
      <PlanningTimeline
        :rows="timelineRows"
        :is-loading="isLoading"
        :availability-periods="availabilityPeriods"
      />
    </div>

    <PlanningSidebar />
    <PlanningContextMenu />
    <PlanningCreatePopover />
  </div>
</template>
