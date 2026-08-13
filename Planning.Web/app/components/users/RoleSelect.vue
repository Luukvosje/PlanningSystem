<script setup lang="ts">
import type { UserResponse, UserRole  } from '~/generated/models';


const props = defineProps<{
  user: UserResponse
}>();

const auth = useAuthStore();
const usersApi = useUsersApi();
const toast = useToast();
const { t } = useI18n();

const { mutate, isPending } = usersApi.useUpdateRoleMutation();

const roleOptions = computed(() =>
  getAssignableRoleOptions(t, auth.currentUser?.userId, {
    id: props.user.id,
    role: props.user.role,
  }),
);

const canEdit = computed(() =>
  canEditUserRole(auth.currentUser?.role, props.user.role),
);

function onRoleChange(role: UserRole) {
  if (!props.user.id || role === props.user.role) {
    return;
  }

  mutate(
    { id: props.user.id, request: { role } },
    {
      onSuccess: async () => {
        if (props.user.id === auth.currentUser?.userId) {
          await auth.fetchMe();
        }

        toast.add({ title: t('users.roleUpdated'), color: 'success' });
      },
      onError: (error) => {
        const { message } = useApiError(error);
        toast.add({ title: message.value, color: 'error' });
      },
    },
  );
}
</script>

<template>
	<USelect
		v-if="canEdit"
		:model-value="user.role"
		:items="roleOptions"
		:loading="isPending"
		class="w-40"
		@update:model-value="onRoleChange"
	/>
	<UBadge
		v-else
		variant="subtle"
	>
		{{ getRoleLabel(user.role, t) }}
	</UBadge>
</template>
