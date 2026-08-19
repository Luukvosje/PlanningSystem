<script setup lang="ts">
definePageMeta({ layout: 'default' });

const route = useRoute();
const id = computed(() => route.params.id as string);

const { data: user, isLoading, error } = useUser(id);
const { t } = useI18n();
</script>

<template>
	<LayoutPageContainer>
		<LayoutPageHeader>
			<UButton
				to="/users"
				variant="ghost"
				color="neutral"
				icon="i-lucide-arrow-left"
			>
				{{ t('users.backToList') }}
			</UButton>
		</LayoutPageHeader>

		<UiQueryState
			:error="error"
			:loading="isLoading"
			:loading-label="t('users.loadingOne')"
		>
			<UsersCard
				v-if="user"
				:user="user"
			/>
		</UiQueryState>
	</LayoutPageContainer>
</template>
