<script setup lang="ts">
import { createOrganizationSchema } from '~/schemas/organization.schema';

definePageMeta({ layout: 'default' });

const { t } = useI18n();

const auth = useAuthStore();
const { updateOrganization } = useOrganizationSettingsApi();
const { data: organization, isLoading, error } = useCurrentOrganization();

onMounted(async () => {
  await auth.fetchMe();

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard');
  }
});

const organizationSchema = createOrganizationSchema(t);

const organizationForm = useForm({
  schema: organizationSchema,
  initialState: {
    name: '',
    email: '',
  },
  controls: [
    {
      name: 'name',
      label: t('organizations.fields.name'),
      type: 'input',
      required: true,
    },
    {
      name: 'email',
      label: t('organizations.fields.email'),
      type: 'email',
      required: true,
      props: { autocomplete: 'email' },
    },
  ],
  grid: true,
  onSubmit: async (data) => {
    await updateOrganization.mutateAsync(data);
  },
});

watch(
  organization,
  (org) => {
    if (!org) {
      return;
    }

    organizationForm.state.name = org.name ?? '';
    organizationForm.state.email = org.email ?? '';
    organizationForm.markClean();
  },
  { immediate: true },
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
