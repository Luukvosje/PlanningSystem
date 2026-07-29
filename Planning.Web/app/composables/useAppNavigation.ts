import type { NavigationMenuItem } from '@nuxt/ui';
import { AppModule } from '~/generated/models';
import { canAccessModule } from '~/utils/modules';

export function useAppNavigation() {
  const route = useRoute();
  const auth = useAuthStore();
  const modules = computed(() => auth.currentUser?.modules);

  const navigationItems = computed<NavigationMenuItem[][]>(() => {
    const main: NavigationMenuItem[] = [
      { label: 'Navigatie', type: 'label' },
      {
        label: 'Dashboard',
        icon: 'i-lucide-layout-dashboard',
        to: '/dashboard',
      },
    ];

    if (canAccessModule(AppModule.Planning, modules.value)) {
      main.push({
        label: 'Planning',
        icon: 'i-lucide-calendar-check',
        to: '/planning',
      });
      main.push({
        label: 'Tijdlijn',
        icon: 'i-lucide-gantt-chart',
        to: '/timeline',
      });
      main.push({
        label: 'Beschikbaarheid',
        icon: 'i-lucide-calendar-clock',
        to: '/beschikbaarheid',
      });
    }

    if (canAccessModule(AppModule.Klant, modules.value)) {
      main.push({
        label: 'Klanten',
        icon: 'i-lucide-contact',
        to: '/customers',
      });
    }

    const groups: NavigationMenuItem[][] = [main];

    if (canManageOrganization(auth.currentUser?.role)) {
      const children: NavigationMenuItem[] = [];

      if (canAccessModule(AppModule.Beheer, modules.value)) {
        children.push({
          label: 'Team',
          icon: 'i-lucide-users',
          to: '/users',
        });
      }

      children.push({
        label: 'Organisatie',
        icon: 'i-lucide-building-2',
        to: '/organizations',
      });

      groups.push([
        { label: 'Beheer', type: 'label' },
        ...children,
      ]);
    }

    return groups;
  });

  const pageTitle = computed(() => {
    const titles: Record<string, string> = {
      '/dashboard': 'Dashboard',
      '/planning': 'Planning',
      '/timeline': 'Tijdlijn',
      '/beschikbaarheid': 'Beschikbaarheid',
      '/customers': 'Klanten',
      '/users': 'Team',
      '/organizations': 'Organisatie',
      '/invites': 'Uitnodigen',
      '/settings': 'Instellingen',
    };

    for (const [path, title] of Object.entries(titles)) {
      if (route.path === path || route.path.startsWith(`${path}/`)) {
        return title;
      }
    }

    return 'Planning';
  });

  return {
    navigationItems,
    pageTitle,
  };
}
