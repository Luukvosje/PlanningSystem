import { useLocalStorage } from '@vueuse/core';
import type { MaybeRefOrGetter } from 'vue';
import type { EntityKind } from '~/composables/entities/useEntitySource';

const STORAGE_KEY = 'planning:entity-picker';
const MAX_RECENT = 20;

export type EntitySortMode = 'alphabetical' | 'recent'

interface EntityPickerState {
  recent: string[]
  sort: EntitySortMode
  showInactive: boolean
}

function emptyState(): EntityPickerState {
  return { recent: [], sort: 'alphabetical', showInactive: false };
}

/**
 * Remembers per organisation and entity type which items you picked last, plus how you left the
 * picker. "Recent" in a picker means what you reached for lately, which the API cannot know -
 * `updatedAtUtc` would only say which record was edited last, by anyone.
 *
 * One storage key holding a map, rather than a key per scope: that keeps the `useLocalStorage`
 * key static while the organisation and the entity type stay reactive.
 */
export function useRecentEntities(kind: MaybeRefOrGetter<EntityKind>) {
  const auth = useAuthStore();
  const store = useLocalStorage<Record<string, EntityPickerState>>(STORAGE_KEY, {});

  const scope = computed(() => `${auth.organizationId ?? 'none'}:${toValue(kind)}`);
  const state = computed(() => store.value[scope.value] ?? emptyState());

  function patch(changes: Partial<EntityPickerState>) {
    store.value = { ...store.value, [scope.value]: { ...state.value, ...changes } };
  }

  /** Position of each id in the recent list; anything unseen sorts after all of them. */
  const rank = computed(() => new Map(state.value.recent.map((id, index) => [id, index])));

  function remember(id: string) {
    patch({ recent: [id, ...state.value.recent.filter((seen) => seen !== id)].slice(0, MAX_RECENT) });
  }

  const sort = computed<EntitySortMode>({
    get: () => state.value.sort,
    set: (value) => patch({ sort: value }),
  });

  const showInactive = computed<boolean>({
    get: () => state.value.showInactive,
    set: (value) => patch({ showInactive: value }),
  });

  return { rank, remember, sort, showInactive };
}
