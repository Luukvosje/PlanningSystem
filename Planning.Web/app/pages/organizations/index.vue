<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query'

definePageMeta({ layout: 'default' })

const auth = useAuthStore()
const toast = useToast()
const queryClient = useQueryClient()
const organizationCreate = useOrganizationCreate()

onMounted(async () => {
  await auth.fetchMe()

  if (!canManageOrganization(auth.currentUser?.role)) {
    await navigateTo('/dashboard')
  }
})

const { data: organizations, isLoading, error } = useMyOrganizations()
const { message } = useApiError(error)

async function switchTo(orgId: string | undefined) {
  if (!orgId || orgId === auth.organizationId) {
    return
  }

  try {
    auth.selectOrganization(orgId)
    await invalidateOrgScopedQueries(queryClient)
    toast.add({ title: 'Organisatie gewisseld', color: 'success' })
  }
  catch (err) {
    const { message: errMsg } = useApiError(err)
    toast.add({ title: errMsg.value, color: 'error' })
  }
}
</script>

<template>
  <PageContainer>
    <PageHeader
      title="Mijn organisaties"
      subtitle="Beheer en wissel tussen je organisaties."
    >
      <template #actions>
        <UButton icon="i-lucide-plus" @click="organizationCreate.open()">
          Nieuwe organisatie
        </UButton>
      </template>
    </PageHeader>

    <div v-if="isLoading" class="space-y-3">
      <USkeleton class="h-16 w-full" />
      <USkeleton class="h-16 w-full" />
    </div>

    <UAlert v-else-if="error" color="error" :title="message" />

    <div v-else-if="organizations?.length" class="space-y-3">
      <UCard
        v-for="org in organizations"
        :key="org.organizationId"
        :class="org.organizationId === auth.organizationId ? 'ring-2 ring-primary' : ''"
      >
        <div class="flex items-center justify-between">
          <div>
            <p class="font-medium">
              {{ org.organizationName }}
            </p>
            <UBadge variant="subtle" class="mt-1">
              {{ getRoleLabel(org.role) }}
            </UBadge>
            <UBadge
              v-if="org.organizationId === auth.organizationId"
              color="primary"
              variant="subtle"
              class="mt-1 ml-2"
            >
              Huidig
            </UBadge>
          </div>
          <UButton
            v-if="org.organizationId !== auth.organizationId"
            variant="outline"
            size="sm"
            @click="() => { switchTo(org.organizationId) }"
          >
            Wisselen
          </UButton>
        </div>
      </UCard>
    </div>

    <UCard v-else>
      <p class="text-muted text-center py-4">
        Je bent nog geen lid van een organisatie.
      </p>
      <div class="flex justify-center gap-2 mt-2">
        <UButton to="/organizations/new">
          Organisatie aanmaken
        </UButton>
        <UButton to="/join" variant="outline">
          Code invullen
        </UButton>
      </div>
    </UCard>

    <OrganizationsModulesCard
      v-if="canManageOrganization(auth.currentUser?.role)"
      class="mt-6"
    />
  </PageContainer>
</template>
