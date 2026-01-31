<script setup lang="ts">
import type { Organization } from '~/typescript/types'

definePageMeta({
  middleware: 'auth-client',
})

const { $serviceFactory: sf} = useNuxtApp()

const organizations = ref<Organization[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  loading.value = true
  error.value = null

  const result = await sf.organizationService.getUserOrganizations()
  if (result.success) {
    organizations.value = result.data ?? []
  } else {
    organizations.value = []
    error.value = result.message || 'Failed to load organizations.'
  }

  loading.value = false
})
</script>

<template>
  <div class="p-6 space-y-6">
    <div>
      <h1 class="text-2xl font-semibold">
        Dashboard
      </h1>
      <p class="text-sm text-muted">
        Jouw organisaties
      </p>
    </div>

    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <span class="font-medium">Organisaties</span>
          <span class="text-sm text-muted">
            {{ organizations.length }}
          </span>
        </div>
      </template>

      <div v-if="loading" class="text-sm text-muted">
        Laden...
      </div>

      <div v-else-if="error" class="text-sm text-red-600">
        {{ error }}
      </div>

      <div v-else>
        <div v-if="organizations.length === 0" class="text-sm text-muted">
          Geen organisaties gevonden.
        </div>

        <ul v-else class="divide-y divide-default">
          <li
            v-for="org in organizations"
            :key="org.id"
            class="py-3 flex items-center justify-between"
          >
            <span class="font-medium">
              {{ org.name }}
            </span>
            <span class="text-sm text-muted">
              #{{ org.id }}
            </span>
          </li>
        </ul>
      </div>
    </UCard>
  </div>
</template>
