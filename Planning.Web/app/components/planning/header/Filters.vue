<script setup lang="ts">
const { t } = useI18n();
const store = usePlanningStore();
const { filters: _filters, statusOptions, hasActiveFilters, clearFilters } = usePlanningFilters();
const { data: users } = useUsers();
const { data: customers } = useCustomers();

const userOptions = computed(() =>
  (users.value ?? []).map((u) => ({
    label: `${u.firstName} ${u.lastName}`.trim(),
    value: u.id!,
  })),
);

const customerOptions = computed(() =>
  (customers.value ?? []).map((c) => ({
    label: c.name ?? t('common.unknown'),
    value: c.id!,
  })),
);
</script>

<template>
	<div class="flex flex-wrap items-center gap-2">
		<UInput
			v-model="store.filters.search"
			icon="i-lucide-search"
			:placeholder="t('common.actions.search') + '...'"
			class="w-48"
			size="sm"
		/>

		<USelectMenu
			v-model="store.filters.userIds"
			:items="userOptions"
			value-key="value"
			label-key="label"
			multiple
			:placeholder="t('planning.employees')"
			class="w-44"
			size="sm"
		/>

		<USelectMenu
			v-model="store.filters.customerIds"
			:items="customerOptions"
			value-key="value"
			label-key="label"
			multiple
			:placeholder="t('nav.customers')"
			class="w-44"
			size="sm"
		/>

		<USelectMenu
			v-model="store.filters.statuses"
			:items="statusOptions"
			value-key="value"
			label-key="label"
			multiple
			:placeholder="t('users.columns.status')"
			class="w-36"
			size="sm"
		/>

		<UButton
			v-if="hasActiveFilters"
			variant="ghost"
			size="sm"
			icon="i-lucide-x"
			:label="t('planning.clearFilters')"
			@click="() => { clearFilters() }"
		/>
	</div>
</template>
