import type { TabsItem } from '@nuxt/ui';

export interface EntityTab extends TabsItem {
  value: string
}

/**
 * Keeps a detail page's active tab in the URL (`?tab=planning`), so a tab is linkable from a mail
 * or a notification. Unknown or missing values fall back to the first tab rather than rendering
 * nothing, and switching replaces the history entry — tabbing around is not navigation.
 */
export function useEntityTabs(items: MaybeRefOrGetter<EntityTab[]>, param = 'tab') {
  const route = useRoute();
  const router = useRouter();

  const resolved = computed(() => toValue(items));

  const tab = computed<string>({
    get: () => {
      const current = route.query[param];
      const value = Array.isArray(current) ? current[0] : current;

      return resolved.value.some((item) => item.value === value) ?
        value as string :
        resolved.value[0]?.value ?? '';
    },
    set: (value) => {
      void router.replace({ query: { ...route.query, [param]: value } });
    },
  });

  return { tab, items: resolved };
}
