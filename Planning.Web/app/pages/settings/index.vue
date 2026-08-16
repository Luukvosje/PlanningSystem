<script setup lang="ts">
import { createUpdateProfileSchema } from '~/schemas/auth.schema';
import { AppModule } from '~/generated/models';
import { canAccessModule } from '~/utils/modules';

definePageMeta({ layout: 'default' });

const { t } = useI18n();
const updateProfileSchema = createUpdateProfileSchema(t);

const auth = useAuthStore();
const { updateProfile } = useProfileApi();
const { isLoading: profileLoading } = useCurrentUser();

const showAvailabilityPattern = computed(() =>
  canAccessModule(AppModule.Planning, auth.currentUser?.modules),
);

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
  submit: { hidden: true },
  grid: true,
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
    profileForm.markClean();
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

		<LayoutCard
			v-else
			:title="t('settings.profile')"
		>
			<component :is="profileForm.render">
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

			<template #footer>
				<div class="flex items-center justify-between gap-4">
					<p
						v-if="profileForm.isDirty.value"
						class="text-muted text-sm"
					>
						{{ t('common.unsavedChanges.text') }}
					</p>
					<span v-else />

					<div class="flex items-center gap-2">
						<UButton
							v-if="profileForm.isDirty.value"
							variant="outline"
							:disabled="profileForm.isSubmitting.value"
							@click="profileForm.discard()"
						>
							{{ t('common.actions.cancel') }}
						</UButton>
						<UButton
							:disabled="!profileForm.isDirty.value"
							:loading="profileForm.isSubmitting.value"
							@click="() => { profileForm.submit(); }"
						>
							{{ t('common.actions.save') }}
						</UButton>
					</div>
				</div>
			</template>
		</LayoutCard>

		<AvailabilityWeeklyPatternCard v-if="showAvailabilityPattern" />
	</LayoutPageContainer>
</template>
