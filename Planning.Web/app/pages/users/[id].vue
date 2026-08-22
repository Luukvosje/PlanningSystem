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

const { tab, items: tabItems } = useEntityTabs(computed(() => [
  { value: 'details', label: t('users.tabs.details') },
  { value: 'planning', label: t('users.tabs.planning') },
]));
</script>

<template>
	<NuxtLayout name="default">
		<template #title>
			<UBreadcrumb :items="breadcrumbs" />
		</template>

		<template #tabs>
			<LayoutPageTabs
				v-model="tab"
				:items="tabItems"
			/>
		</template>

		<LayoutPageContainer>
			<UiQueryState
				:error="error"
				:loading="isLoading"
				:loading-label="t('users.loadingOne')"
			>
				<template v-if="user">
					<UsersDetails
						v-if="tab === 'details'"
						:user="user"
					/>

					<PlanningEntityShifts
						v-else
						:user-id="id"
					/>
				</template>
			</UiQueryState>
		</LayoutPageContainer>
	</NuxtLayout>
</template>
