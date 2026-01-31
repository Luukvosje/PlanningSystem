<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'guest-client',
})

const { login, loading } = useAuth()

const form = reactive({
  email: '',
  password: '',
})

const error = ref<string | null>(null)

async function onSubmit() {
  error.value = null

  const result = await login(form.email, form.password)
  if (result.success) {
    await navigateTo('/')
    return
  }

  error.value = result.message || 'Login failed.'
}
</script>

<template>
  <UCard>
    <template #header>
      <div class="space-y-1">
        <h1 class="text-xl font-semibold">Login</h1>
        <p class="text-sm text-muted">
          Log in om je dashboard te openen.
        </p>
      </div>
    </template>

    <form class="space-y-4" @submit.prevent="onSubmit">
      <div class="space-y-1">
        <label class="text-sm font-medium">Email</label>
        <input
          v-model="form.email"
          type="email"
          autocomplete="email"
          class="w-full rounded-(--ui-radius) border border-default bg-default px-3 py-2 text-sm"
          required
        />
      </div>

      <div class="space-y-1">
        <label class="text-sm font-medium">Wachtwoord</label>
        <input
          v-model="form.password"
          type="password"
          autocomplete="current-password"
          class="w-full rounded-(--ui-radius) border border-default bg-default px-3 py-2 text-sm"
          required
        />
      </div>

      <p v-if="error" class="text-sm text-red-600">
        {{ error }}
      </p>

      <UButton type="submit" class="w-full" :loading="loading">
        Login
      </UButton>
    </form>

    <template #footer>
      <p class="text-sm text-muted">
        Geen account?
        <NuxtLink to="/auth/register" class="text-primary font-medium">
          Registreer
        </NuxtLink>
      </p>
    </template>
  </UCard>
</template>

