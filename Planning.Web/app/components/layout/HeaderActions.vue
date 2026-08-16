<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui';
import { useMediaQuery } from '@vueuse/core';
import type {
  HeaderAction,
  HeaderActionPopover,
  HeaderActionSelect,
  HeaderActionSlot,
  HeaderActionSwitch,
} from '~/types/headerActions';

const props = defineProps<{
  items: HeaderAction[]
}>();

defineSlots<{
  [key: string]: () => unknown
}>();

const { t } = useI18n();
const isDesktop = useMediaQuery('(min-width: 1024px)');

const overlayActions = computed(() =>
  props.items.filter((action): action is HeaderActionSlot | HeaderActionPopover =>
    action.type === 'slot' || action.type === 'popover',
  ),
);

const switchActions = computed(() =>
  props.items.filter((action): action is HeaderActionSwitch => action.type === 'switch'),
);

const menuItems = computed<DropdownMenuItem[]>(() =>
  props.items.flatMap((action): DropdownMenuItem[] => {
    if (action.type === 'button') {
      return [{
        label: action.label,
        icon: action.icon,
        color: action.color,
        onSelect: action.onSelect,
      }];
    }

    if (action.type === 'switch') {
      return [{
        label: action.label,
        description: action.description,
        icon: action.icon,
        slot: action.key,
        onSelect: (event: Event) => {
          event.preventDefault();
          action.onUpdate(!action.modelValue);
        },
      }];
    }

    if (action.type === 'select') {
      return [{
        label: action.label,
        children: action.items.map((item) => ({
          label: item.label,
          icon: item.icon,
          type: 'checkbox' as const,
          checked: item.value === action.value,
          onSelect: () => {
            action.onUpdate(item.value);
          },
        })),
      }];
    }

    return [];
  }),
);

function onSelectMenuValue(action: HeaderActionSelect, value: unknown) {
  if (typeof value === 'string') {
    action.onUpdate(value);
  }
}

function onSwitchValue(action: HeaderActionSwitch, value: unknown) {
  if (typeof value === 'boolean') {
    action.onUpdate(value);
  }
}
</script>

<template>
	<div
		v-if="isDesktop"
		class="flex min-w-0 flex-wrap items-center justify-end gap-2"
	>
		<template
			v-for="action in items"
			:key="action.key"
		>
			<UButton
				v-if="action.type === 'button'"
				:icon="action.icon"
				:label="action.label"
				:color="action.color ?? 'neutral'"
				variant="outline"
				size="sm"
				@click="action.onSelect"
			/>

			<div
				v-else-if="action.type === 'switch'"
				class="flex items-center gap-2"
			>
				<span class="text-sm text-default">
					{{ action.label }}
				</span>
				<USwitch
					:model-value="action.modelValue"
					size="sm"
					color="secondary"
					@update:model-value="onSwitchValue(action, $event)"
				/>
			</div>

			<UFieldGroup
				v-else-if="action.type === 'select' && action.desktop === 'buttons'"
			>
				<UButton
					v-for="item in action.items"
					:key="item.value"
					:icon="item.icon"
					:label="item.label"
					:variant="action.value === item.value ? 'solid' : 'outline'"
					:color="action.value === item.value ? 'secondary' : 'neutral'"
					size="sm"
					@click="action.onUpdate(item.value)"
				/>
			</UFieldGroup>

			<USelectMenu
				v-else-if="action.type === 'select'"
				:model-value="action.value"
				:items="action.items"
				value-key="value"
				label-key="label"
				size="sm"
				class="w-44"
				@update:model-value="onSelectMenuValue(action, $event)"
			/>

			<UPopover
				v-else-if="action.type === 'popover'"
				:content="{ align: 'end', sideOffset: 4 }"
			>
				<UButton
					:icon="action.icon"
					:label="action.label"
					variant="outline"
					color="neutral"
					size="sm"
				/>
				<template #content>
					<slot :name="action.key" />
				</template>
			</UPopover>

			<slot
				v-else-if="action.type === 'slot'"
				:name="action.key"
			/>
		</template>
	</div>

	<UDropdownMenu
		v-else
		:items="menuItems"
		:modal="false"
		:content="{ align: 'end' }"
		:ui="{ content: 'min-w-64' }"
		size="sm"
	>
		<UButton
			icon="i-lucide-settings"
			color="neutral"
			variant="ghost"
			square
			size="sm"
			:aria-label="t('layout.headerActions.open')"
		/>

		<template
			v-if="overlayActions.length"
			#content-top
		>
			<div
				class="flex flex-col gap-2 p-2"
				:class="menuItems.length ? 'border-b border-default' : undefined"
			>
				<div
					v-for="action in overlayActions"
					:key="action.key"
				>
					<slot :name="action.key" />
				</div>
			</div>
		</template>

		<template
			v-for="action in switchActions"
			:key="action.key"
			#[`${action.key}-trailing`]
		>
			<USwitch
				:model-value="action.modelValue"
				size="sm"
				color="secondary"
				@update:model-value="onSwitchValue(action, $event)"
				@click.stop
			/>
		</template>
	</UDropdownMenu>
</template>
