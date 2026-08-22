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
const profile = computed(() => currentUser.value ?? auth.currentUser);
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
			<FormEditableSection
				v-if="profile"
				:edit="profileEdit"
				:entity="profile"
				:title="t('settings.profile')"
			/>
		</UiQueryState>

		<AvailabilityWeeklyPattern v-if="showAvailabilityPattern" />
	</LayoutPageContainer>
</template>
