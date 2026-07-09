<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query'

const auth = useAuthStore()
const toast = useToast()
const queryClient = useQueryClient()
const route = useRoute()

const { data: currentUser } = useCurrentUser()
const { data: organizations } = useMyOrganizations()
const { navigationItems, pageTitle } = useAppNavigation()

watch(currentUser, (user) => {
  if (user) {
    auth.currentUser = user
  }
}, { immediate: true })

const organizationOptions = computed(() =>
  (organizations.value ?? []).map(org => ({
    label: org.organizationName ?? 'Onbekend',
    value: org.organizationId ?? '',
  })),
)

const selectedOrgId = computed({
  get: () => auth.organizationId ?? '',
  set: async (orgId: string) => {
    if (!orgId || orgId === auth.organizationId) {
      return
    }

    try {
      auth.selectOrganization(orgId)
      await invalidateOrgScopedQueries(queryClient)
      toast.add({ title: 'Organisatie gewisseld', color: 'success' })
    }
    catch (error) {
      const { message } = useApiError(error)
      toast.add({ title: message.value, color: 'error' })
    }
  },
})

const userLabel = computed(() => {
  const user = auth.currentUser
  if (!user) {
    return 'Account'
  }

  return `${user.firstName} ${user.lastName}`
})

async function onLogout() {
  auth.logout()
  await navigateTo('/login')
}

const pageIcon = computed(() => {
  const icons: Record<string, string> = {
    '/dashboard': 'i-lucide-layout-dashboard',
    '/planning': 'i-lucide-calendar-range',
    '/timeline': 'i-lucide-calendar-range',
    '/users': 'i-lucide-users',
    '/organizations': 'i-lucide-building-2',
  }

  for (const [path, icon] of Object.entries(icons)) {
    if (route.path === path || route.path.startsWith(`${path}/`)) {
      return icon
    }
  }

  return 'i-lucide-layout-dashboard'
})
</script>

<template>
  <UDashboardGroup unit="px"  storage-key="planning">
    <UDashboardSidebar
      id="main"
      :default-size="240"
      :min-size="240"
      :max-size="240"
      collapsible
    >
      <template #header>
        <NuxtLink to="/dashboard" class="flex items-center gap-2 font-semibold text-highlighted">
          <UIcon name="i-lucide-calendar-days" class="size-5 text-secondary" />
          <span>Planning</span>
        </NuxtLink>
      </template>

      <UNavigationMenu
        :items="navigationItems"
        orientation="vertical"
        highlight
        highlight-color="secondary"
        class="w-full"
      />

      <template #footer>
        <div class="space-y-4 flex-1">
          <ColorModeSwitch />
          <USelect
          v-if="auth.hasOrganization && organizationOptions.length > 0"
          v-model="selectedOrgId"
          :items="organizationOptions"
          class="w-full"
          size="sm"
          />
    </div>
    </template>
  </UDashboardSidebar>

    <UDashboardPanel id="content">
      <template #header>

          <UDashboardNavbar :title="pageTitle" :icon="pageIcon">
            <template #right>
              <UDropdownMenu
          :items="[[{ label: 'Uitloggen', icon: 'i-lucide-log-out', onSelect: onLogout }]]"
          >
          <UButton
          variant="ghost"
          color="neutral"
          class="w-full justify-between"
          trailing-icon="i-lucide-chevron-up"
          >
          {{ userLabel }}
        </UButton>
      </UDropdownMenu>         
    </template>
  </UDashboardNavbar>
</template>

<template #body>
  <UAlert
  v-if="auth.isAuthenticated && !auth.hasOrganization"
  color="warning"
  variant="subtle"
  title="Geen organisatie gekoppeld"
  description="Maak een organisatie aan of vul een uitnodigingscode in."
  :actions="[
            { label: 'Organisatie aanmaken', to: '/organizations/new' },
            { label: 'Code invullen', to: '/join', variant: 'outline' },
          ]"
        />
        
        <slot />
      </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>