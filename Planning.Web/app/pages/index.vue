<script setup lang="ts">
definePageMeta({ layout: 'auth' })

const auth = useAuthStore()

if (auth.isAuthenticated) {
  await navigateTo(auth.hasOrganization ? '/dashboard' : '/organizations/new')
}
</script>

<template>
  <UCard class="w-full lg:max-w-2xl mx-auto">
    <template #header>
      <h1 class="text-2xl font-semibold">
        Planning
      </h1>
      <p class="text-muted text-sm mt-1">
        Plan je team eenvoudig en overzichtelijk.
      </p>
    </template>

    <div class="flex flex-col gap-3">
      <UButton to="/login" block size="lg">
        Inloggen
      </UButton>
      <UButton to="/register" variant="outline" block size="lg">
        Account aanmaken
      </UButton>
      <UButton to="/join" variant="ghost" block>
        Uitnodigingscode invullen
      </UButton>
    </div>
  </UCard>
</template>
