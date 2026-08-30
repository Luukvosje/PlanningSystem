<script setup lang="ts">
const { t } = useI18n();
const store = usePlanningStore();
const { filters: _filters, statusOptions, hasActiveFilters, clearFilters } = usePlanningFilters();
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

		<UiEntitySelect
			v-model:values="store.filters.userIds"
			kind="user"
			multiple
			:placeholder="t('planning.employees')"
			class="w-44"
			size="sm"
		/>

		<UiEntitySelect
			v-model:values="store.filters.customerIds"
			kind="customer"
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
