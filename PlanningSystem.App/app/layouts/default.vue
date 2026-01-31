<script setup lang="ts">
const { user, isAuthenticated, loading, logout } = useAuth()

async function onLogout() {
  await logout()
  await navigateTo('/auth/login')
}
</script>

<template>
  <div class="min-h-screen flex flex-col">
    <UHeader>
      <template #title>
        <NuxtLink to="/" class="flex items-center gap-3">
          <AppLogo class="w-auto h-6 shrink-0" />
          <span class="font-semibold">PlanningSystem</span>
        </NuxtLink>
      </template>

      <template #right>
        <UColorModeButton />

        <template v-if="isAuthenticated">
          <span class="hidden sm:inline text-sm text-muted">
            {{ user?.name }}
          </span>

          <UButton
            color="neutral"
            variant="soft"
            :loading="loading"
            @click="onLogout"
          >
            Logout
          </UButton>
        </template>

        <template v-else>
          <UButton to="/auth/login" color="neutral" variant="ghost">
            Login
          </UButton>
          <UButton to="/auth/register">
            Register
          </UButton>
        </template>
      </template>
    </UHeader>

    <UMain class="flex-1">
      <slot />
    </UMain>

    <USeparator icon="i-simple-icons-nuxtdotjs" />

    <UFooter>
      <template #left>
        <p class="text-sm text-muted">
          Copyright © {{ new Date().getFullYear() }}
        </p>
      </template>

      <template #right>
        <UButton
          to="https://github.com/nuxt/ui"
          target="_blank"
          icon="i-simple-icons-github"
          aria-label="GitHub"
          color="neutral"
          variant="ghost"
        />
      </template>
    </UFooter>
  </div>
</template>

