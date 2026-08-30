<script setup lang="ts">
import type { UserResponse } from '~/generated/models';

const props = defineProps<{
  user: UserResponse
}>();

const auth = useAuthStore();
const canEditAvailability = computed(() => canManagePlanning(auth.currentUser?.role));
const canEdit = computed(() => canEditUserRole(auth.currentUser?.role, props.user.role));
const fields = useUserFields();
const userEdit = useUserEdit();
const { t } = useI18n();

const values = computed(() => ({
  firstName: props.user.firstName,
  lastName: props.user.lastName,
  email: props.user.email,
}));
</script>

<template>
	<LayoutCard :title="`${user.firstName} ${user.lastName}`">
		<template #actions>
			<div class="flex flex-wrap items-center gap-2">
				<UsersStatusToggle :user="user" />
				<UButton
					v-if="canEditAvailability"
					size="sm"
					variant="outline"
					icon="i-lucide-calendar-clock"
					:label="t('nav.availability')"
					:to="`/beschikbaarheid?userId=${user.id}`"
				/>
			</div>
		</template>

		<FormDisplay
			:controls="fields"
			:values="values"
		/>
	</LayoutCard>

	<FormEditableSection
		:key="user.id"
		:edit="userEdit"
		:entity="user"
		:title="t('users.access.title')"
		:description="t('users.access.description')"
		:can-edit="canEdit"
	/>
</template>
