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

const { tab, items: tabItems } = useEntityTabs(computed(() => [
  { value: 'overview', label: t('customers.tabs.overview') },
  { value: 'details', label: t('customers.tabs.details') },
  { value: 'planning', label: t('customers.tabs.planning') },
]));

// The Details tab already is the form for anyone who may edit, so this is a shortcut to it
// rather than a mode switch — hence hidden once you are there.
const showEdit = computed(() => canManage.value && !!customer.value && tab.value !== 'details');

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
				v-if="showEdit"
				icon="i-lucide-pencil"
				@click="tab = 'details'"
			>
				{{ t('common.actions.edit') }}
			</UButton>

			<UButton
				variant="ghost"
				color="error"
				icon="i-lucide-trash-2"
				@click="() => { showDeleteModal = true }"
			>
				{{ t('common.actions.delete') }}
			</UButton>
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
				:loading-label="t('customers.loadingOne')"
			>
				<template v-if="customer">
					<CustomerOverview v-if="tab === 'overview'" />

					<CustomerDetails
						v-else-if="tab === 'details'"
						:customer="customer"
					/>

					<PlanningEntityShifts
						v-else
						:customer-id="id"
					/>

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
