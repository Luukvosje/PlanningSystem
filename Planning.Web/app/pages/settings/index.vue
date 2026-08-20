<script setup lang="ts">
import { AppModule } from '~/generated/models';
import { canAccessModule } from '~/utils/modules';

definePageMeta({ layout: 'default' });

const { t } = useI18n();

const auth = useAuthStore();
const { isLoading: profileLoading, error: profileError } = useCurrentUser();

const showAvailabilityPattern = computed(() =>
  canAccessModule(AppModule.Planning, auth.currentUser?.modules),
);

onMounted(() => auth.fetchMe());

const profileForm = useProfileForm();
</script>

<template>
	<LayoutPageContainer>
		<LayoutSectionHeader
			:description="t('settings.description')"
		/>

		<UiQueryState
			:error="profileError"
			:loading="profileLoading && !auth.currentUser"
			:loading-label="t('settings.loadingProfile')"
		>
			<LayoutSection :title="t('settings.profile')">
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
					<template #footer>
						<component :is="profileForm.renderFooter" />
					</template>
				</component>
			</LayoutSection>
		</UiQueryState>

		<AvailabilityWeeklyPattern v-if="showAvailabilityPattern" />
	</LayoutPageContainer>
</template>
