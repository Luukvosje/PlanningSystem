<script setup lang="ts">
import type { PlanningRowMode } from '~/types/planning';

const { t } = useI18n();
const store = usePlanningStore();

const rowModes = computed<{ label: string, value: PlanningRowMode, icon: string }[]>(() => [
  { label: t('nav.team'), value: 'resource', icon: 'i-lucide-users' },
  { label: t('nav.customers'), value: 'customer', icon: 'i-lucide-building-2' },
]);
</script>
<template>
	<div class="flex justify-between">
		<div class="flex flex-wrap items-center gap-2">
			<UFieldGroup>
				<UButton
					v-for="mode in rowModes"
					:key="mode.value"
					:icon="mode.icon"
					:label="mode.label"
					:variant="store.rowMode === mode.value ? 'solid' : 'outline'"
					:color="store.rowMode === mode.value ? 'secondary' : 'neutral'"
					@click="() => { store.rowMode = mode.value }"
				/>
			</UFieldGroup>

		</div>
		<div class="flex flex-wrap items-center gap-2">
			<UButton
				v-if="!store.isTodayInView"
				variant="outline"
				color="neutral"
				:label="t('dashboard.today')"
				@click="store.goToToday()"
			/>
			<UFieldGroup>
				<UButton
					variant="outline"
					color="neutral"
					icon="i-lucide-chevron-left"
					:aria-label="t('planning.previousPeriod')"
					@click="store.shiftPeriod(-1)"
				/>
				<UButton
					variant="outline"
					color="neutral"
					icon="i-lucide-chevron-right"
					:aria-label="t('planning.nextPeriod')"
					@click="store.shiftPeriod(1)"
				/>
			</UFieldGroup>

			<PlanningSettingsDate />
			<PlanningSettings />
			<!-- <UButton
				v-if="canManage"
				icon="i-lucide-plus"
				:label="t('planning.new')"
				@click="() => { emit('create') }"
			/> -->
		</div>
	</div>
</template>
