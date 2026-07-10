<script setup lang="ts">
import type { AppModule } from '~/generated/models'
import {
  getOrganizationModuleToggleStates,
  modulesFromSettings,
  modulesToOrganizationRequest,
} from '~/utils/modules'

const { data: organization, isLoading } = useCurrentOrganization()
const { updateOrganizationModules } = useModulesApi()

const moduleState = ref<Record<AppModule, boolean>>(modulesFromSettings([]))

const toggleStates = computed(() =>
  getOrganizationModuleToggleStates(organization.value?.modules),
)

watch(
  () => organization.value?.modules,
  (modules) => {
    moduleState.value = modulesFromSettings(modules)
  },
  { immediate: true },
)

function save() {
  updateOrganizationModules.mutate(modulesToOrganizationRequest(moduleState.value))
}
</script>

<template>
  <UCard>
    <template #header>
      <div>
        <h3 class="font-semibold">
          Modules
        </h3>
        <p class="text-sm text-muted">
          Bepaal welke modules beschikbaar zijn voor de hele organisatie.
        </p>
      </div>
    </template>

    <USkeleton v-if="isLoading" class="h-32 w-full" />

    <ModulesToggles
      v-else
      v-model="moduleState"
      :toggle-states="toggleStates"
      :saving="updateOrganizationModules.isPending.value"
      @save="save"
    />
  </UCard>
</template>
