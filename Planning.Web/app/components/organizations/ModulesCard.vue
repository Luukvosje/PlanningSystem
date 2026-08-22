<script setup lang="ts">
import type { AppModule } from '~/generated/models';
import {
  getOrganizationModuleToggleStates,
  modulesFromSettings,
  modulesToOrganizationRequest,
} from '~/utils/modules';

const { t } = useI18n();
const { data: organization, isLoading } = useCurrentOrganization();
const { updateOrganizationModules } = useModulesApi();

const moduleState = ref<Record<AppModule, boolean>>(modulesFromSettings([]));

const toggleStates = computed(() =>
  getOrganizationModuleToggleStates(organization.value?.modules),
);

watch(
  () => organization.value?.modules,
  (modules) => {
    moduleState.value = modulesFromSettings(modules);
  },
  { immediate: true },
);

function save() {
  updateOrganizationModules.mutate(modulesToOrganizationRequest(moduleState.value));
}
</script>

<template>
	<LayoutCard
		:title="t('users.modules.title')"
		:description="t('organizations.modules.description')"
	>
		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('organizations.modules.loading')"
		/>

		<ModulesToggles
			v-else
			v-model="moduleState"
			:toggle-states="toggleStates"
			:disabled="updateOrganizationModules.isPending.value"
		/>

		<template #footer>
			<div class="flex justify-end">
				<UButton
					size="sm"
					:loading="updateOrganizationModules.isPending.value"
					@click="save"
				>
					{{ t('common.actions.save') }}
				</UButton>
			</div>
		</template>
	</LayoutCard>
</template>
