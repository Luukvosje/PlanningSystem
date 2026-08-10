<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui';
import { useQueryClient } from '@tanstack/vue-query';
import { useLocalStorage, useMediaQuery } from '@vueuse/core';

const auth = useAuthStore();
const toast = useToast();
const queryClient = useQueryClient();
const route = useRoute();

const sidebarOpen = useLocalStorage('sidebar-open', true);
const isDesktop = useMediaQuery('(min-width: 1024px)');

function toggleSidebar() {
  sidebarOpen.value = !sidebarOpen.value;
}

defineShortcuts({
  h: toggleSidebar,
});

const sidebarToggleLabel = computed(() =>
  sidebarOpen.value ? 'Navigatie verbergen' : 'Navigatie tonen',
);

function closeMobileSidebar() {
  if (!isDesktop.value) {
    sidebarOpen.value = false;
  }
}

const { data: currentUser } = useCurrentUser();
const { data: organizations } = useMyOrganizations();
const { navigationItems, pageTitle } = useAppNavigation();

const mobileNavigationItems = computed(() =>
  navigationItems.value.map((group) =>
    group.map((item) => {
      if (item.type === 'label' || !item.to) {
        return item;
      }

      const existingOnSelect = item.onSelect;
      return {
        ...item,
        onSelect: (event: Event) => {
          existingOnSelect?.(event);
          closeMobileSidebar();
        },
      };
    }),
  ),
);

watch(currentUser, (user) => {
  if (user) {
    auth.currentUser = user;
  }
}, { immediate: true });

const organizationOptions = computed(() =>
  (organizations.value ?? []).map((org) => ({
    label: org.organizationName ?? 'Onbekend',
    value: org.organizationId ?? '',
  })),
);

const selectedOrganizationId = computed({
  get: () => auth.organizationId ?? '',
  set: (orgId: string) => {
    void switchOrganization(orgId);
  },
});

async function switchOrganization(orgId: string) {
  if (!orgId || orgId === auth.organizationId) {
    return;
  }

  try {
    auth.selectOrganization(orgId);
    await invalidateOrgScopedQueries(queryClient);
    toast.add({ title: 'Organisatie gewisseld', color: 'success' });
  } catch (error) {
    const { message } = useApiError(error);
    toast.add({ title: message.value, color: 'error' });
  }
}

const userLabel = computed(() => {
  const user = auth.currentUser;
  if (!user) {
    return 'Account';
  }

  return `${user.firstName} ${user.lastName}`;
});

const userEmail = computed(() => auth.currentUser?.email ?? '');

const userItems = computed<DropdownMenuItem[][]>(() => [
  [
    {
      label: userLabel.value,
      description: userEmail.value || undefined,
      type: 'label',
    },
  ],
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
]);

async function onLogout() {
  auth.logout();
  await navigateTo('/login');
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
  };

  for (const [path, icon] of Object.entries(icons)) {
    if (route.path === path || route.path.startsWith(`${path}/`)) {
      return icon;
    }
  }

  return 'i-lucide-layout-dashboard';
});

const isEdgeAlignedPage = computed(() => {
  const path = route.path;
  return path === '/timeline' || path.startsWith('/timeline/') ||
    path === '/planning' || path.startsWith('/planning/');
});

/** Rail w-14 (56px), md square button (32px), footer p-4 (16px) — menu aligns with the nav aside (w-48). */
const userMenuContent = computed(() => ({
  side: 'right' as const,
  align: 'end' as const,
  sideOffset: (56 - 32) / 2,
  alignOffset: -16,
  collisionPadding: 16,
}));
</script>

<template>
	<div class="flex h-svh flex-1 gap-4 overflow-hidden bg-neutral-50 p-4 dark:bg-neutral-950 max-lg:gap-2 max-lg:p-2">
		<div class="hidden h-full max-h-full shrink-0 overflow-hidden rounded-xl bg-default shadow-sm ring ring-default lg:flex">
			<aside class="flex w-14 shrink-0 flex-col items-center">
				<div class="flex h-(--ui-header-height) shrink-0 items-center justify-center">
					<NuxtLink
						to="/dashboard"
						class="flex size-8 items-center justify-center rounded-lg text-highlighted"
						aria-label="Planning"
					>
						<UIcon
							name="i-lucide-calendar-days"
							class="size-5 text-secondary"
						/>
					</NuxtLink>
				</div>

				<div class="flex flex-1 items-center justify-center">
					<UTooltip
						:text="sidebarToggleLabel"
						:kbds="['H']"
						:content="{ side: 'right' }"
					>
						<UButton
							:icon="sidebarOpen ? 'i-lucide-panel-left-close' : 'i-lucide-panel-left'"
							color="neutral"
							variant="ghost"
							square
							size="md"
							:aria-label="sidebarToggleLabel"
							@click="toggleSidebar"
						/>
					</UTooltip>
				</div>

				<div class="flex shrink-0 items-center justify-center p-4">
					<UDropdownMenu
						:items="userItems"
						:content="userMenuContent"
						:ui="{ content: 'w-48' }"
					>
						<UButton
							icon="i-lucide-user"
							color="neutral"
							variant="ghost"
							square
							size="md"
							class="data-[state=open]:bg-elevated"
							aria-label="Account"
						/>
					</UDropdownMenu>
				</div>
			</aside>

			<aside
				class="flex flex-col overflow-hidden transition-[width,border-color] duration-200 ease-out motion-reduce:transition-none"
				:class="sidebarOpen ? 'w-48 border-s border-default' : 'w-0 border-s-0'"
			>
				<div class="flex h-full w-48 min-h-0 flex-col">
					<div class="flex min-h-(--ui-header-height) shrink-0 items-center px-4">
						<NuxtLink
							to="/dashboard"
							class="truncate font-semibold text-highlighted"
						>
							Planning
						</NuxtLink>
					</div>

					<div class="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto p-4">
						<UNavigationMenu
							:items="navigationItems"
							orientation="vertical"
							highlight
							highlight-color="secondary"
							class="w-full"
							:ui="{ link: 'p-1.5 overflow-hidden' }"
						/>
					</div>

					<div class="flex shrink-0 flex-col gap-3 p-4">
						<ColorModeSwitch />

						<USelect
							v-if="organizationOptions.length > 0"
							v-model="selectedOrganizationId"
							:items="organizationOptions"
							icon="i-lucide-building-2"
							placeholder="Organisatie"
							class="w-full"
							size="sm"
						/>
					</div>
				</div>
			</aside>
		</div>

		<!-- Mobile slideover -->
		<USidebar
			v-if="!isDesktop"
			v-model:open="sidebarOpen"
			variant="inset"
			collapsible="offcanvas"
			:ui="{
				header: 'sticky top-0 z-10 shrink-0',
				body: 'p-2',
				footer: 'flex-col items-stretch gap-2 p-2',
			}"
		>
			<template #header>
				<NuxtLink
					to="/dashboard"
					class="flex items-center gap-2 overflow-hidden font-semibold text-highlighted"
					@click="closeMobileSidebar"
				>
					<UIcon
						name="i-lucide-calendar-days"
						class="size-5 shrink-0 text-secondary"
					/>
					<span class="truncate">Planning</span>
				</NuxtLink>
			</template>

			<template #default>
				<UNavigationMenu
					:items="mobileNavigationItems"
					orientation="vertical"
					highlight
					highlight-color="secondary"
					class="w-full"
					:ui="{ link: 'p-1.5 overflow-hidden' }"
				/>
			</template>

			<template #footer>
				<ColorModeSwitch />

				<USelect
					v-if="organizationOptions.length > 0"
					v-model="selectedOrganizationId"
					:items="organizationOptions"
					icon="i-lucide-building-2"
					placeholder="Organisatie"
					class="w-full"
					size="sm"
				/>

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
						size="sm"
						class="w-full overflow-hidden data-[state=open]:bg-elevated"
						:ui="{ trailingIcon: 'text-dimmed ms-auto' }"
					/>
				</UDropdownMenu>
			</template>
		</USidebar>

		<div class="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden rounded-xl bg-default shadow-sm ring ring-default max-lg:rounded-lg">
			<div class="flex h-(--ui-header-height) shrink-0 items-center gap-2 border-b border-default px-4 max-lg:sticky max-lg:top-0 max-lg:z-10 max-lg:h-11 max-lg:gap-1.5 max-lg:bg-default max-lg:px-3">
				<UButton
					icon="i-lucide-panel-left"
					color="neutral"
					variant="ghost"
					class="lg:hidden"
					size="sm"
					aria-label="Toggle sidebar"
					@click="toggleSidebar"
				/>
				<UIcon
					:name="pageIcon"
					class="size-5 text-secondary max-lg:size-4"
				/>
				<h1 class="font-semibold text-highlighted max-lg:truncate max-lg:text-sm">
					{{ pageTitle }}
				</h1>
			</div>

			<div
				class="flex min-h-0 flex-1 flex-col overflow-hidden"
				:class="isEdgeAlignedPage ? 'p-0' : 'p-4 max-lg:p-3'"
			>
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
					class="mb-4 shrink-0 max-lg:mb-3"
				/>

				<div class="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
					<slot />
				</div>
			</div>
		</div>
	</div>
</template>
