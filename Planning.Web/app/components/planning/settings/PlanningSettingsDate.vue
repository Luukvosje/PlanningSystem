<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui';
import type { TimelineZoom } from '~/types/planning';
import {
  getZoomLabel,
  getZoomPresets,
} from '~/utils/planning/timelineMath';

const { t } = useI18n();
const store = usePlanningStore();

const zoomLabel = computed(() => getZoomLabel(store.zoom, t));
const zoomPresets = computed(() => getZoomPresets(t));

type ZoomPresetMenuItem = DropdownMenuItem & {
  selected: boolean
  keypress: string
  zoom: TimelineZoom
};

const items = computed<ZoomPresetMenuItem[][]>(() => [
  zoomPresets.value.map((preset) => ({
    label: preset.label,
    zoom: preset.value,
    keypress: preset.keypress,
    selected: store.zoom === preset.value,
    onSelect: () => {
      store.zoom = preset.value;
    },
  })),
]);

function isZoomPresetItem(item: DropdownMenuItem): item is ZoomPresetMenuItem {
  return 'selected' in item && 'keypress' in item;
}

function isEditableTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) {
    return false;
  }
  const tag = target.tagName;
  return tag === 'INPUT' ||
    tag === 'TEXTAREA' ||
    tag === 'SELECT' ||
    target.isContentEditable;
}

function onKeydown(event: KeyboardEvent) {
  if (event.ctrlKey || event.metaKey || event.altKey) {
    return;
  }
  if (isEditableTarget(event.target)) {
    return;
  }

  if (event.key === '-' || event.key === '_') {
    event.preventDefault();
    store.zoomOut();
    return;
  }

  if (event.key === '+' || event.key === '=') {
    event.preventDefault();
    store.zoomIn();
    return;
  }

  const preset = zoomPresets.value.find(
    (p) => p.keypress === event.key.toLowerCase(),
  );

  if (preset) {
    event.preventDefault();
    store.zoom = preset.value;
  }
}

onMounted(() => {
  window.addEventListener('keydown', onKeydown);
});

onUnmounted(() => {
  window.removeEventListener('keydown', onKeydown);
});
</script>
<template>
	<UFieldGroup>
		<UDropdownMenu
			:items="items"
			:arrow="true"
			:content="{align: 'start', alignOffset: 0, sideOffset: 0}"
			:ui="{
				content: 'w-(--reka-dropdown-menu-trigger-width) min-w-44',
				group: 'flex flex-col gap-1 p-1.5',
				item: 'p-0 before:hidden data-highlighted:before:hidden',
			}"
		>
			<template #item="{ item }">
				<UButton
					v-if="isZoomPresetItem(item)"
					as="div"
					role="presentation"
					class="pointer-events-none"
					:variant="item.selected ? 'soft' : 'ghost'"
					:color="item.selected ? 'secondary' : 'neutral'"
					:label="item.label"
					:block="true"
					:ui="{
						base: item.selected
							? ''
							: 'group-data-highlighted:bg-elevated/50',
						label: 'flex-1 text-left'
					}"
				>
					<template #trailing>
						<UIcon
							v-if="item.selected"
							name="i-lucide-check"
							class="size-4 shrink-0"
						/>
						<span
							v-else
							class="rounded-sm bg-elevated px-1.5 py-0.5 text-xs font-normal tabular-nums text-muted"
						>{{ item.keypress }}</span>
					</template>
				</UButton>
			</template>
			<UButton
				variant="outline"
				color="neutral"
				:label="zoomLabel"
				class="font-semibold"
			/>
		</UDropdownMenu>
		<UButton
			variant="outline"
			color="neutral"
			icon="i-lucide-minus"
			:disabled="!store.canZoomOutLevel"
			@click="store.zoomOut()"
		/>
		<UButton
			variant="outline"
			color="neutral"
			icon="i-lucide-plus"
			:disabled="!store.canZoomInLevel"
			@click="store.zoomIn()"
		/>
	</UFieldGroup>
</template>


