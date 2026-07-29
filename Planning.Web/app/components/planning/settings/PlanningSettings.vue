<script setup lang="ts">
const store = usePlanningStore()
const { canManage } = usePlanningPermissions()

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
	{ type: 'label', label: 'Weergave' },
	{ type: 'layout' },
	{ type: 'separator' },
	{ type: 'label', label: 'Instellingen' },
	...(canManage.value ? [{
		type: 'toggle' as const,
		key: 'snapToBlocks' as const,
		label: 'Uitlijnen op blokken',
		description: 'Sleep en vergroot blokken uitgelijnd op andere blokken',
	}] : []),
	{ type: 'toggle', key: 'showAvailability', label: 'Beschikbaarheid', description: 'Toon wanneer medewerkers niet beschikbaar zijn' },
	{ type: 'toggle', key: 'showConcepts', label: 'Toon concepten', description: 'Toon geplande (niet-bevestigde) opdrachten' },
	{ type: 'toggle', key: 'showBlockColor', label: 'Toon blok kleur', description: 'Gebruik aangepaste blok kleuren' },
	{ type: 'toggle', key: 'showWeekends', label: 'Toon weekeinden', description: 'Markeer zaterdag en zondag' },
	{ type: 'toggle', key: 'showFullDay', label: 'Toon volledige dag', description: 'Toon alle uren, ook buiten openingstijden' },
])
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
