<script setup lang="ts">
import { createOrganizationSchema } from '~/schemas/organization.schema';

definePageMeta({ layout: 'default' });

const { t } = useI18n();
const FormView = resolveComponent('FormView');

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
  submit: { label: t('common.actions.save'), block: true },
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
  },
  { immediate: true },
);
</script>

<template>
	<PageContainer>
		<PageHeader
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

		<UCard
			v-else
			class="max-w-lg"
		>
			<template #header>
				<h2 class="font-semibold">
					{{ t('organizations.details') }}
				</h2>
			</template>

			<component
				:is="FormView"
				:form="organizationForm"
			/>
		</UCard>

		<OrganizationsLogoCard
			v-if="organization && !isLoading && !error"
			:organization="organization"
			class="mt-6"
		/>

		<OrganizationsPlanningSettingsCard
			v-if="!isLoading && !error"
			class="max-w-2xl mt-6"
		/>
	</PageContainer>
</template>
