<script setup lang="ts">
import type { UserResponse } from '~/generated/models';

defineProps<{
  user: UserResponse
}>();

const auth = useAuthStore();
const canEditAvailability = computed(() => canManagePlanning(auth.currentUser?.role));
const { t } = useI18n();
</script>

<template>
	<LayoutSection
		:title="`${user.firstName} ${user.lastName}`"
		:description="user.email"
	>
		<div class="flex flex-wrap items-center gap-2">
			<UsersRoleSelect :user="user" />
			<UBadge
				:color="user.isActive ? 'success' : 'neutral'"
				variant="subtle"
			>
				{{ user.isActive ? t('users.active') : t('users.inactive') }}
			</UBadge>
			<UButton
				v-if="canEditAvailability"
				size="sm"
				variant="outline"
				icon="i-lucide-calendar-clock"
				:label="t('nav.availability')"
				:to="`/beschikbaarheid?userId=${user.id}`"
			/>
		</div>
	</LayoutSection>
</template>
