<script setup lang="ts">
definePageMeta({ layout: 'default' });

const route = useRoute();
const id = computed(() => route.params.id as string);

const { data: user, isLoading, error } = useUser(id);
const { message } = useApiError(error);
const { t } = useI18n();
</script>

<template>
	<LayoutPageContainer>
		<UButton
			to="/users"
			variant="ghost"
			icon="i-lucide-arrow-left"
			size="sm"
		>
			{{ t('users.backToList') }}
		</UButton>

		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('users.loadingOne')"
		/>

		<UAlert
			v-else-if="error"
			color="error"
			:title="message"
		/>

		<UsersCard
			v-else-if="user"
			:user="user"
		/>
	</LayoutPageContainer>
</template>
