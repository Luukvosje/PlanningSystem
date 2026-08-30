<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui';
import { useLocalStorage, useMediaQuery } from '@vueuse/core';
import { unlessEditing } from '~/utils/shortcuts';

const auth = useAuthStore();
const route = useRoute();
const { t, locale, setLocale } = useI18n();

const languageOptions = [
  { code: 'nl' as const, label: 'Nederlands', icon: 'i-circle-flags-nl' },
  { code: 'en' as const, label: 'English', icon: 'i-circle-flags-gb' },
];

const currentLanguageIcon = computed(() =>
  languageOptions.find((option) => option.code === locale.value)?.icon ?? languageOptions[0].icon,
);

type SidebarMode = 'open' | 'auto' | 'closed'

const SIDEBAR_MODE_ICONS: Record<SidebarMode, string> = {
  open: 'i-lucide-panel-left-close',
  auto: 'i-lucide-panel-left-dashed',
  closed: 'i-lucide-panel-left',
};

const sidebarMode = useLocalStorage<SidebarMode>('sidebar-mode', 'open');
const mobileSidebarOpen = ref(false);
const isDesktop = useMediaQuery('(min-width: 1024px)');

/** Hovering anywhere over the rail (or the panel hanging off it) reveals the panel in auto mode. */
const sidebarHovered = ref(false);
/**
 * Reka sets `pointer-events: none` on the body while a select in the panel is open, so the
 * pointer leaves the sidebar without the user moving it. Closing on that mouseleave would
 * unmount the open list together with the panel, hence the second condition.
 */
const panelControlActive = ref(false);
const floatingPanelOpen = computed(() =>
  sidebarMode.value === 'auto' && (sidebarHovered.value || panelControlActive.value));

/** The keyboard shortcut stays a plain on/off: auto is a choice you make in the menu, not one you toggle into. */
function toggleSidebar() {
  if (!isDesktop.value) {
    mobileSidebarOpen.value = !mobileSidebarOpen.value;
    return;
  }

  sidebarMode.value = sidebarMode.value === 'open' ? 'closed' : 'open';
}

defineShortcuts({
  h: unlessEditing(toggleSidebar),
});

const sidebarModeItems = computed<DropdownMenuItem[][]>(() => [
  (['open', 'auto', 'closed'] as const).map((mode) => ({
    label: t(`layout.sidebarMode.${mode}`),
    icon: SIDEBAR_MODE_ICONS[mode],
    type: 'checkbox' as const,
    checked: sidebarMode.value === mode,
    onSelect: () => {
      sidebarMode.value = mode;
    },
  })),
]);

function closeMobileSidebar() {
  if (!isDesktop.value) {
    mobileSidebarOpen.value = false;
  }
}

const { data: currentUser } = useCurrentUser();
const { navigationModules, activeModule, navigationItems, pageTitle } = useAppNavigation();

useHead({ title: pageTitle });

// The panel already sits under its module's name in the rail, so the items don't repeat the icons.
const panelItems = computed(() =>
  (activeModule.value?.items ?? []).map((item) => ({ label: item.label, to: item.to })));

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

const userLabel = computed(() => {
  const user = auth.currentUser;
  if (!user) {
    return t('common.account');
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
      label: t('nav.settings'),
      icon: 'i-lucide-settings',
      to: '/settings',
    },
  ],
  [
    {
      label: t('layout.userMenu.language'),
      icon: currentLanguageIcon.value,
      children: languageOptions.map((option) => ({
        label: option.label,
        icon: option.icon,
        type: 'checkbox' as const,
        checked: locale.value === option.code,
        onSelect: () => setLocale(option.code),
      })),
    },
  ],
  [
    {
      label: t('layout.userMenu.logout'),
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
    '/organization': 'i-lucide-building-2',
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
	<div class="flex h-svh flex-1 gap-2 overflow-hidden bg-muted p-2 max-lg:gap-2 max-lg:p-2 bg-linear-to-r from-neutral-50 to-neutral-200 dark:from-neutral-900 dark:to-neutral-800">
		<div
			class="relative hidden h-full max-h-full shrink-0 lg:block"
			@mouseenter="sidebarHovered = true"
			@mouseleave="sidebarHovered = false"
		>
			<!-- The floating panel butts straight against this card, so the seam between them loses its corners. -->
			<div
				class="flex h-full overflow-hidden bg-default shadow-sm ring ring-default transition-[border-radius] duration-150 ease-out motion-reduce:transition-none"
				:class="floatingPanelOpen ? 'rounded-s-xl' : 'rounded-xl'"
			>
				<aside class="flex w-14 shrink-0 flex-col items-center">
					<div class="flex h-(--ui-header-height) shrink-0 items-center justify-center">
						<NuxtLink
							to="/dashboard"
							class="flex size-8 items-center justify-center rounded-lg text-highlighted"
							aria-label="Planning"
						>
							<UIcon
								name="i-lucide-calendar-days"
								class="size-5 text-brand"
							/>
						</NuxtLink>
					</div>

					<div class="flex min-h-0 flex-1 flex-col items-center gap-1 overflow-y-auto py-2">
						<UTooltip
							v-for="module in navigationModules"
							:key="module.key"
							:text="module.label"
							:content="{ side: 'right' }"
						>
							<UButton
								:icon="module.icon"
								:color="module.key === activeModule?.key ? 'brand' : 'neutral'"
								:variant="module.key === activeModule?.key ? 'soft' : 'ghost'"
								:to="module.items[0]?.to"
								square
								size="md"
								:aria-label="module.label"
								:aria-current="module.key === activeModule?.key ? 'page' : undefined"
							/>
						</UTooltip>
					</div>

					<div class="flex shrink-0 flex-col items-center gap-1 p-4">
						<UDropdownMenu
							:items="sidebarModeItems"
							:content="{ side: 'right', align: 'end', sideOffset: 12 }"
							:ui="{ content: 'w-44' }"
						>
							<UTooltip
								:text="t('layout.sidebarMode.label')"
								:kbds="['H']"
								:content="{ side: 'right' }"
							>
								<UButton
									:icon="SIDEBAR_MODE_ICONS[sidebarMode]"
									color="neutral"
									variant="ghost"
									square
									size="md"
									class="data-[state=open]:bg-elevated"
									:aria-label="t('layout.sidebarMode.label')"
								/>
							</UTooltip>
						</UDropdownMenu>

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
								:aria-label="t('common.account')"
							/>
						</UDropdownMenu>
					</div>
				</aside>

				<aside
					class="flex flex-col overflow-hidden transition-[width,border-color] duration-200 ease-out motion-reduce:transition-none"
					:class="sidebarMode === 'open' ? 'w-48 border-s border-default' : 'w-0 border-s-0'"
				>
					<LayoutNavPanel
						:label="activeModule?.label"
						:items="panelItems"
					/>
				</aside>
			</div>

			<Transition
				enter-active-class="transition duration-150 ease-out"
				enter-from-class="-translate-x-2 opacity-0"
				enter-to-class="translate-x-0 opacity-100"
				leave-active-class="transition duration-100 ease-in"
				leave-from-class="translate-x-0 opacity-100"
				leave-to-class="-translate-x-2 opacity-0"
			>
				<LayoutNavPanel
					v-if="floatingPanelOpen"
					:label="activeModule?.label"
					:items="panelItems"
					class="absolute inset-y-0 start-14 z-40 overflow-hidden rounded-e-xl shadow-lg ring ring-default"
					@control-active="panelControlActive = $event"
				/>
			</Transition>
		</div>

		<!-- Mobile slideover -->
		<USidebar
			v-if="!isDesktop"
			v-model:open="mobileSidebarOpen"
			variant="inset"
			collapsible="offcanvas"
			:menu="{
				ui: {
					content: 'inset-y-2 left-2 w-[calc(100%-1rem)]',
				},
			}"
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
						class="size-5 shrink-0 text-brand"
					/>
					<span class="truncate">Planning</span>
				</NuxtLink>
			</template>

			<template #default>
				<UNavigationMenu
					:items="mobileNavigationItems"
					orientation="vertical"
					highlight
					highlight-color="brand"
					class="w-full"
					:ui="{ link: 'p-1.5 overflow-hidden', separator: 'hidden' }"
				/>
			</template>

			<template #footer>
				<ControlsColorModeSwitch />

				<ControlsOrganizationSwitch />

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

		<!--
			Its own stacking context. The timeline stacks its sticky resource column at z-1000 to keep
			it above a block being dragged, and without isolation that number is measured against the
			floating nav panel and wins - the resource names paint straight over it. Contained here,
			page-level z-indexes stay a page concern and the panel keeps the layer it asked for.
		-->
		<div class="isolate flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden rounded-xl bg-default shadow-sm ring ring-default max-lg:rounded-lg">
			<div class="flex h-(--ui-header-height) shrink-0 items-center gap-2 border-b border-default px-4 max-lg:sticky max-lg:top-0 max-lg:z-10 max-lg:h-11 max-lg:gap-1.5 max-lg:bg-default max-lg:px-3">
				<UButton
					icon="i-lucide-panel-left"
					color="neutral"
					variant="ghost"
					class="lg:hidden"
					size="sm"
					:aria-label="t('layout.toggleSidebarMobile')"
					@click="toggleSidebar"
				/>
				<UIcon
					:name="pageIcon"
					class="size-5 shrink-0 text-brand max-lg:size-4"
				/>
				<slot name="title">
					<h1 class="min-w-0 truncate font-semibold text-highlighted max-lg:text-sm">
						{{ pageTitle }}
					</h1>
				</slot>

				<div class="ms-auto flex min-w-0 items-center justify-end gap-2 max-lg:gap-1.5">
					<slot name="actions" />
				</div>
			</div>

			<div
				v-if="$slots.tabs"
				class="flex shrink-0 items-center border-b border-default px-4 max-lg:px-3"
			>
				<slot name="tabs" />
			</div>

			<div
				class="flex min-h-0 flex-1 flex-col overflow-hidden"
				:class="isEdgeAlignedPage ? 'p-0' : 'p-4 max-lg:p-3'"
			>
				<UAlert
					v-if="auth.isAuthenticated && !auth.hasOrganization"
					color="warning"
					variant="subtle"
					:title="t('layout.noOrganizationAlert.title')"
					:description="t('layout.noOrganizationAlert.description')"
					:actions="[
						{ label: t('layout.noOrganizationAlert.createOrganization'), to: '/organization/new' },
						{ label: t('layout.noOrganizationAlert.enterCode'), to: '/join', variant: 'outline' },
					]"
					class="mb-4 shrink-0 max-lg:mb-3"
				/>

				<div class="flex min-h-0 min-w-0 flex-1 flex-col overflow-auto">
					<slot />
				</div>
			</div>
		</div>
	</div>
</template>
