<script setup lang="ts">
definePageMeta({ layout: 'default' })

const auth = useAuthStore()

onMounted(() => auth.fetchMe())

const { data: users, isLoading } = useUsers()

const teamCount = computed(() => users.value?.length ?? 0)
const organizationName = computed(() => auth.currentUser?.organizationName ?? 'Onbekend')
const roleLabel = computed(() => getRoleLabel(auth.currentUser?.role))
</script>

<template>
  <LayoutPageContainer>
    <LayoutPageHeader
      title="Dashboard"
      :subtitle="`Overzicht van ${organizationName}`"
    />

    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <UCard :ui="{ body: 'p-6' }">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs font-semibold uppercase tracking-wider text-muted">
              Organisatie
            </p>
            <p class="mt-2 text-lg font-semibold">
              {{ organizationName }}
            </p>
          </div>
          <UIcon name="i-lucide-building-2" class="size-5 text-secondary" />
        </div>
      </UCard>

      <UCard :ui="{ body: 'p-6' }">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs font-semibold uppercase tracking-wider text-muted">
              Teamleden
            </p>
            <p class="mt-2 text-lg font-semibold">
              <USkeleton v-if="isLoading" class="h-7 w-12" />
              <span v-else>{{ teamCount }}</span>
            </p>
          </div>
          <UIcon name="i-lucide-users" class="size-5 text-secondary" />
        </div>
      </UCard>

      <UCard :ui="{ body: 'p-6' }">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs font-semibold uppercase tracking-wider text-muted">
              Jouw rol
            </p>
            <p class="mt-2 text-lg font-semibold">
              {{ roleLabel }}
            </p>
          </div>
          <UIcon name="i-lucide-shield" class="size-5 text-secondary" />
        </div>
      </UCard>
    </div>

    <div class="grid gap-4 sm:grid-cols-2">
      <UCard>
        <template #header>
          <h2 class="font-semibold">
            Snel naar
          </h2>
        </template>

        <div class="flex flex-col gap-2">
          <UButton to="/planning" variant="outline" icon="i-lucide-calendar-range" block>
            Bekijk planning
          </UButton>
          <UButton to="/users" variant="outline" icon="i-lucide-users" block>
            Team bekijken
          </UButton>
        </div>
      </UCard>

      <UCard>
        <template #header>
          <h2 class="font-semibold">
            Vandaag
          </h2>
        </template>

        <p class="text-sm text-muted">
          Welkom terug. Gebruik de planning om taken voor deze week te bekijken en beheren.
        </p>
      </UCard>
    </div>
  </LayoutPageContainer>
</template>
