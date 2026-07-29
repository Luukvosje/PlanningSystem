<script setup lang="ts">
import type { UserResponse } from '~/generated/models'

defineProps<{
  user: UserResponse
}>()

const auth = useAuthStore()
const canManage = computed(() => canManageOrganization(auth.currentUser?.role))
const canEditAvailability = computed(() => canManagePlanning(auth.currentUser?.role))
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
          <UBadge :color="user.isActive ? 'success' : 'neutral'" variant="subtle">
            {{ user.isActive ? 'Actief' : 'Inactief' }}
          </UBadge>
          <UButton
            v-if="canEditAvailability"
            size="sm"
            variant="outline"
            icon="i-lucide-calendar-clock"
            label="Beschikbaarheid"
            :to="`/beschikbaarheid?userId=${user.id}`"
          />
        </div>
      </div>
    </UCard>

    <!-- <UsersModulesCard v-if="canManage" :user="user" /> -->
  </div>
</template>