import type { NavigationMenuItem } from '@nuxt/ui';
import { AppModule } from '~/generated/models';
import { canAccessModule } from '~/utils/modules';

export function useAppNavigation() {
  const route = useRoute();
  const auth = useAuthStore();
  const { t } = useI18n();
  const modules = computed(() => auth.currentUser?.modules);

  const navigationItems = computed<NavigationMenuItem[][]>(() => {
    const main: NavigationMenuItem[] = [
      { label: t('nav.menu'), type: 'label' },
      {
        label: t('nav.dashboard'),
        icon: 'i-lucide-layout-dashboard',
        to: '/dashboard',
      },
    ];

    if (canAccessModule(AppModule.Planning, modules.value)) {
      main.push({
        label: t('nav.planning'),
        icon: 'i-lucide-calendar-check',
        to: '/planning',
      });
      main.push({
        label: t('nav.timeline'),
        icon: 'i-lucide-gantt-chart',
        to: '/timeline',
      });
      main.push({
        label: t('nav.availability'),
        icon: 'i-lucide-calendar-clock',
        to: '/beschikbaarheid',
      });
    }

    const groups: NavigationMenuItem[][] = [main];

    if (canAccessModule(AppModule.Klant, modules.value)) {
      groups.push([
        { label: t('nav.relations'), type: 'label' },
        {
          label: t('nav.customers'),
          icon: 'i-lucide-contact',
          to: '/customers',
        },
      ]);
    }

    if (canManageOrganization(auth.currentUser?.role)) {
      const children: NavigationMenuItem[] = [];

      if (canAccessModule(AppModule.Beheer, modules.value)) {
        children.push({
          label: t('nav.team'),
          icon: 'i-lucide-users',
          to: '/users',
        });
      }

      children.push({
        label: t('nav.organization'),
        icon: 'i-lucide-building-2',
        to: '/organizations',
      });

      groups.push([
        { label: t('nav.management'), type: 'label' },
        ...children,
      ]);
    }

    return groups;
  });

  const pageTitle = computed(() => {
    const titles: Record<string, string> = {
      '/dashboard': t('nav.dashboard'),
      '/planning': t('nav.planning'),
      '/timeline': t('nav.timeline'),
      '/beschikbaarheid': t('nav.availability'),
      '/customers': t('nav.customers'),
      '/users': t('nav.team'),
      '/organizations': t('nav.organization'),
      '/invites': t('nav.invites'),
      '/settings': t('nav.settings'),
    };

    for (const [path, title] of Object.entries(titles)) {
      if (route.path === path || route.path.startsWith(`${path}/`)) {
        return title;
      }
    }

    return t('nav.planning');
  });

  return {
    navigationItems,
    pageTitle,
  };
}
