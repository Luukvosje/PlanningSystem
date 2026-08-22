<script setup lang="ts">
/**
 * One number on a detail page. `value` omitted means the figure doesn't exist yet — the tile then
 * shows a dash plus `placeholder`, so an unfinished KPI row still reads as deliberate.
 */
withDefaults(defineProps<{
	label: string
	value?: string | number
	icon?: string
	placeholder?: string
}>(), {
	value: undefined,
	icon: undefined,
	placeholder: undefined,
});

const { t } = useI18n();
</script>

<template>
	<div class="flex min-w-0 flex-col gap-1 rounded-lg p-4 ring ring-default">
		<div class="flex items-center gap-2">
			<UIcon
				v-if="icon"
				:name="icon"
				class="size-4 shrink-0 text-dimmed"
			/>
			<p class="min-w-0 truncate text-sm text-muted">
				{{ label }}
			</p>
		</div>

		<p
			v-if="value !== undefined"
			class="truncate text-2xl font-semibold text-highlighted"
		>
			{{ value }}
		</p>

		<template v-else>
			<p class="text-2xl font-semibold text-dimmed">
				{{ t('common.empty') }}
			</p>
			<p
				v-if="placeholder"
				class="text-xs text-dimmed"
			>
				{{ placeholder }}
			</p>
		</template>
	</div>
</template>
