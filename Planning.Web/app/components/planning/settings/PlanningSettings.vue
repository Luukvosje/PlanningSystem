<script setup lang="ts">
const { t } = useI18n();
const store = usePlanningStore();
const { canManage } = usePlanningPermissions();

interface ToggleSetting {
	type: 'toggle'
	key: 'snapToBlocks' | 'showAvailability' | 'showConcepts' | 'showBlockColor' | 'showWeekends' | 'showFullDay'
	label: string
	description?: string
}

interface SeparatorSetting {
	type: 'separator'
}

interface LabelSetting {
	type: 'label'
	label: string
}

interface LayoutSetting {
	type: 'layout'
}

type Setting = ToggleSetting | SeparatorSetting | LabelSetting | LayoutSetting

const settings = computed((): Setting[] => [
	{ type: 'label', label: t('planning.settings.view') },
	{ type: 'layout' },
	{ type: 'separator' },
	{ type: 'label', label: t('nav.settings') },
	...(canManage.value ? [{
		type: 'toggle' as const,
		key: 'snapToBlocks' as const,
		label: t('planning.settings.snapToBlocks'),
		description: t('planning.settings.snapToBlocksDescription'),
	}] : []),
	{ type: 'toggle', key: 'showAvailability', label: t('nav.availability'), description: t('planning.settings.showAvailabilityDescription') },
	{ type: 'toggle', key: 'showConcepts', label: t('planning.settings.showConcepts'), description: t('planning.settings.showConceptsDescription') },
	{ type: 'toggle', key: 'showBlockColor', label: t('planning.settings.showBlockColor'), description: t('planning.settings.showBlockColorDescription') },
	{ type: 'toggle', key: 'showWeekends', label: t('planning.settings.showWeekends'), description: t('planning.settings.showWeekendsDescription') },
	{ type: 'toggle', key: 'showFullDay', label: t('planning.settings.showFullDay'), description: t('planning.settings.showFullDayDescription') },
]);
</script>

<template>
	<UPopover :content="{ align: 'end', sideOffset: 4 }">
		<UButton
			icon="i-lucide-settings"
			variant="ghost"
			color="neutral"
		/>

		<template #content>
			<div class="w-64 p-2 flex flex-col gap-0.5">
				<template
					v-for="(setting, index) in settings"
					:key="index"
				>
					<p
						v-if="setting.type === 'label'"
						class="px-2 pt-2 pb-1 text-xs font-semibold text-muted uppercase tracking-wide"
					>
						{{ setting.label }}
					</p>

					<div
						v-else-if="setting.type === 'layout'"
						class="px-1 pb-1"
					>
						<PlanningSettingsLayoutPicker v-model="store.rowLayout" />
					</div>

					<USeparator
						v-else-if="setting.type === 'separator'"
						class="my-1"
					/>

					<PlanningSettingsToggle
						v-else-if="setting.type === 'toggle'"
						v-model="store[setting.key]"
						:label="setting.label"
						:description="setting.description"
					/>
				</template>
			</div>
		</template>
	</UPopover>
</template>
