<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'guest-client',
})

const { register, loading } = useAuth()

const form = reactive({
  name: '',
  email: '',
  password: '',
})

const error = ref<string | null>(null)

async function onSubmit() {
  error.value = null

  const result = await register(form.name, form.email, form.password)
  if (result.success) {
    await navigateTo('/auth/login')
    return
  }

  error.value = result.message || 'Registration failed.'
}
</script>

<template>
  <UCard>
    <template #header>
      <div class="space-y-1">
        <h1 class="text-xl font-semibold">Register</h1>
        <p class="text-sm text-muted">
          Maak een account aan om te starten.
        </p>
      </div>
    </template>

    <form class="space-y-4" @submit.prevent="onSubmit">
      <div class="space-y-1">
        <label class="text-sm font-medium">Naam</label>
        <input
          v-model="form.name"
          type="text"
          autocomplete="name"
          class="w-full rounded-(--ui-radius) border border-default bg-default px-3 py-2 text-sm"
          required
        />
      </div>

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
          autocomplete="new-password"
          class="w-full rounded-(--ui-radius) border border-default bg-default px-3 py-2 text-sm"
          required
        />
      </div>

      <p v-if="error" class="text-sm text-red-600">
        {{ error }}
      </p>

      <UButton type="submit" class="w-full" :loading="loading">
        Register
      </UButton>
    </form>

    <template #footer>
      <p class="text-sm text-muted">
        Al een account?
        <NuxtLink to="/auth/login" class="text-primary font-medium">
          Login
        </NuxtLink>
      </p>
    </template>
  </UCard>
</template>

