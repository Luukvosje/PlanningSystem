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

const isEditing = ref(false);
const customerForm = useCustomerForm(customer, {
  onSaved: () => {
    isEditing.value = false;
  },
});

function cancelEdit() {
  customerForm.discard();
  isEditing.value = false;
}

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
				v-if="!isEditing"
				variant="outline"
				color="neutral"
				icon="i-lucide-pencil"
				@click="() => { isEditing = true }"
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

		<LayoutPageContainer>
			<UiQueryState
				:error="error"
				:loading="isLoading"
				:loading-label="t('customers.loadingOne')"
			>
				<template v-if="customer">
						<CustomerCard :customer="customer">
							<template
								v-if="isEditing"
								#default
							>
								<component
									:is="customerForm.render"
								>
									<template #footer>
										<div class="flex items-center justify-end gap-2 pt-4">
											<UButton
												type="button"
												variant="outline"
												color="neutral"
												:disabled="customerForm.isSubmitting.value"
												@click="cancelEdit"
											>
												{{ t('common.actions.cancel') }}
											</UButton>
											<UButton
												type="submit"
												:loading="customerForm.isSubmitting.value"
											>
												{{ t('common.actions.save') }}
											</UButton>
										</div>
									</template>
								</component>
							</template>
						</CustomerCard>

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
