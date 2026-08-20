<script setup lang="ts">
import type { BreadcrumbItem } from '@nuxt/ui';
import { useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '~/utils/queryKeys';

definePageMeta({ layout: false });

const route = useRoute();
const router = useRouter();
const id = computed(() => route.params.id as string);

const auth = useAuthStore();
const customersApi = useCustomersApi();
const queryClient = useQueryClient();
const toast = useToast();
const canManage = computed(() => canManageCustomers(auth.currentUser?.role));
const { t } = useI18n();

onMounted(async () => {
  await auth.fetchMe();

  if (!auth.hasOrganization) {
    await navigateTo('/dashboard');
  }
});

const { data: customer, isLoading, error } = useCustomer(id);

const breadcrumbs = computed<BreadcrumbItem[]>(() => [
  { label: t('nav.customers'), to: '/customers' },
  { label: customer.value?.name ?? t('common.loading') },
]);

// Placeholders: the numbers need an aggregate endpoint that doesn't exist yet.
const statTiles = computed(() => [
  { key: 'shiftsThisMonth', icon: 'i-lucide-calendar-check' },
  { key: 'hoursThisMonth', icon: 'i-lucide-clock' },
  { key: 'lastShift', icon: 'i-lucide-history' },
  { key: 'activeEmployees', icon: 'i-lucide-users' },
].map((tile) => ({
  ...tile,
  label: t(`customers.stats.${tile.key}`),
})));

const showDeleteModal = ref(false);
const isDeleting = ref(false);

async function onDelete() {
  isDeleting.value = true;

  try {
    await customersApi.delete(id.value);
    await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });

    toast.add({ title: t('customers.deleted'), color: 'success' });
    showDeleteModal.value = false;
    await router.push('/customers');
  } catch (err) {
    const { message: errMsg } = useApiError(err);
    toast.add({ title: errMsg.value, color: 'error' });
  } finally {
    isDeleting.value = false;
  }
}
</script>

<template>
	<NuxtLayout name="default">
		<template #title>
			<UBreadcrumb :items="breadcrumbs" />
		</template>

		<template
			v-if="canManage && customer"
			#actions
		>
			<UButton
				variant="outline"
				color="error"
				icon="i-lucide-trash-2"
				@click="() => { showDeleteModal = true }"
			>
				{{ t('common.actions.delete') }}
			</UButton>
		</template>

		<LayoutPageContainer>
			<UiQueryState
				:error="error"
				:loading="isLoading"
				:loading-label="t('customers.loadingOne')"
			>
				<template v-if="customer">
					<div class="grid grid-cols-2 gap-4 lg:grid-cols-4">
						<UiStatTile
							v-for="tile in statTiles"
							:key="tile.key"
							:label="tile.label"
							:icon="tile.icon"
							:placeholder="t('customers.stats.unavailable')"
						/>
					</div>

					<LayoutSection :title="t('customers.trend.title')">
						<UiEmptyState
							icon="i-lucide-chart-line"
							:title="t('customers.trend.empty')"
							:description="t('customers.trend.emptyDescription')"
						/>
					</LayoutSection>

					<CustomerDetails :customer="customer" />

					<UiConfirmModal
						v-model:open="showDeleteModal"
						:title="t('customers.deleteTitle')"
						:description="t('customers.deleteDescription')"
						:confirm-label="t('common.actions.delete')"
						:loading="isDeleting"
						@confirm="onDelete"
					/>
				</template>
			</UiQueryState>
		</LayoutPageContainer>
	</NuxtLayout>
</template>
