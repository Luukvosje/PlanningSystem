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
        Je bent standaard beschikbaar. Voeg alleen blokkades of uitzonderingen toe wanneer je niet kunt werken.
      </p>
    </div>

    <AvailabilityRulesEditor
      v-if="targetUserId"
      :employee-id="targetUserId"
      :can-select-user="canManageOthers"
    />
  </LayoutPageContainer>
</template>
