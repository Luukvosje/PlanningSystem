<script setup lang="ts">
import { createOrganizationSchema } from '~/schemas/organization.schema';

definePageMeta({ layout: 'default' });

const { t } = useI18n();

const auth = useAuthStore();
const { updateOrganization } = useOrganizationSettingsApi();
const { data: organization, isLoading, error } = useCurrentOrganization();
const { message } = useApiError(error);

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
  submit: { hidden: true },
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
		<LayoutPageHeader
			:title="t('nav.organization')"
			:subtitle="t('organizations.manageDescription')"
		/>

		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('organizations.loading')"
		/>

		<UAlert
			v-else-if="error"
			color="error"
			:title="message"
		/>

		<div
			v-else
			class="grid grid-cols-1 gap-6 lg:grid-cols-2"
		>
			<LayoutCard :title="t('organizations.details')">
				<component :is="organizationForm.render" />

				<template #footer>
					<div class="flex items-center justify-between gap-4">
						<p
							v-if="organizationForm.isDirty.value"
							class="text-muted text-sm"
						>
							{{ t('common.unsavedChanges.text') }}
						</p>
						<span v-else />

						<div class="flex items-center gap-2">
							<UButton
								v-if="organizationForm.isDirty.value"
								variant="outline"
								:disabled="organizationForm.isSubmitting.value"
								@click="organizationForm.discard()"
							>
								{{ t('common.actions.cancel') }}
							</UButton>
							<UButton
								:disabled="!organizationForm.isDirty.value"
								:loading="organizationForm.isSubmitting.value"
								@click="() => { organizationForm.submit(); }"
							>
								{{ t('common.actions.save') }}
							</UButton>
						</div>
					</div>
				</template>
			</LayoutCard>

			<div class="flex flex-col gap-6">
				<OrganizationsLogoCard
					v-if="organization"
					:organization="organization"
				/>

				<OrganizationsPlanningSettingsCard />
			</div>
		</div>
	</LayoutPageContainer>
</template>
