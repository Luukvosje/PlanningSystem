<script setup lang="ts">
import { organizationSchema } from '~/schemas/organization.schema'

definePageMeta({ layout: 'default' })

const FormView = resolveComponent('FormView')

const auth = useAuthStore()
const { updateOrganization } = useOrganizationSettingsApi()
const { data: organization, isLoading, error } = useCurrentOrganization()
const { message } = useApiError(error)

onMounted(async () => {
  await auth.fetchMe()

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard')
  }
})

const organizationForm = useForm({
  schema: organizationSchema,
  initialState: {
    name: '',
    email: '',
  },
  controls: [
    {
      name: 'name',
      label: 'Naam',
      type: 'input',
      required: true,
    },
    {
      name: 'email',
      label: 'E-mail',
      type: 'email',
      required: true,
      props: { autocomplete: 'email' },
    },
  ],
  submit: { label: 'Opslaan', block: true },
  onSubmit: async (data) => {
    await updateOrganization.mutateAsync(data)
  },
})

watch(
  organization,
  (org) => {
    if (!org) {
      return
    }

    organizationForm.state.name = org.name ?? ''
    organizationForm.state.email = org.email ?? ''
  },
  { immediate: true },
)
</script>

<template>
  <PageContainer>
    <PageHeader
      title="Organisatie"
      subtitle="Beheer de gegevens van je organisatie."
    />

    <UiLoadingIndicator v-if="isLoading" label="Organisatie laden..." />

    <UAlert v-else-if="error" color="error" :title="message" />

    <UCard v-else class="max-w-lg">
      <template #header>
        <h2 class="font-semibold">
          Gegevens
        </h2>
      </template>

      <component :is="FormView" :form="organizationForm" />
    </UCard>

    <OrganizationsLogoCard
      v-if="organization && !isLoading && !error"
      :organization="organization"
      class="mt-6"
    />

    <OrganizationsPlanningSettingsCard v-if="!isLoading && !error" class="max-w-2xl mt-6" />
  </PageContainer>
</template>
