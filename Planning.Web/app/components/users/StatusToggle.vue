<script setup lang="ts">
import type { UserResponse } from '~/generated/models';

const props = defineProps<{
  user: UserResponse
}>();

const auth = useAuthStore();
const usersApi = useUsersApi();
const toast = useToast();
const { t } = useI18n();

const { mutate, isPending } = usersApi.useUpdateStatusMutation();

const canEdit = computed(() =>
  canEditUserStatus(
    { id: auth.currentUser?.userId, role: auth.currentUser?.role },
    { id: props.user.id, role: props.user.role },
  ),
);

const label = computed(() => (props.user.isActive ? t('users.active') : t('users.inactive')));

function onStatusChange(isActive: boolean) {
  if (!props.user.id || isActive === props.user.isActive) {
    return;
  }

  mutate(
    { id: props.user.id, request: { isActive } },
    {
      onSuccess: () => {
        toast.add({
          title: isActive ? t('users.activated') : t('users.deactivated'),
          color: 'success',
        });
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
	<USwitch
		v-if="canEdit"
		:model-value="user.isActive"
		:label="label"
		:loading="isPending"
		color="success"
		@update:model-value="onStatusChange"
	/>
	<UBadge
		v-else
		:color="user.isActive ? 'success' : 'neutral'"
		variant="subtle"
	>
		{{ label }}
	</UBadge>
</template>
