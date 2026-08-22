<script setup lang="ts">
const { t } = useI18n();

// Placeholders: the numbers need an aggregate endpoint that doesn't exist yet.
const statTiles = computed(() => [
  { key: 'shiftsThisMonth', icon: 'i-lucide-calendar-check' },
  { key: 'hoursThisMonth', icon: 'i-lucide-clock' },
  { key: 'lastShift', icon: 'i-lucide-history' },
  { key: 'activeEmployees', icon: 'i-lucide-users' },
].map((tile) => ({
  ...tile,
  label: t(`customers.stats.${tile.key}`),
})));
</script>

<template>
	<div class="grid grid-cols-2 gap-4 lg:grid-cols-4">
		<UiStatTile
			v-for="tile in statTiles"
			:key="tile.key"
			:label="tile.label"
			:icon="tile.icon"
			:placeholder="t('customers.stats.unavailable')"
		/>
	</div>

	<LayoutSection :title="t('customers.trend.title')">
		<UiEmptyState
			icon="i-lucide-chart-line"
			:title="t('customers.trend.empty')"
			:description="t('customers.trend.emptyDescription')"
		/>
	</LayoutSection>
</template>
