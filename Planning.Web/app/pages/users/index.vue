<script setup lang="ts">
import type { TableColumn, TableRow } from '@nuxt/ui';
import type { UserResponse } from '~/generated/models';

definePageMeta({ layout: 'default' });

const auth = useAuthStore();
const inviteCreate = useInviteCreate();
const { t } = useI18n();

onMounted(async () => {
  await auth.fetchMe();

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard');
  }
});

const { data: users, isLoading, error } = useUsers();
const { message } = useApiError(error);

const canInvite = computed(() => canManageInvites(auth.currentUser?.role));

const globalFilter = ref('');

const columns = computed<TableColumn<UserResponse>[]>(() => [
  {
    id: 'name',
    header: t('users.columns.name'),
    accessorFn: (row) => `${row.firstName ?? ''} ${row.lastName ?? ''}`.trim(),
  },
  {
    accessorKey: 'email',
    header: t('users.columns.email'),
  },
  {
    id: 'role',
    accessorKey: 'role',
    header: t('users.columns.role'),
  },
  {
    id: 'status',
    header: t('users.columns.status'),
    accessorFn: (row) => (row.isActive ? t('users.active') : t('users.inactive')),
  },
]);

function matchesUserFilter(user: UserResponse, filter: string) {
  const query = filter.toLowerCase().trim();
  if (!query) {
    return true;
  }

  const haystack = [
    user.firstName,
    user.lastName,
    user.email,
    getRoleLabel(user.role, t),
    user.isActive ? t('users.active') : t('users.inactive'),
  ]
    .filter(Boolean)
    .join(' ')
    .toLowerCase();

  return haystack.includes(query);
}

function onRowSelect(_event: Event, row: TableRow<UserResponse>) {
  const id = row.original.id;
  if (id) {
    navigateTo(`/users/${id}`);
  }
}
</script>

<template>
	<LayoutPageContainer fill>
		<LayoutPageHeader
			class="shrink-0"
			:title="t('nav.team')"
			:subtitle="auth.currentUser?.organizationName"
		>
			<template #actions>
				<UButton
					v-if="canInvite"
					icon="i-lucide-user-plus"
					@click="inviteCreate.open()"
				>
					{{ t('nav.invites') }}
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
			:label="t('users.loading')"
		/>

		<div
			v-else
			class="flex min-h-0 flex-1 flex-col gap-4"
		>
			<UInput
				v-model="globalFilter"
				icon="i-lucide-search"
				:placeholder="t('users.searchPlaceholder')"
				class="max-w-md shrink-0"
			/>

			<div class="min-h-0 flex-1 overflow-hidden rounded-xl border border-default">
				<UTable
					v-model:global-filter="globalFilter"
					:data="users ?? []"
					:columns="columns"
					:global-filter-options="{
						globalFilterFn: (row, _columnId, filterValue) => matchesUserFilter(row.original, filterValue),
					}"
					sticky
					class="h-full"
					@select="onRowSelect"
				>
					<template #empty>
						<div class="flex h-full min-h-48 flex-col items-center justify-center gap-3 py-6">
							<template v-if="!(users?.length) && !globalFilter">
								<p class="text-muted text-sm">
									{{ t('users.empty') }}
								</p>
								<UButton
									v-if="canInvite"
									@click="inviteCreate.open()"
								>
									{{ t('nav.invites') }}
								</UButton>
							</template>
							<p
								v-else
								class="text-muted text-sm"
							>
								{{ t('users.noResults') }}
							</p>
						</div>
					</template>

					<template #role-cell="{ row }">
						<div @click.stop>
							<UsersRoleSelect :user="row.original" />
						</div>
					</template>

					<template #status-cell="{ row }">
						<UBadge
							:color="row.original.isActive ? 'success' : 'neutral'"
							variant="subtle"
						>
							{{ row.original.isActive ? t('users.active') : t('users.inactive') }}
						</UBadge>
					</template>
				</UTable>
			</div>
		</div>
	</LayoutPageContainer>
</template>
