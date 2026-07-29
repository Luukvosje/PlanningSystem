<script setup lang="ts">
import type { TableColumn, TableRow } from '@nuxt/ui'
import type { UserResponse } from '~/generated/models'

definePageMeta({ layout: 'default' })

const auth = useAuthStore()
const inviteCreate = useInviteCreate()

onMounted(async () => {
  await auth.fetchMe()

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard')
  }
})

const { data: users, isLoading, error } = useUsers()
const { message } = useApiError(error)

const canInvite = computed(() => canManageInvites(auth.currentUser?.role))

const globalFilter = ref('')

const columns: TableColumn<UserResponse>[] = [
  {
    id: 'name',
    header: 'Naam',
    accessorFn: row => `${row.firstName ?? ''} ${row.lastName ?? ''}`.trim(),
  },
  {
    accessorKey: 'email',
    header: 'E-mail',
  },
  {
    id: 'role',
    accessorKey: 'role',
    header: 'Rol',
  },
  {
    id: 'status',
    header: 'Status',
    accessorFn: row => (row.isActive ? 'Actief' : 'Inactief'),
  },
]

function matchesUserFilter(user: UserResponse, filter: string) {
  const query = filter.toLowerCase().trim()
  if (!query) {
    return true
  }

  const haystack = [
    user.firstName,
    user.lastName,
    user.email,
    getRoleLabel(user.role),
    user.isActive ? 'actief' : 'inactief',
  ]
    .filter(Boolean)
    .join(' ')
    .toLowerCase()

  return haystack.includes(query)
}

function onRowSelect(_event: Event, row: TableRow<UserResponse>) {
  const id = row.original.id
  if (id) {
    navigateTo(`/users/${id}`)
  }
}
</script>

<template>
  <LayoutPageContainer>
    <LayoutPageHeader
      title="Team"
      :subtitle="auth.currentUser?.organizationName"
    >
      <template #actions>
        <UButton v-if="canInvite" icon="i-lucide-user-plus" @click="inviteCreate.open()">
          Uitnodigen
        </UButton>
      </template>
    </LayoutPageHeader>

    <UAlert v-if="error" color="error" :title="message" />

    <UiLoadingIndicator v-else-if="isLoading" label="Team laden..." />

    <div v-else-if="users?.length || globalFilter" class="space-y-4">
      <UInput
        v-model="globalFilter"
        icon="i-lucide-search"
        placeholder="Zoeken op naam, e-mail, rol of status..."
        class="max-w-md"
      />

      <UTable
        v-model:global-filter="globalFilter"
        :data="users ?? []"
        :columns="columns"
        :global-filter-options="{
          globalFilterFn: (row, _columnId, filterValue) => matchesUserFilter(row.original, filterValue),
        }"
        empty="Geen teamleden gevonden."
        class="flex-1"
        @select="onRowSelect"
      >
        <template #role-cell="{ row }">
          <div @click.stop>
            <UsersRoleSelect :user="row.original" />
          </div>
        </template>

        <template #status-cell="{ row }">
          <UBadge :color="row.original.isActive ? 'success' : 'neutral'" variant="subtle">
            {{ row.original.isActive ? 'Actief' : 'Inactief' }}
          </UBadge>
        </template>
      </UTable>
    </div>

    <UCard v-else>
      <p class="text-muted text-center py-4">
        Nog geen teamleden. Nodig iemand uit via een code.
      </p>
      <div v-if="canInvite" class="flex justify-center">
        <UButton @click="inviteCreate.open()">
          Uitnodigen
        </UButton>
      </div>
    </UCard>
  </LayoutPageContainer>
</template>
