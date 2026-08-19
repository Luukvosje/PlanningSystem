<script setup lang="ts" generic="T extends object">
import type { TableColumn, TableRow } from '@nuxt/ui';

/**
 * The list-page table: bordered container, sticky header, global filter and empty state.
 *
 * `customers/index.vue` and `users/index.vue` were copies of one another down to the wrapper
 * class string, which is why the two drifted apart on details like the empty-state text.
 *
 * Row density is set globally in app.config.ts (`ui.table.slots.th/td`), not here.
 */
const props = withDefaults(defineProps<{
	rows: T[] | undefined
	columns: TableColumn<T>[]
	/** Returns true when a row matches the search term. */
	filterFn?: (row: T, term: string) => boolean
	emptyTitle: string
	/** Shown instead of `emptyTitle` once a search term hides everything. */
	noResultsTitle: string
	selectable?: boolean
}>(), {
	filterFn: undefined,
	selectable: true,
});

const emit = defineEmits<{ select: [row: T] }>();

const search = defineModel<string>('search', { default: '' });

const hasRows = computed(() => (props.rows?.length ?? 0) > 0);

const filterOptions = computed(() => {
	const filterFn = props.filterFn;
	if (!filterFn) {
		return undefined;
	}
	return {
		globalFilterFn: (row: TableRow<T>, _columnId: string, term: string) => filterFn(row.original, term),
	};
});
const isFiltered = computed(() => search.value.trim().length > 0);

function onSelect(_event: Event, row: TableRow<T>) {
	emit('select', row.original);
}

/** Own slots stay ours; everything else (cell overrides) goes through to UTable. */
const passThroughSlots = computed(() =>
	Object.keys(useSlots()).filter((name) => name !== 'empty' && name !== 'empty-action'),
);
</script>

<template>
	<div class="min-h-0 flex-1 overflow-hidden rounded-lg border border-default">
		<UTable
			v-model:global-filter="search"
			:data="rows ?? []"
			:columns="columns"
			:global-filter-options="filterOptions"
			sticky
			class="h-full"
			:ui="selectable ? { tr: 'data-[selectable=true]:cursor-pointer' } : undefined"
			@select="onSelect"
		>
			<template #empty>
				<UiEmptyState
					fill
					:title="hasRows || isFiltered ? noResultsTitle : emptyTitle"
				>
					<template
						v-if="!hasRows && !isFiltered"
						#action
					>
						<slot name="empty-action" />
					</template>
				</UiEmptyState>
			</template>

			<template
				v-for="name in passThroughSlots"
				:key="name"
				#[name]="slotProps"
			>
				<slot
					:name="name"
					v-bind="slotProps ?? {}"
				/>
			</template>
		</UTable>
	</div>
</template>
