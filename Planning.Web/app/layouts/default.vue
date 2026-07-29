<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'
import { useQueryClient } from '@tanstack/vue-query'
import { useLocalStorage, useMediaQuery } from '@vueuse/core'

type SidebarMode = 'open' | 'closed' | 'auto'

const auth = useAuthStore()
const toast = useToast()
const queryClient = useQueryClient()
const route = useRoute()

const sidebarMode = useLocalStorage<SidebarMode>('sidebar-mode', 'auto')
const sidebarOpen = ref(false)
const isDesktop = useMediaQuery('(min-width: 1024px)')
const collapseTimer = ref<ReturnType<typeof setTimeout> | null>(null)
const expandTimer = ref<ReturnType<typeof setTimeout> | null>(null)
/** After a click while collapsed, keep auto-expand suppressed until the pointer leaves. */
const suppressAutoExpand = ref(false)

const AUTO_EXPAND_DELAY_MS = 400

const sidebarModeOptions = [
  { label: 'Open', value: 'open' },
  { label: 'Dicht', value: 'closed' },
  { label: 'Automatisch', value: 'auto' },
]

function clearSidebarTimers() {
  if (collapseTimer.value) {
    clearTimeout(collapseTimer.value)
    collapseTimer.value = null
  }
  if (expandTimer.value) {
    clearTimeout(expandTimer.value)
    expandTimer.value = null
  }
}

watch(sidebarMode, (mode) => {
  clearSidebarTimers()
  suppressAutoExpand.value = false

  if (mode === 'open') {
    sidebarOpen.value = true
  }
  else {
    sidebarOpen.value = false
  }
}, { immediate: true })

function onSidebarMouseEnter() {
  if (collapseTimer.value) {
    clearTimeout(collapseTimer.value)
    collapseTimer.value = null
  }

  if (sidebarMode.value !== 'auto' || !isDesktop.value || suppressAutoExpand.value) {
    return
  }

  // Delay expand so a quick click on an icon can navigate without opening the sidebar.
  expandTimer.value = setTimeout(() => {
    expandTimer.value = null
    if (sidebarMode.value === 'auto' && !suppressAutoExpand.value) {
      sidebarOpen.value = true
    }
  }, AUTO_EXPAND_DELAY_MS)
}

function onSidebarMouseLeave() {
  if (expandTimer.value) {
    clearTimeout(expandTimer.value)
    expandTimer.value = null
  }

  suppressAutoExpand.value = false

  if (sidebarMode.value === 'auto' && isDesktop.value) {
    collapseTimer.value = setTimeout(() => {
      sidebarOpen.value = false
    }, 200)
  }
}

function onSidebarPointerDown() {
  // Clicking while collapsed: cancel pending expand and stay icon-only.
  if (!sidebarOpen.value) {
    if (expandTimer.value) {
      clearTimeout(expandTimer.value)
      expandTimer.value = null
    }
    if (sidebarMode.value === 'auto') {
      suppressAutoExpand.value = true
    }
  }
}

function toggleSidebar() {
  clearSidebarTimers()
  suppressAutoExpand.value = false
  const nextOpen = !sidebarOpen.value
  sidebarOpen.value = nextOpen
  sidebarMode.value = nextOpen ? 'open' : 'closed'
}

onBeforeUnmount(() => {
  clearSidebarTimers()
})

function closeMobileSidebar() {
  if (!isDesktop.value) {
    sidebarOpen.value = false
  }
}

const { data: currentUser } = useCurrentUser()
const { data: organizations } = useMyOrganizations()
const { navigationItems, pageTitle } = useAppNavigation()

const mobileNavigationItems = computed(() =>
  navigationItems.value.map(group =>
    group.map((item) => {
      if (item.type === 'label' || !item.to) {
        return item
      }

      const existingOnSelect = item.onSelect
      return {
        ...item,
        onSelect: (event: Event) => {
          existingOnSelect?.(event)
          closeMobileSidebar()
        },
      }
    }),
  ),
)

const sidebarUi = computed(() => ({
  container: 'h-full',
  ...(!isDesktop.value
    ? {
        header: 'sticky top-0 z-10 shrink-0 ',
        body: 'p-2',
        footer: 'p-2 shrink-0',
      }
    : {}),
}))

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

async function switchOrganization(orgId: string) {
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
}

const userLabel = computed(() => {
  const user = auth.currentUser
  if (!user) {
    return 'Account'
  }

  return `${user.firstName} ${user.lastName}`
})

const userEmail = computed(() => auth.currentUser?.email ?? '')

const userItems = computed<DropdownMenuItem[][]>(() => {
  const items: DropdownMenuItem[][] = [
    [
      {
        label: userLabel.value,
        description: userEmail.value || undefined,
        type: 'label',
      },
    ],
  ]

  if (organizationOptions.value.length > 0) {
    items.push([
      {
        label: 'Organisatie',
        icon: 'i-lucide-building-2',
        children: [
          organizationOptions.value.map(org => ({
            label: org.label,
            icon: org.value === auth.organizationId ? 'i-lucide-check' : undefined,
            onSelect: () => switchOrganization(org.value),
          })),
        ],
      },
    ])
  }

  items.push(
    [
      {
        label: 'Instellingen',
        icon: 'i-lucide-settings',
        to: '/settings',
      },
    ],
    [
      {
        label: 'Uitloggen',
        icon: 'i-lucide-log-out',
        color: 'error',
        onSelect: onLogout,
      },
    ],
  )

  return items
})

async function onLogout() {
  auth.logout()
  await navigateTo('/login')
}

const pageIcon = computed(() => {
  const icons: Record<string, string> = {
    '/dashboard': 'i-lucide-layout-dashboard',
    '/planning': 'i-lucide-calendar-check',
    '/timeline': 'i-lucide-gantt-chart',
    '/beschikbaarheid': 'i-lucide-calendar-clock',
    '/customers': 'i-lucide-contact',
    '/users': 'i-lucide-users',
    '/organizations': 'i-lucide-building-2',
    '/settings': 'i-lucide-settings',
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
  <div class="flex min-h-svh flex-1 bg-neutral-50 dark:bg-neutral-950">
    <USidebar
      v-model:open="sidebarOpen"
      variant="inset"
      collapsible="icon"
      :ui="sidebarUi"
      @mouseenter="onSidebarMouseEnter"
      @mouseleave="onSidebarMouseLeave"
      @pointerdown="onSidebarPointerDown"
    >
      <template #header>
        <NuxtLink
          to="/dashboard"
          class="flex items-center gap-2 overflow-hidden font-semibold text-highlighted"
          @click="closeMobileSidebar"
        >
          <UIcon name="i-lucide-calendar-days" class="size-5 shrink-0 text-secondary" />
          <span class="truncate group-data-[state=collapsed]/sidebar:hidden">Planning</span>
        </NuxtLink>
      </template>

      <template #default="{ state }">
        <UNavigationMenu
          :key="state"
          :items="isDesktop ? navigationItems : mobileNavigationItems"
          orientation="vertical"
          highlight
          highlight-color="secondary"
          class="w-full"
          :ui="{ link: 'p-1.5 overflow-hidden' }"
        />
      </template>

      <template #footer>
        <div class="flex w-full flex-col gap-4 max-lg:gap-2">
          <div class="flex flex-col gap-4 group-data-[state=collapsed]/sidebar:hidden max-lg:gap-2">
            <ColorModeSwitch />
          </div>

          <UDropdownMenu
            :items="userItems"
            :content="{ align: 'center', collisionPadding: 12 }"
            :ui="{ content: 'w-(--reka-dropdown-menu-trigger-width) min-w-48' }"
          >
            <UButton
              icon="i-lucide-user"
              :label="userLabel"
              trailing-icon="i-lucide-chevrons-up-down"
              color="neutral"
              variant="ghost"
              square
              :size="isDesktop ? undefined : 'sm'"
              class="w-full overflow-hidden data-[state=open]:bg-elevated"
              :ui="{ trailingIcon: 'text-dimmed ms-auto group-data-[state=collapsed]/sidebar:hidden' }"
            />
          </UDropdownMenu>

          <USelect
            v-model="sidebarMode"
            :items="sidebarModeOptions"
            class="w-full group-data-[state=collapsed]/sidebar:hidden max-lg:hidden"
            size="sm"
          />
        </div>
      </template>
    </USidebar>

    <div class="flex flex-1 flex-col overflow-hidden bg-default peer-data-[variant=inset]:m-4 peer-data-[variant=inset]:rounded-xl peer-data-[variant=inset]:shadow-sm peer-data-[variant=inset]:ring peer-data-[variant=inset]:ring-default max-lg:peer-data-[variant=inset]:m-2 max-lg:peer-data-[variant=inset]:rounded-lg lg:peer-data-[variant=floating]:my-4 lg:peer-data-[variant=inset]:not-peer-data-[collapsible=offcanvas]:ms-0">
      <div class="flex h-(--ui-header-height) shrink-0 items-center gap-2 border-b border-default px-4 max-lg:sticky max-lg:top-0 max-lg:z-10 max-lg:h-11 max-lg:gap-1.5 max-lg:bg-default max-lg:px-3">
        <UButton
          icon="i-lucide-panel-left"
          color="neutral"
          variant="ghost"
          :size="isDesktop ? undefined : 'sm'"
          aria-label="Toggle sidebar"
          @click="toggleSidebar"
        />
        <UIcon :name="pageIcon" class="size-5 text-secondary max-lg:size-4" />
        <h1 class="font-semibold text-highlighted max-lg:truncate max-lg:text-sm">
          {{ pageTitle }}
        </h1>
      </div>

      <div class="min-h-0 flex-1 overflow-auto p-4 max-lg:p-3">
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
          class="m-4 max-lg:m-0 max-lg:mb-3"
        />

        <slot />
      </div>
    </div>
  </div>
</template>
