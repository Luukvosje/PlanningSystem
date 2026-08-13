<script setup lang="ts">
import type { UserResponse } from '~/generated/models';

defineProps<{
  user: UserResponse
}>();

const auth = useAuthStore();
const _canManage = computed(() => canManageOrganization(auth.currentUser?.role));
const canEditAvailability = computed(() => canManagePlanning(auth.currentUser?.role));
const { t } = useI18n();
</script>

<template>
	<div class="space-y-4">
		<UCard>
			<div class="space-y-2">
				<h2 class="text-xl font-semibold">
					{{ user.firstName }} {{ user.lastName }}
				</h2>
				<p class="text-muted">
					{{ user.email }}
				</p>
				<div class="flex flex-wrap gap-2 pt-2 items-center">
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
			</div>
		</UCard>

		<!-- <UsersModulesCard v-if="canManage" :user="user" /> -->
	</div>
</template>