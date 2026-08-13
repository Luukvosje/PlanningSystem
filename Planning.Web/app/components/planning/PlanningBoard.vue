<script setup lang="ts">
import type { TimelineRow } from '~/types/planning';

const { t } = useI18n();
const store = usePlanningStore();
const { records, isInitialLoading, error } = usePlanning();
const { availabilityPeriods } = usePlanningAvailability(records);
const { message } = useApiError(error);
const { data: users, isLoading: usersLoading } = useUsers();
const { data: customers, isLoading: customersLoading } = useCustomers();

const boardLoading = computed(() =>
  isInitialLoading.value ||
  usersLoading.value ||
  (store.rowMode === 'customer' && customersLoading.value),
);

const visibleRecords = computed(() =>
  store.showConcepts ?
    records.value :
    records.value.filter((r) => r.status !== 'Planned'),
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
      label: customer.name ?? t('common.unknown'),
      records: visibleRecords.value.filter((r) => r.customerId === customer.id),
    }));

    if (customerIds.length === 0) {
      const unassigned = visibleRecords.value.filter((r) => !r.customerId);
      if (unassigned.length > 0) {
        rows.unshift({
          id: '__unassigned__',
          label: t('planning.withoutCustomer'),
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
      records: visibleRecords.value.filter((r) => r.assignedUserId === user.id),
    }));
  }

  // Always keep resource/customer rows visible so users can still create
  // on an empty timeline (e.g. when status/search filters match nothing).
  return rows;
});

function openCreate() {
  const filteredUserIds = store.filters.userIds;
  const filteredCustomerIds = store.filters.customerIds;
  const activeUsers = (users.value ?? []).filter((u) => u.isActive !== false);
  const preferredUser = filteredUserIds.length > 0 ?
    activeUsers.find((u) => u.id && filteredUserIds.includes(u.id)) :
    activeUsers[0];
  if (!preferredUser?.id) {
    return;
  }

  const start = new Date();
  start.setMinutes(0, 0, 0);
  const end = new Date(start);
  end.setHours(end.getHours() + 1);

  const customerId = filteredCustomerIds.length === 1 ?
    filteredCustomerIds[0]! :
    undefined;

  store.openCreateSidebar({
    assignedUserId: preferredUser.id,
    customerId: customerId ?? null,
    status: store.filters.statuses[0],
    startUtc: start.toISOString(),
    endUtc: end.toISOString(),
  });
}
</script>

<template>
	<div class="flex h-full min-h-0 flex-col">
		<div class="flex shrink-0 flex-col gap-4 p-4 max-lg:gap-4 max-lg:p-4">
			<PlanningHeaderToolbar @create="openCreate" />
			<PlanningHeaderFilters />

			<UAlert
				v-if="error"
				color="error"
				:title="message"
			/>
		</div>

		<div class="min-h-0 flex-1 overflow-hidden">
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
