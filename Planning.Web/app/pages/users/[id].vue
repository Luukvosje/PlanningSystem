<script setup lang="ts">
import type { BreadcrumbItem } from '@nuxt/ui';

definePageMeta({ layout: false });

const route = useRoute();
const id = computed(() => route.params.id as string);

const { data: user, isLoading, error } = useUser(id);
const { t } = useI18n();

const breadcrumbs = computed<BreadcrumbItem[]>(() => [
  { label: t('nav.team'), to: '/users' },
  { label: user.value ? `${user.value.firstName} ${user.value.lastName}` : t('common.loading') },
]);
</script>

<template>
	<NuxtLayout name="default">
		<template #title>
			<UBreadcrumb :items="breadcrumbs" />
		</template>

		<LayoutPageContainer>
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
	</NuxtLayout>
</template>
