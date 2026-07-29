<script setup lang="ts">
import { updateProfileSchema } from '~/schemas/auth.schema'

definePageMeta({ layout: 'default' })

const FormView = resolveComponent('FormView')

const auth = useAuthStore()
const { updateProfile } = useProfileApi()
const { isLoading: profileLoading } = useCurrentUser()

onMounted(() => auth.fetchMe())

const profileForm = useForm({
  schema: updateProfileSchema,
  initialState: {
    firstName: auth.currentUser?.firstName ?? '',
    lastName: auth.currentUser?.lastName ?? '',
    email: auth.currentUser?.email ?? '',
  },
  controls: [
    {
      name: 'firstName',
      label: 'Voornaam',
      type: 'input',
      required: true,
      props: { autocomplete: 'given-name' },
    },
    {
      name: 'lastName',
      label: 'Achternaam',
      type: 'input',
      required: true,
      hidden: true,
      props: { autocomplete: 'family-name' },
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
    await updateProfile.mutateAsync(data)
  },
})

watch(
  () => auth.currentUser,
  (user) => {
    if (!user) {
      return
    }

    profileForm.state.firstName = user.firstName ?? ''
    profileForm.state.lastName = user.lastName ?? ''
    profileForm.state.email = user.email ?? ''
  },
  { immediate: true },
)
</script>

<template>
  <LayoutPageContainer>
    <LayoutPageHeader
      title="Instellingen"
      subtitle="Beheer je persoonlijke gegevens."
    />

    <UiLoadingIndicator v-if="profileLoading && !auth.currentUser" label="Profiel laden..." />

    <UCard v-else class="max-w-lg">
      <template #header>
        <h2 class="font-semibold">
          Profiel
        </h2>
      </template>

      <component :is="FormView" :form="profileForm">
        <template #control-firstName="{ form }">
          <div class="grid grid-cols-2 gap-4">
            <UFormField label="Voornaam" name="firstName" required>
              <UInput
                v-model="form.state.firstName"
                autocomplete="given-name"
                class="w-full"
                :disabled="form.isSubmitting.value"
              />
            </UFormField>
            <UFormField label="Achternaam" name="lastName" required>
              <UInput
                v-model="form.state.lastName"
                autocomplete="family-name"
                class="w-full"
                :disabled="form.isSubmitting.value"
              />
            </UFormField>
          </div>
        </template>
      </component>
    </UCard>
  </LayoutPageContainer>
</template>
