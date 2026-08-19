<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';
import { queryKeys } from '~/utils/queryKeys';

definePageMeta({ layout: 'default' });

const route = useRoute();
const router = useRouter();
const id = computed(() => route.params.id as string);

const auth = useAuthStore();
const customersApi = useCustomersApi();
const queryClient = useQueryClient();
const toast = useToast();
const customerEdit = useCustomerEdit();
const canManage = computed(() => canManageCustomers(auth.currentUser?.role));
const { t } = useI18n();

onMounted(async () => {
  await auth.fetchMe();

  if (!auth.hasOrganization) {
    await navigateTo('/dashboard');
  }
});

const { data: customer, isLoading, error } = useCustomer(id);
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
	<LayoutPageContainer>
		<LayoutPageHeader>
			<UButton
				to="/customers"
				variant="ghost"
				color="neutral"
				icon="i-lucide-arrow-left"
			>
				{{ t('customers.backToList') }}
			</UButton>

			<template
				v-if="canManage && customer"
				#actions
			>
				<UButton
					variant="outline"
					color="neutral"
					icon="i-lucide-pencil"
					@click="customerEdit.open(customer)"
				>
					{{ t('common.actions.edit') }}
				</UButton>
				<UButton
					variant="outline"
					color="error"
					icon="i-lucide-trash-2"
					@click="() => { showDeleteModal = true }"
				>
					{{ t('common.actions.delete') }}
				</UButton>
			</template>
		</LayoutPageHeader>

		<UiQueryState
			:error="error"
			:loading="isLoading"
			:loading-label="t('customers.loadingOne')"
		>
			<template v-if="customer">
				<CustomerCard :customer="customer" />

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
</template>
