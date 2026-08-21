import type { RemovableRef } from '@vueuse/core';
import { useLocalStorage } from '@vueuse/core';
import type { PlanningBoardPreferences } from '~/utils/planning/boardPreferences';
import {
  clearLegacyPlanningBoardPreferences,
  createDefaultPlanningBoardPreferences,
  planningBoardPreferencesSerializer,
  planningBoardStorageKey,
  readLegacyPlanningBoardPreferences,
} from '~/utils/planning/boardPreferences';

export type PlanningBoardPreferencesRef = RemovableRef<PlanningBoardPreferences>

/**
 * Persists PlanningBoard UI state (date, zoom, filters, …) in localStorage.
 * Scoped per organization so filters/ids stay valid when switching orgs.
 */
export function usePlanningBoardPreferences(): {
  preferences: PlanningBoardPreferencesRef
  resetPreferences: () => void
} {
  const auth = useAuthStore();

  const storageKey = computed(() => planningBoardStorageKey(auth.organizationId));

  const preferences = useLocalStorage<PlanningBoardPreferences>(
    storageKey,
    createDefaultPlanningBoardPreferences,
    {
      deep: true,
      serializer: planningBoardPreferencesSerializer,
    },
  );

  migrateLegacyPreferences(preferences);

  function resetPreferences() {
    preferences.value = createDefaultPlanningBoardPreferences();
  }

  return {
    preferences,
    resetPreferences,
  };
}

function migrateLegacyPreferences(preferences: PlanningBoardPreferencesRef) {
  if (!import.meta.client) {
    return;
  }

  const legacy = readLegacyPlanningBoardPreferences();
  if (Object.keys(legacy).length === 0) {
    return;
  }

  preferences.value = {
    ...preferences.value,
    ...legacy,
  };
  clearLegacyPlanningBoardPreferences();
}
