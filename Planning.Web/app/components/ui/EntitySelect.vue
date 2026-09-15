<script setup lang="ts">
import type { EntityKind, EntityOption } from '~/composables/entities/useEntitySource';

/**
 * One picker for every kind of entity you choose from a list - employees, customers, whatever
 * comes next. Search, an active/inactive switch and A-Z/Recent sorting live here once, instead of
 * in the four hand-rolled `{ label, value }` mappings this replaces, which had already drifted
 * apart over whether inactive people belong in the list.
 *
 * What an entity *is* - which query, how it is named, what you search in, whether it can be
 * inactive - lives in `useEntitySource`, not here.
 *
 * Single and multiple select each get their own model so both stay correctly typed:
 * `v-model:value` holds one id, `v-model:values` holds many, and `multiple` says which one lives.
 */
const props = withDefaults(defineProps<{
	kind: EntityKind
	multiple?: boolean
	disabled?: boolean
	clearable?: boolean
	placeholder?: string
	size?: 'sm' | 'md'
	/** Limits the list to these ids. Undefined or empty means no limit. */
	restrictTo?: string[]
	/** Fixed entries above the list, outside search and sorting - e.g. an open shift. */
	pinned?: { id: string, label: string }[]
}>(), {
	multiple: false,
	disabled: false,
	clearable: false,
	placeholder: undefined,
	size: 'md',
	restrictTo: undefined,
	pinned: undefined,
});

const value = defineModel<string | null>('value', { default: null });
const values = defineModel<string[]>('values', { default: () => [] });

const { t } = useI18n();

const {
	options,
	isLoading,
	supportsActive,
	icon,
	placeholder: entityPlaceholder,
} = useEntitySource(() => props.kind);

const { rank, remember, sort, showInactive } = useRecentEntities(() => props.kind);

const sortOptions = computed(() => [
	{ value: 'alphabetical' as const, label: t('common.sort.alphabetical'), icon: 'i-lucide-arrow-down-a-z' },
	{ value: 'recent' as const, label: t('common.sort.recent'), icon: 'i-lucide-history' },
]);

function compare(a: EntityOption, b: EntityOption) {
	if (sort.value === 'recent') {
		const byRank = (rank.value.get(a.id) ?? Number.MAX_SAFE_INTEGER) -
			(rank.value.get(b.id) ?? Number.MAX_SAFE_INTEGER);

		if (byRank !== 0) {
			return byRank;
		}
	}

	return a.label.localeCompare(b.label);
}

// Pinned entries first, then the real list. `USelectMenu` does the text search over the result.
const items = computed<EntityOption[]>(() => {
	const limit = props.restrictTo;
	let list = options.value;

	if (limit?.length) {
		list = list.filter((option) => limit.includes(option.id));
	}

	if (supportsActive.value && !showInactive.value) {
		list = list.filter((option) => option.isActive);
	}

	const pinned = (props.pinned ?? []).map<EntityOption>((entry) => ({
		id: entry.id,
		label: entry.label,
		description: null,
		isActive: true,
	}));

	return [...pinned, ...[...list].sort(compare)];
});

const selection = computed(() => (props.multiple ? values.value : value.value));

function isPinned(id: string) {
	return (props.pinned ?? []).some((entry) => entry.id === id);
}

function onUpdate(next: unknown) {
	if (props.multiple) {
		const ids = (next as string[] | null) ?? [];

		ids
			.filter((id) => !values.value.includes(id) && !isPinned(id))
			.forEach(remember);

		values.value = ids;
		return;
	}

	const id = (next as string | null) ?? null;
	value.value = id;

	if (id && !isPinned(id)) {
		remember(id);
	}
}
</script>

<template>
	<USelectMenu
		:model-value="selection"
		:items="items"
		value-key="id"
		label-key="label"
		:filter-fields="['label', 'description']"
		:multiple="multiple"
		:disabled="disabled"
		:icon="icon"
		:placeholder="placeholder ?? entityPlaceholder"
		:loading="isLoading"
		:search-input="{ placeholder: `${t('common.actions.search')}...` }"
		:size="size"
		:clear="clearable"
		class="w-full"
		:content="{side: 'bottom', align: 'end', position: 'popper'}"
		:ui="{content: 'min-w-96'}"
		@update:model-value="onUpdate"
	>
		<template #item-trailing="{ item }">
			<UBadge
				v-if="!item.isActive"
				color="neutral"
				variant="subtle"
				size="sm"
			>
				{{ t('common.status.inactive') }}
			</UBadge>
		</template>

		<!-- Inside the combobox portal, so clicks must not reach the list behind it. -->
		<template #content-bottom>
			<div
				class="flex items-center justify-between gap-2 border-t border-default px-2 py-1.5"
				@pointerdown.stop
				@click.stop
			>
				<UFieldGroup size="sm">
					<UButton
						v-for="option in sortOptions"
						:key="option.value"
						:icon="option.icon"
						:variant="sort === option.value ? 'solid' : 'outline'"
						:color="sort === option.value ? 'brand' : 'neutral'"
						size="sm"
						@click="sort = option.value"
					/>
				</UFieldGroup>

				<USwitch
					v-if="supportsActive"
					v-model="showInactive"
					:label="t('common.showInactive')"
					size="sm"
					color="brand"
					:ui="{ label: 'text-xs text-muted' }"
				/>
			</div>
		</template>
	</USelectMenu>
</template>
