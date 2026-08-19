<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui';
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

function openUser(user: UserResponse) {
  if (user.id) {
    navigateTo(`/users/${user.id}`);
  }
}
</script>

<template>
	<LayoutPageContainer fill>
		<LayoutPageHeader
			class="shrink-0"
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

		<UiQueryState
			:error="error"
			:loading="isLoading"
			:loading-label="t('users.loading')"
		>
			<div class="flex min-h-0 flex-1 flex-col gap-4">
				<UInput
					v-model="globalFilter"
					icon="i-lucide-search"
					:placeholder="t('users.searchPlaceholder')"
					class="max-w-md shrink-0"
				/>

				<UiDataTable
					v-model:search="globalFilter"
					:rows="users"
					:columns="columns"
					:filter-fn="matchesUserFilter"
					:empty-title="canInvite ? t('users.emptyManage') : t('users.empty')"
					:no-results-title="t('users.noResults')"
					@select="openUser"
				>
					<template #empty-action>
						<UButton
							v-if="canInvite"
							@click="inviteCreate.open()"
						>
							{{ t('nav.invites') }}
						</UButton>
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
				</UiDataTable>
			</div>
		</UiQueryState>
	</LayoutPageContainer>
</template>
