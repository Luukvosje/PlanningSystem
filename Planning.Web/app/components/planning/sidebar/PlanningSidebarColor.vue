<script setup lang="ts">
import { PLANNING_COLORS } from '~/types/planning';

const props = defineProps<{
  color: string;
}>();

const emit = defineEmits<{
  'update:color': [value: string];
}>();

const { t } = useI18n();
const { canManage } = usePlanning();
const isHovered = ref(false);
</script>

<template>
	<UPopover
		:arrow="true"
		:content="{ side: 'bottom', align: 'center', sideOffset: 8 }"
	>
		<UTooltip
			:title="t('planning.chooseColor')"
		>
			<UButton
				variant="ghost"
				color="neutral"
				class="group size-7 rounded-sm transition-transform hover:scale-110 p-0 m-0 flex items-center justify-center"
				:style="{ backgroundColor: props.color }"
				:disabled="!canManage"
				:ui="{ leadingIcon: 'group-hover:block hidden bg-default' }"
				leading-icon="i-lucide-palette"
				@mouse-leave="() => { isHovered = false }"
				@mouse-enter="() => { isHovered = true }"
			/>
		</UTooltip>

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
					:class="c === props.color ? 'ring-primary-500' : 'ring-transparent'"
					:style="{ backgroundColor: c }"
					:disabled="!canManage"
					:aria-label="`${t('planning.color')} ${c}`"
					@click="emit('update:color', c)"
				/>
			</div>
		</template>
	</UPopover>
</template>
