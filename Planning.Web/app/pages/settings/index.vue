<script setup lang="ts">
import { AppModule } from '~/generated/models';
import { canAccessModule } from '~/utils/modules';

definePageMeta({ layout: 'default' });

const { t } = useI18n();

const auth = useAuthStore();
const { data: currentUser, isLoading: profileLoading, error: profileError } = useCurrentUser();

const showAvailabilityPattern = computed(() =>
  canAccessModule(AppModule.Planning, auth.currentUser?.modules),
);

onMounted(() => auth.fetchMe());

const profileEdit = useProfileEdit();
const profileControls = profileEdit.form.controls;
const profile = computed(() => currentUser.value ?? auth.currentUser);
const profileValues = computed(() => profile.value ? profileEdit.toState(profile.value) : {});
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
				<template
					v-if="profile"
					#actions
				>
					<UButton
						variant="outline"
						color="neutral"
						icon="i-lucide-pencil"
						size="sm"
						@click="profileEdit.open(profile)"
					>
						{{ t('common.actions.edit') }}
					</UButton>
				</template>

				<FormDisplay
					:controls="profileControls"
					:values="profileValues"
				/>
			</LayoutSection>
		</UiQueryState>

		<AvailabilityWeeklyPattern v-if="showAvailabilityPattern" />
	</LayoutPageContainer>
</template>
