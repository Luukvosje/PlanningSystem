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

const organizationEdit = useOrganizationEdit();
const organizationControls = organizationEdit.form.controls;
const organizationValues = computed(() =>
  organization.value ? organizationEdit.toState(organization.value) : {},
);
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
						<template
							v-if="organization"
							#actions
						>
							<UButton
								variant="outline"
								color="neutral"
								icon="i-lucide-pencil"
								size="sm"
								@click="organizationEdit.open(organization)"
							>
								{{ t('common.actions.edit') }}
							</UButton>
						</template>

						<FormDisplay
							:controls="organizationControls"
							:values="organizationValues"
						/>
					</LayoutSection>
				</div>

				<OrganizationsPlanningSettings />
			</div>
		</UiQueryState>
	</LayoutPageContainer>
</template>
