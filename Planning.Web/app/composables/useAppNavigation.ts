import type { NavigationMenuItem } from '@nuxt/ui';
import { AppModule } from '~/generated/models';
import { canAccessModule } from '~/utils/modules';

export interface AppNavigationItem {
  label: string
  icon: string
  to: string
}

/**
 * One entry in the icon rail: a main module plus the pages that live under it. The rail shows the
 * modules, the panel next to it shows the items of whichever module you are in.
 */
export interface AppNavigationModule {
  key: string
  label: string
  icon: string
  items: AppNavigationItem[]
}

export function useAppNavigation() {
  const route = useRoute();
  const auth = useAuthStore();
  const { t } = useI18n();
  const modules = computed(() => auth.currentUser?.modules);

  const navigationModules = computed<AppNavigationModule[]>(() => {
    const hasPlanning = canAccessModule(AppModule.Planning, modules.value);

    const planningItems: AppNavigationItem[] = [
      { label: t('nav.dashboard'), icon: 'i-lucide-layout-dashboard', to: '/dashboard' },
    ];

    if (hasPlanning) {
      planningItems.push({ label: t('nav.planning'), icon: 'i-lucide-calendar-check', to: '/planning' });
      planningItems.push({ label: t('nav.timeline'), icon: 'i-lucide-gantt-chart', to: '/timeline' });

      if (canManagePlanning(auth.currentUser?.role)) {
        planningItems.push({
          label: t('nav.availability'),
          icon: 'i-lucide-calendar-clock',
          to: '/beschikbaarheid',
        });
      }
    }

    // Without the planning module this group is the dashboard and nothing else, so it doesn't
    // pretend to be about planning.
    const result: AppNavigationModule[] = [{
      key: 'planning',
      label: hasPlanning ? t('nav.planning') : t('nav.menu'),
      icon: hasPlanning ? 'i-lucide-calendar-check' : 'i-lucide-layout-dashboard',
      items: planningItems,
    }];

    if (canAccessModule(AppModule.Klant, modules.value)) {
      result.push({
        key: 'relations',
        label: t('nav.relations'),
        icon: 'i-lucide-contact',
        items: [
          { label: t('nav.customers'), icon: 'i-lucide-contact', to: '/customers' },
        ],
      });
    }

    if (canManageOrganization(auth.currentUser?.role)) {
      const managementItems: AppNavigationItem[] = [];

      if (canAccessModule(AppModule.Beheer, modules.value)) {
        managementItems.push({ label: t('nav.team'), icon: 'i-lucide-users', to: '/users' });
      }

      managementItems.push({
        label: t('nav.organization'),
        icon: 'i-lucide-building-2',
        to: '/organization',
      });

      result.push({
        key: 'management',
        label: t('nav.management'),
        icon: 'i-lucide-building-2',
        items: managementItems,
      });
    }

    return result;
  });

  function matchesRoute(to: string) {
    return route.path === to || route.path.startsWith(`${to}/`);
  }

  /** The module you are in, falling back to the first one for pages that belong to none (settings). */
  const activeModule = computed<AppNavigationModule | undefined>(() =>
    navigationModules.value.find((module) => module.items.some((item) => matchesRoute(item.to))) ??
    navigationModules.value[0]);

  /** The same navigation as one flat list with group labels, for the mobile slideover. */
  const navigationItems = computed<NavigationMenuItem[][]>(() =>
    navigationModules.value.map((module) => [
      { label: module.label, type: 'label' as const },
      ...module.items.map((item) => ({ label: item.label, icon: item.icon, to: item.to })),
    ]));

  const pageTitle = computed(() => {
    const titles: Record<string, string> = {
      '/dashboard': t('nav.dashboard'),
      '/planning': t('nav.planning'),
      '/timeline': t('nav.timeline'),
      '/beschikbaarheid': t('nav.availability'),
      '/customers': t('nav.customers'),
      '/users': t('nav.team'),
      '/organization': t('nav.organization'),
      '/invites': t('nav.invites'),
      '/settings': t('nav.settings'),
    };

    for (const [path, title] of Object.entries(titles)) {
      if (matchesRoute(path)) {
        return title;
      }
    }

    return t('nav.planning');
  });

  return {
    navigationModules,
    activeModule,
    navigationItems,
    pageTitle,
  };
}
