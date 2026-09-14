<script setup lang="ts">
import { PLANNING_COLORS } from '~/types/planning';

/**
 * The colour palette, in one place: the planning sidebar picks a record's colour with it and a
 * customer picks the colour its records start with. The trigger is a slot, because those two look
 * nothing alike - a swatch in a slideover header, a button in a form row.
 */
withDefaults(defineProps<{
	disabled?: boolean
}>(), {
	disabled: false,
});

const color = defineModel<string>({ required: true });

const isOpen = ref(false);

const { t } = useI18n();

function select(value: string) {
	color.value = value;
	isOpen.value = false;
}
</script>

<template>
	<UPopover
		v-model:open="isOpen"
		:arrow="true"
		:content="{ side: 'bottom', align: 'center', sideOffset: 8 }"
	>
		<slot
			name="trigger"
			:color="color"
		>
			<UButton
				variant="outline"
				color="neutral"
				:disabled="disabled"
				:aria-label="t('common.color.choose')"
			>
				<span
					class="size-4 rounded-full ring-1 ring-default"
					:style="{ backgroundColor: color }"
				/>
				{{ t('common.color.choose') }}
			</UButton>
		</slot>

		<template #content>
			<div
				class="flex flex-wrap gap-2 p-2"
				style="max-width: 152px;"
			>
				<button
					v-for="c in PLANNING_COLORS"
					:key="c"
					type="button"
					class="size-5 rounded-full ring-2 ring-offset-2 transition-transform hover:scale-110 focus:outline-none"
					:class="c === color ? 'ring-brand' : 'ring-transparent'"
					:style="{ backgroundColor: c }"
					:disabled="disabled"
					:aria-label="`${t('common.color.label')} ${c}`"
					@click="select(c)"
				/>
			</div>
		</template>
	</UPopover>
</template>
