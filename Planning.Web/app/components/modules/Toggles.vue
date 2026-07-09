<script setup lang="ts">
import type { AppModule, ModuleSettingResponse } from '~/generated/models'
import { ALL_MODULES, MODULE_LABELS } from '~/utils/modules'

const props = defineProps<{
  modelValue: Record<AppModule, boolean>
  disabledModules?: Record<AppModule, boolean>
  saving?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: Record<AppModule, boolean>]
  save: []
}>()

function isToggleDisabled(module: AppModule) {
  return props.disabledModules?.[module] === false
}

function updateModule(module: AppModule, enabled: boolean) {
  emit('update:modelValue', {
    ...props.modelValue,
    [module]: enabled,
  })
}

function moduleTooltip(module: AppModule) {
  if (isToggleDisabled(module)) {
    return 'Niet beschikbaar op organisatieniveau'
  }

  return undefined
}
</script>

<template>
  <div class="space-y-3">
    <div
      v-for="module in ALL_MODULES"
      :key="module"
      class="flex items-center justify-between gap-4"
    >
      <div>
        <p class="font-medium">
          {{ MODULE_LABELS[module] }}
        </p>
      </div>
      <UTooltip :text="moduleTooltip(module)" :disabled="!isToggleDisabled(module)">
        <USwitch
          :model-value="modelValue[module]"
          :disabled="isToggleDisabled(module) || saving"
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
        Opslaan
      </UButton>
    </div>
  </div>
</template>
