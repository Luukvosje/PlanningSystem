<script setup lang="ts">
import type { AppModule } from '~/generated/models'
import { modulesFromSettings, modulesToRequest } from '~/utils/modules'

const { data: organization, isLoading } = useCurrentOrganization()
const { updateOrganizationModules } = useModulesApi()

const moduleState = ref<Record<AppModule, boolean>>(modulesFromSettings([]))

watch(
  () => organization.value?.modules,
  (modules) => {
    moduleState.value = modulesFromSettings(modules)
  },
  { immediate: true },
)

function save() {
  updateOrganizationModules.mutate(modulesToRequest(moduleState.value))
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
      :saving="updateOrganizationModules.isPending.value"
      @save="save"
    />
  </UCard>
</template>
