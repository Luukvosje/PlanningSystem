<script setup lang="ts">
import type { TableColumn, TableRow } from '@nuxt/ui';
import type { CustomerResponse } from '~/generated/models';

definePageMeta({ layout: 'default' });

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
const { message } = useApiError(error);

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

function onRowSelect(_event: Event, row: TableRow<CustomerResponse>) {
  const id = row.original.id;
  if (id) {
    navigateTo(`/customers/${id}`);
  }
}
</script>

<template>
	<LayoutPageContainer fill>
		<LayoutPageHeader
			class="shrink-0"
			:title="t('nav.customers')"
			:subtitle="auth.currentUser?.organizationName"
		>
			<template
				v-if="canManage"
				#actions
			>
				<UButton
					icon="i-lucide-plus"
					@click="customerCreate.open()"
				>
					{{ t('customers.create.title') }}
				</UButton>
			</template>
		</LayoutPageHeader>

		<UAlert
			v-if="error"
			class="shrink-0"
			color="error"
			:title="message"
		/>

		<UiLoadingIndicator
			v-else-if="isLoading"
			class="shrink-0"
			:label="t('customers.loading')"
		/>

		<div
			v-else
			class="flex min-h-0 flex-1 flex-col gap-4"
		>
			<UInput
				v-model="globalFilter"
				icon="i-lucide-search"
				:placeholder="t('customers.searchPlaceholder')"
				class="max-w-md shrink-0"
			/>

			<div class="min-h-0 flex-1 overflow-hidden rounded-xl border border-default">
				<UTable
					v-model:global-filter="globalFilter"
					:data="customers ?? []"
					:columns="columns"
					:global-filter-options="{
						globalFilterFn: (row, _columnId, filterValue) => matchesCustomerFilter(row.original, filterValue),
					}"
					sticky
					class="h-full"
					@select="onRowSelect"
				>
					<template #empty>
						<div class="flex h-full min-h-48 flex-col items-center justify-center gap-3 py-6">
							<template v-if="!(customers?.length) && !globalFilter">
								<p class="text-muted text-sm">
									{{ canManage ? t('customers.emptyManage') : t('customers.empty') }}
								</p>
								<UButton
									v-if="canManage"
									@click="customerCreate.open()"
								>
									{{ t('customers.add') }}
								</UButton>
							</template>
							<p
								v-else
								class="text-muted text-sm"
							>
								{{ t('customers.noResults') }}
							</p>
						</div>
					</template>
				</UTable>
			</div>
		</div>
	</LayoutPageContainer>
</template>
