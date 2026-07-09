<script setup lang="ts">
definePageMeta({ layout: 'default' })

const auth = useAuthStore()
const route = useRoute()

const canManageOthers = computed(() => canManagePlanning(auth.currentUser?.role))

const targetUserId = computed(() => {
  const queryUserId = route.query.userId
  if (typeof queryUserId === 'string' && queryUserId && canManageOthers.value) {
    return queryUserId
  }
  return auth.currentUser?.userId ?? ''
})
</script>

<template>
  <LayoutPageContainer>
    <div class="space-y-2 mb-6">
      <h1 class="text-2xl font-semibold">
        Beschikbaarheid
      </h1>
      <p class="text-muted text-sm">
        Geef aan wanneer je wel of niet beschikbaar bent. Planners zien dit in de weekplanning.
      </p>
    </div>

    <AvailabilityWeekEditor
      v-if="targetUserId"
      :user-id="targetUserId"
      :can-select-user="canManageOthers"
    />
  </LayoutPageContainer>
</template>
