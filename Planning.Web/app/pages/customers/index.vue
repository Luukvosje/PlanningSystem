<script setup lang="ts">
import type { TableColumn, TableRow } from '@nuxt/ui'
import type { CustomerResponse } from '~/generated/models'

definePageMeta({ layout: 'default' })

const auth = useAuthStore()
const customerCreate = useCustomerCreate()

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
  <LayoutPageContainer>
    <LayoutPageHeader
      title="Klanten"
      :subtitle="auth.currentUser?.organizationName"
    >
      <template #actions>
        <UButton icon="i-lucide-plus" @click="customerCreate.open()">
          Nieuwe klant
        </UButton>
      </template>
    </LayoutPageHeader>

    <UAlert v-if="error" color="error" :title="message" />

    <div v-else-if="isLoading || customers?.length || globalFilter" class="space-y-4">
      <UInput
        v-model="globalFilter"
        icon="i-lucide-search"
        placeholder="Zoeken op naam, e-mail of adres..."
        class="max-w-md"
      />

      <UTable
        v-model:global-filter="globalFilter"
        :data="customers ?? []"
        :columns="columns"
        :loading="isLoading"
        :global-filter-options="{
          globalFilterFn: (row, _columnId, filterValue) => matchesCustomerFilter(row.original, filterValue),
        }"
        empty="Geen klanten gevonden."
        class="flex-1"
        @select="onRowSelect"
      />
    </div>

    <UCard v-else>
      <p class="text-muted text-center py-4">
        Nog geen klanten. Voeg je eerste klant toe.
      </p>
      <div class="flex justify-center">
        <UButton @click="customerCreate.open()">
          Klant toevoegen
        </UButton>
      </div>
    </UCard>
  </LayoutPageContainer>
</template>
