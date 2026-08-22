<script setup lang="ts">
definePageMeta({ layout: false });

const { t } = useI18n();

const auth = useAuthStore();
const { data: organization, isLoading, error } = useCurrentOrganization();

onMounted(async () => {
  await auth.fetchMe();

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard');
  }
});

const organizationEdit = useOrganizationEdit();

const { tab, items: tabItems } = useEntityTabs(computed(() => [
  { value: 'details', label: t('organizations.tabs.details') },
  { value: 'planning', label: t('organizations.tabs.planning') },
  { value: 'modules', label: t('organizations.tabs.modules') },
]));
</script>

<template>
	<NuxtLayout name="default">
		<template #tabs>
			<LayoutPageTabs
				v-model="tab"
				:items="tabItems"
			/>
		</template>

		<LayoutPageContainer>
			<UiQueryState
				:error="error"
				:loading="isLoading"
				:loading-label="t('organizations.loading')"
			>
				<template v-if="tab === 'details'">
					<OrganizationsLogo
						v-if="organization"
						:organization="organization"
					/>

					<FormEditableSection
						v-if="organization"
						:key="organization.id"
						:edit="organizationEdit"
						:entity="organization"
						:title="t('organizations.details')"
						:description="t('organizations.manageDescription')"
					/>
				</template>

				<OrganizationsPlanningSettings v-else-if="tab === 'planning'" />

				<OrganizationsModulesCard v-else />
			</UiQueryState>
		</LayoutPageContainer>
	</NuxtLayout>
</template>
