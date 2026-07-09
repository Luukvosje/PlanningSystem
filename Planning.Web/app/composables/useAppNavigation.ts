import type { NavigationMenuItem } from '@nuxt/ui'
import { AppModule } from '~/generated/models'

const beheerRoutes = ['/users', '/organizations', '/invites']

export function useAppNavigation() {
  const route = useRoute()
  const auth = useAuthStore()
  const modules = computed(() => auth.currentUser?.modules)

  const navigationItems = computed<NavigationMenuItem[][]>(() => {
    const main: NavigationMenuItem[] = [
      { label: 'Navigatie', type: 'label' },
      {
        label: 'Dashboard',
        icon: 'i-lucide-layout-dashboard',
        to: '/dashboard',
      },
    ]

    if (hasModule(modules.value, AppModule.Planning)) {
      main.push({
        label: 'Planning',
        icon: 'i-lucide-calendar-range',
        to: '/planning',
      })
      main.push({
        label: 'Beschikbaarheid',
        icon: 'i-lucide-calendar-clock',
        to: '/beschikbaarheid',
      })
    }

    if (hasModule(modules.value, AppModule.Klant)) {
      main.push({
        label: 'Klanten',
        icon: 'i-lucide-contact',
        to: '/customers',
      })
    }

    const groups: NavigationMenuItem[][] = [main]

    if (canManageOrganization(auth.currentUser?.role)) {
      const children: NavigationMenuItem[] = []

      if (hasModule(modules.value, AppModule.Beheer)) {
        children.push({
          label: 'Team',
          icon: 'i-lucide-users',
          to: '/users',
        })
      }

      children.push({
        label: 'Organisaties',
        icon: 'i-lucide-building-2',
        to: '/organizations',
      })

      groups.push([
        { label: 'Beheer', type: 'label' },
        {
          label: 'Beheer',
          icon: 'i-lucide-settings-2',
          type: 'trigger',
          defaultOpen: beheerRoutes.some(path => route.path.startsWith(path)),
          children,
        },
      ])
    }

    return groups
  })

  const pageTitle = computed(() => {
    const titles: Record<string, string> = {
      '/dashboard': 'Dashboard',
      '/planning': 'Planning',
      '/beschikbaarheid': 'Beschikbaarheid',
      '/timeline': 'Planning',
      '/customers': 'Klanten',
      '/users': 'Team',
      '/organizations': 'Organisaties',
      '/invites': 'Uitnodigen',
    }

    for (const [path, title] of Object.entries(titles)) {
      if (route.path === path || route.path.startsWith(`${path}/`)) {
        return title
      }
    }

    return 'Planning'
  })

  return {
    navigationItems,
    pageTitle,
  }
}
