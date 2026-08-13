<script setup lang="ts">
import { createUpdateProfileSchema } from '~/schemas/auth.schema';

definePageMeta({ layout: 'default' });

const { t } = useI18n();
const updateProfileSchema = createUpdateProfileSchema(t);

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const { updateProfile } = useProfileApi();
const { isLoading: profileLoading } = useCurrentUser();

onMounted(() => auth.fetchMe());

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
      label: t('auth.firstName'),
      type: 'input',
      required: true,
      props: { autocomplete: 'given-name' },
    },
    {
      name: 'lastName',
      label: t('auth.lastName'),
      type: 'input',
      required: true,
      hidden: true,
      props: { autocomplete: 'family-name' },
    },
    {
      name: 'email',
      label: t('auth.email'),
      type: 'email',
      required: true,
      props: { autocomplete: 'email' },
    },
  ],
  submit: { label: t('common.actions.save'), block: true },
  onSubmit: async (data) => {
    await updateProfile.mutateAsync(data);
  },
});

watch(
  () => auth.currentUser,
  (user) => {
    if (!user) {
      return;
    }

    profileForm.state.firstName = user.firstName ?? '';
    profileForm.state.lastName = user.lastName ?? '';
    profileForm.state.email = user.email ?? '';
  },
  { immediate: true },
);
</script>

<template>
	<LayoutPageContainer>
		<LayoutPageHeader
			:title="t('nav.settings')"
			:subtitle="t('settings.description')"
		/>

		<UiLoadingIndicator
			v-if="profileLoading && !auth.currentUser"
			:label="t('settings.loadingProfile')"
		/>

		<UCard
			v-else
			class="max-w-lg"
		>
			<template #header>
				<h2 class="font-semibold">
					{{ t('settings.profile') }}
				</h2>
			</template>

			<component
				:is="FormView"
				:form="profileForm"
			>
				<template #control-firstName="{ form }">
					<div class="grid grid-cols-2 gap-4">
						<UFormField
							:label="t('auth.firstName')"
							name="firstName"
							required
						>
							<UInput
								v-model="form.state.firstName"
								autocomplete="given-name"
								class="w-full"
								:disabled="form.isSubmitting.value"
							/>
						</UFormField>
						<UFormField
							:label="t('auth.lastName')"
							name="lastName"
							required
						>
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
