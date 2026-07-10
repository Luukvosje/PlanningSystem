<script setup lang="ts">
import type { AppModule, UserResponse } from '~/generated/models'
import {
  getUserModuleToggleStates,
  modulesFromSettings,
  modulesToRequest,
} from '~/utils/modules'

const props = defineProps<{
  user: UserResponse
}>()

const { data: organization } = useCurrentOrganization()
const { updateUserModules } = useModulesApi()

const moduleState = ref<Record<AppModule, boolean>>(modulesFromSettings([]))

const toggleStates = computed(() =>
  getUserModuleToggleStates(
    props.user.role,
    props.user.modules,
    organization.value?.modules,
  ),
)

watch(
  () => [props.user.modules, props.user.role, organization.value?.modules] as const,
  () => {
    const states = toggleStates.value
    moduleState.value = ALL_MODULES.reduce((result, module) => {
      result[module] = states[module].disabled
        ? states[module].checked
        : modulesFromSettings(props.user.modules)[module]
      return result
    }, {} as Record<AppModule, boolean>)
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
      :toggle-states="toggleStates"
      :saving="updateUserModules.isPending.value"
      @save="save"
    />
  </UCard>
</template>
