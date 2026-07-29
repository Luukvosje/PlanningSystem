<script setup lang="ts">
import type { TimelineRow } from '~/types/planning';

const store = usePlanningStore();
const { records, isInitialLoading, error } = usePlanning();
const { hasActiveFilters } = usePlanningFilters();
const { availabilityPeriods } = usePlanningAvailability(records);
const { message } = useApiError(error);
const { data: users, isLoading: usersLoading } = useUsers();
const { data: customers, isLoading: customersLoading } = useCustomers();

const boardLoading = computed(() =>
  isInitialLoading.value ||
  usersLoading.value ||
  (store.rowMode === 'customer' && customersLoading.value),
);

const timelineRows = computed<TimelineRow[]>(() => {
  const { userIds, customerIds } = store.filters;
  let rows: TimelineRow[];

  if (store.rowMode === 'customer') {
    let customerList = customers.value ?? [];
    if (customerIds.length > 0) {
      customerList = customerList.filter((customer) => customerIds.includes(customer.id!));
    }

    rows = customerList.map((customer) => ({
      id: customer.id!,
      label: customer.name ?? 'Onbekend',
      records: records.value.filter((r) => r.customerId === customer.id),
    }));

    if (customerIds.length === 0) {
      const unassigned = records.value.filter((r) => !r.customerId);
      if (unassigned.length > 0) {
        rows.unshift({
          id: '__unassigned__',
          label: 'Zonder klant',
          records: unassigned,
        });
      }
    }
  } else {
    let userList = (users.value ?? []).filter((u) => u.isActive !== false);
    if (userIds.length > 0) {
      userList = userList.filter((user) => userIds.includes(user.id!));
    }

    rows = userList.map((user) => ({
      id: user.id!,
      label: `${user.firstName} ${user.lastName}`.trim(),
      records: records.value.filter((r) => r.assignedUserId === user.id),
    }));
  }

  const resourceFilterActive = store.rowMode === 'resource' ?
    userIds.length > 0 :
    customerIds.length > 0;

  if (hasActiveFilters.value && !resourceFilterActive) {
    return rows.filter((row) => row.records.length > 0);
  }

  return rows;
});

function openCreate() {
  const firstUser = users.value?.[0];
  if (!firstUser?.id) {
return;
}

  const start = new Date();
  start.setMinutes(0, 0, 0);
  const end = new Date(start);
  end.setHours(end.getHours() + 1);

  store.openCreateSidebar({
    assignedUserId: firstUser.id,
    startUtc: start.toISOString(),
    endUtc: end.toISOString(),
  });
}
</script>

<template>
	<div class="flex flex-col gap-4 h-[calc(100vh-8rem)]">
		<PlanningHeaderToolbar @create="openCreate" />
		<PlanningHeaderFilters />

		<UAlert
			v-if="error"
			color="error"
			:title="message"
		/>

		<div class="flex-1 min-h-0 overflow-hidden grow">
			<PlanningLoading v-if="boardLoading" />
			<PlanningTimeline
				v-else
				:rows="timelineRows"
				:availability-periods="availabilityPeriods"
			/>
		</div>

		<PlanningSidebar />
		<PlanningContextMenu />
	</div>
</template>
