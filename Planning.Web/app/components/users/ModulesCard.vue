<script setup lang="ts">
import type { AppModule, UserResponse } from '~/generated/models'
import { modulesFromSettings, modulesToRequest } from '~/utils/modules'

const props = defineProps<{
  user: UserResponse
}>()

const { data: organization } = useCurrentOrganization()
const { updateUserModules } = useModulesApi()

const moduleState = ref<Record<AppModule, boolean>>(modulesFromSettings([]))

const orgModuleState = computed(() => modulesFromSettings(organization.value?.modules))

watch(
  () => props.user.modules,
  (modules) => {
    moduleState.value = modulesFromSettings(modules)
  },
  { immediate: true },
)

function save() {
  if (!props.user.id) {
    return
  }

  updateUserModules.mutate({
    userId: props.user.id,
    request: modulesToRequest(moduleState.value),
  })
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
          Bepaal welke modules deze gebruiker mag gebruiken.
        </p>
      </div>
    </template>

    <ModulesToggles
      v-model="moduleState"
      :disabled-modules="orgModuleState"
      :saving="updateUserModules.isPending.value"
      @save="save"
    />
  </UCard>
</template>
