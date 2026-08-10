<script setup lang="ts">
import type { TableColumn, TableRow } from '@nuxt/ui'
import type { CustomerResponse } from '~/generated/models'

definePageMeta({ layout: 'default' })

const auth = useAuthStore()
const customerCreate = useCustomerCreate()
const canManage = computed(() => canManageCustomers(auth.currentUser?.role))

onMounted(async () => {
  await auth.fetchMe()

  if (!auth.hasOrganization) {
    await navigateTo('/dashboard')
  }
})

const { data: customers, isLoading, error } = useCustomers()
const { message } = useApiError(error)

const globalFilter = ref('')

const columns: TableColumn<CustomerResponse>[] = [
  {
    accessorKey: 'name',
    header: 'Naam',
  },
  {
    accessorKey: 'email',
    header: 'E-mail',
  },
  {
    accessorKey: 'address',
    header: 'Adres',
  },
]

function matchesCustomerFilter(customer: CustomerResponse, filter: string) {
  const query = filter.toLowerCase().trim()
  if (!query) {
    return true
  }

  const haystack = [customer.name, customer.email, customer.address]
    .filter(Boolean)
    .join(' ')
    .toLowerCase()

  return haystack.includes(query)
}

function onRowSelect(_event: Event, row: TableRow<CustomerResponse>) {
  const id = row.original.id
  if (id) {
    navigateTo(`/customers/${id}`)
  }
}
</script>

<template>
  <LayoutPageContainer fill>
    <LayoutPageHeader
      class="shrink-0"
      title="Klanten"
      :subtitle="auth.currentUser?.organizationName"
    >
      <template v-if="canManage" #actions>
        <UButton icon="i-lucide-plus" @click="customerCreate.open()">
          Nieuwe klant
        </UButton>
      </template>
    </LayoutPageHeader>

    <UAlert v-if="error" class="shrink-0" color="error" :title="message" />

    <UiLoadingIndicator v-else-if="isLoading" class="shrink-0" label="Klanten laden..." />

    <div v-else class="flex min-h-0 flex-1 flex-col gap-4">
      <UInput
        v-model="globalFilter"
        icon="i-lucide-search"
        placeholder="Zoeken op naam, e-mail of adres..."
        class="max-w-md shrink-0"
      />

      <UTable
        v-model:global-filter="globalFilter"
        :data="customers ?? []"
        :columns="columns"
        :global-filter-options="{
          globalFilterFn: (row, _columnId, filterValue) => matchesCustomerFilter(row.original, filterValue),
        }"
        sticky
        class="min-h-0 flex-1"
        @select="onRowSelect"
      >
        <template #empty>
          <div class="flex h-full min-h-48 flex-col items-center justify-center gap-3 py-6">
            <template v-if="!(customers?.length) && !globalFilter">
              <p class="text-muted text-sm">
                {{ canManage ? 'Nog geen klanten. Voeg je eerste klant toe.' : 'Nog geen klanten.' }}
              </p>
              <UButton v-if="canManage" @click="customerCreate.open()">
                Klant toevoegen
              </UButton>
            </template>
            <p v-else class="text-muted text-sm">
              Geen klanten gevonden.
            </p>
          </div>
        </template>
      </UTable>
    </div>
  </LayoutPageContainer>
</template>
