<script setup lang="ts">
import type { AppModule } from '~/generated/models';
import { getModuleLabel, visibleModulesFromToggleStates } from '~/utils/modules';
import type { ModuleToggleState } from '~/utils/modules';

const props = defineProps<{
  modelValue: Record<AppModule, boolean>
  toggleStates: Record<AppModule, ModuleToggleState>
  saving?: boolean
}>();

const { t } = useI18n();

const emit = defineEmits<{
  'update:modelValue': [value: Record<AppModule, boolean>]
  save: []
}>();

const visibleModules = computed(() => visibleModulesFromToggleStates(props.toggleStates));

function updateModule(module: AppModule, enabled: boolean) {
  if (props.toggleStates[module]?.disabled) {
    return;
  }

  emit('update:modelValue', {
    ...props.modelValue,
    [module]: enabled,
  });
}
</script>

<template>
	<div class="space-y-3">
		<div
			v-for="module in visibleModules"
			:key="module"
			class="flex items-center justify-between gap-4"
		>
			<div>
				<p class="font-medium">
					{{ getModuleLabel(module, t) }}
				</p>
			</div>
			<UTooltip
				:text="toggleStates[module].tooltip"
				:disabled="!toggleStates[module].tooltip"
			>
				<USwitch
					:model-value="toggleStates[module].disabled ? toggleStates[module].checked : modelValue[module]"
					:disabled="toggleStates[module].disabled || saving"
					@update:model-value="(value) => updateModule(module, value)"
				/>
			</UTooltip>
		</div>

		<div class="flex justify-end pt-2">
			<UButton
				size="sm"
				:loading="saving"
				@click="emit('save')"
			>
				{{ t('common.actions.save') }}
			</UButton>
		</div>
	</div>
</template>
