<script setup lang="ts">
definePageMeta({ layout: 'default' });

const { t } = useI18n();

const auth = useAuthStore();
const { data: organization, isLoading, error } = useCurrentOrganization();

onMounted(async () => {
  await auth.fetchMe();

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard');
  }
});

const organizationForm = useOrganizationSettingsForm();
</script>

<template>
	<LayoutPageContainer>
		<LayoutSectionHeader
			:description="t('organizations.manageDescription')"
		/>

		<UiQueryState
			:error="error"
			:loading="isLoading"
			:loading-label="t('organizations.loading')"
		>
			<div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
				<div class="flex flex-col gap-6">
					<OrganizationsLogo
						v-if="organization"
						:organization="organization"
					/>

					<LayoutSection :title="t('organizations.details')">
						<component :is="organizationForm.render">
							<template #footer>
								<component :is="organizationForm.renderFooter" />
							</template>
						</component>
					</LayoutSection>
				</div>

				<OrganizationsPlanningSettings />
			</div>
		</UiQueryState>
	</LayoutPageContainer>
</template>
