<script setup lang="ts">
import type { TableColumn, TableRow } from '@nuxt/ui';
import type { CustomerResponse } from '~/generated/models';

definePageMeta({ layout: false });

const auth = useAuthStore();
const customerCreate = useCustomerCreate();
const canManage = computed(() => canManageCustomers(auth.currentUser?.role));
const { t } = useI18n();

onMounted(async () => {
  await auth.fetchMe();

  if (!auth.hasOrganization) {
    await navigateTo('/dashboard');
  }
});

const { data: customers, isLoading, error } = useCustomers();

const globalFilter = ref('');

const columns = computed<TableColumn<CustomerResponse>[]>(() => [
  {
    accessorKey: 'name',
    header: t('customers.fields.name'),
  },
  {
    accessorKey: 'email',
    header: t('customers.fields.email'),
  },
  {
    accessorKey: 'address',
    header: t('customers.fields.address'),
  },
]);

function matchesCustomerFilter(customer: CustomerResponse, filter: string) {
  const query = filter.toLowerCase().trim();
  if (!query) {
    return true;
  }

  const haystack = [customer.name, customer.email, customer.address]
    .filter(Boolean)
    .join(' ')
    .toLowerCase();

  return haystack.includes(query);
}

function openCustomer(customer: CustomerResponse) {
  if (customer.id) {
    navigateTo(`/customers/${customer.id}`);
  }
}
</script>

<template>
	<NuxtLayout name="default">
		<template #actions>
			<UButton
				v-if="canManage"
				icon="i-lucide-plus"
				@click="customerCreate.open()"
			>
				{{ t('customers.create.title') }}
			</UButton>
		</template>

		<LayoutPageContainer fill>
			<UiQueryState
				:error="error"
				:loading="isLoading"
				:loading-label="t('customers.loading')"
			>
				<div class="flex min-h-0 flex-1 flex-col gap-4">
					<UInput
						v-model="globalFilter"
						icon="i-lucide-search"
						:placeholder="t('customers.searchPlaceholder')"
						class="max-w-md shrink-0"
					/>

					<UiDataTable
						v-model:search="globalFilter"
						:rows="customers"
						:columns="columns"
						:filter-fn="matchesCustomerFilter"
						:empty-title="canManage ? t('customers.emptyManage') : t('customers.empty')"
						:no-results-title="t('customers.noResults')"
						@select="openCustomer"
					>
						<template #name-cell="{ row }: { row: TableRow<CustomerResponse> }">
							<div class="flex items-center gap-3">
								<UAvatar
									:text="initialsFromName(row.original.name)"
									size="sm"
									class="shrink-0"
									:style="{ backgroundColor: row.original.color, color: readableTextColor(row.original.color) }"
								/>
								<span class="font-medium">{{ row.original.name }}</span>
							</div>
						</template>

						<template #empty-action>
							<UButton
								v-if="canManage"
								@click="customerCreate.open()"
							>
								{{ t('customers.add') }}
							</UButton>
						</template>
					</UiDataTable>
				</div>
			</UiQueryState>
		</LayoutPageContainer>
	</NuxtLayout>
</template>
