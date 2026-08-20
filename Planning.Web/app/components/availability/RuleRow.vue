<script setup lang="ts">
/**
 * One availability rule in a list.
 *
 * The weekly and one-time lists in RulesEditor gave every rule a full LayoutCard, while
 * WeeklyPatternCard rendered the same data as a light bordered row - the same information at two
 * very different visual weights. A rule is a row, not a card.
 */
defineProps<{
	label: string
	description?: string | null
	removing?: boolean
}>();

defineEmits<{ edit: []; remove: [] }>();

const { t } = useI18n();
</script>

<template>
	<div class="flex items-center justify-between gap-3 rounded-md border border-default px-3 py-2">
		<div class="min-w-0 ">
			<p class="truncate text-sm font-medium">
				{{ label }}
			</p>
			<p
				v-if="description"
				class="mt-0.5 truncate text-xs text-muted"
			>
				{{ description }}
			</p>
		</div>

		<div class="flex shrink-0 gap-1">
			<UButton
				icon="i-lucide-pencil"
				variant="ghost"
				color="neutral"
				size="sm"
				:aria-label="t('common.actions.edit')"
				@click="$emit('edit')"
			/>
			<UButton
				icon="i-lucide-trash-2"
				variant="ghost"
				color="error"
				size="sm"
				:loading="removing"
				:aria-label="t('common.actions.delete')"
				@click="$emit('remove')"
			/>
		</div>
	</div>
</template>
