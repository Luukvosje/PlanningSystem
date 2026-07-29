import type {
  PlanningFilters,
  PlanningRowMode,
  PlanningStatus,
  TimelineZoom,
} from '~/types/planning';
import { startOfMonth, toDateKey } from '~/utils/planning/dateUtils';

export const PLANNING_BOARD_PREFERENCES_VERSION = 1;

const ROW_MODES: PlanningRowMode[] = ['resource', 'customer'];
const ZOOM_LEVELS: TimelineZoom[] = ['15m', '30m', '1h', '2h', '4h', 'day', 'week', 'month'];
const STATUSES: PlanningStatus[] = ['Planned', 'Confirmed', 'Completed', 'Cancelled'];

export const LEGACY_PLANNING_SNAP_KEY = 'planning-snap-to-blocks';

export interface PlanningBoardPreferences {
  version: number
  currentDate: Date
  rowMode: PlanningRowMode
  zoom: TimelineZoom
  slotScale: number
  snapToBlocks: boolean
  filters: PlanningFilters
}

/** Serializable shape stored in localStorage. */
export interface StoredPlanningBoardPreferences {
  version: number
  currentDate: string
  rowMode: PlanningRowMode
  zoom: TimelineZoom
  slotScale: number
  snapToBlocks: boolean
  filters: PlanningFilters
}

export function planningBoardStorageKey(organizationId: string | null | undefined): string {
  return organizationId ?
    `planning:board-preferences:${organizationId}` :
    'planning:board-preferences';
}

export function createDefaultPlanningBoardPreferences(): PlanningBoardPreferences {
  return {
    version: PLANNING_BOARD_PREFERENCES_VERSION,
    currentDate: startOfMonth(new Date()),
    rowMode: 'resource',
    zoom: '1h',
    slotScale: 1,
    snapToBlocks: true,
    filters: {
      userIds: [],
      customerIds: [],
      statuses: [],
      search: '',
    },
  };
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function pickEnum<T extends string>(value: unknown, allowed: readonly T[], fallback: T): T {
  return typeof value === 'string' && (allowed as readonly string[]).includes(value) ?
    value as T :
    fallback;
}

function parseDateKey(value: unknown, fallback: Date): Date {
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return new Date(fallback);
  }

  const [year, month, day] = value.split('-').map(Number);
  if (!year || !month || !day) {
    return new Date(fallback);
  }

  return new Date(year, month - 1, day);
}

function normalizeStringArray(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.filter((item): item is string => typeof item === 'string');
}

function normalizeFilters(value: unknown): PlanningFilters {
  if (!isPlainObject(value)) {
    return createDefaultPlanningBoardPreferences().filters;
  }

  return {
    userIds: normalizeStringArray(value.userIds),
    customerIds: normalizeStringArray(value.customerIds),
    statuses: normalizeStringArray(value.statuses)
      .filter((status): status is PlanningStatus =>
        (STATUSES as readonly string[]).includes(status),
      ),
    search: typeof value.search === 'string' ? value.search : '',
  };
}

function clampSlotScale(_value: unknown, _fallback: number): number {
  // Continuous slot scale removed; always persist 1 for backward compat.
  return 1;
}

export function normalizePlanningBoardPreferences(
  raw: unknown,
  fallback = createDefaultPlanningBoardPreferences(),
): PlanningBoardPreferences {
  const source = isPlainObject(raw) ? raw : {};

  return {
    version: PLANNING_BOARD_PREFERENCES_VERSION,
    currentDate: parseDateKey(source.currentDate, fallback.currentDate),
    rowMode: pickEnum(source.rowMode, ROW_MODES, fallback.rowMode),
    zoom: pickEnum(source.zoom, ZOOM_LEVELS, fallback.zoom),
    slotScale: clampSlotScale(source.slotScale, fallback.slotScale),
    snapToBlocks: typeof source.snapToBlocks === 'boolean' ?
      source.snapToBlocks :
      fallback.snapToBlocks,
    filters: normalizeFilters(source.filters),
  };
}

export function toStoredPlanningBoardPreferences(
  preferences: PlanningBoardPreferences,
): StoredPlanningBoardPreferences {
  return {
    version: PLANNING_BOARD_PREFERENCES_VERSION,
    currentDate: toDateKey(preferences.currentDate),
    rowMode: preferences.rowMode,
    zoom: preferences.zoom,
    slotScale: preferences.slotScale,
    snapToBlocks: preferences.snapToBlocks,
    filters: {
      userIds: [...preferences.filters.userIds],
      customerIds: [...preferences.filters.customerIds],
      statuses: [...preferences.filters.statuses],
      search: preferences.filters.search,
    },
  };
}

export const planningBoardPreferencesSerializer = {
  read(raw: string): PlanningBoardPreferences {
    try {
      return normalizePlanningBoardPreferences(JSON.parse(raw));
    } catch {
      return createDefaultPlanningBoardPreferences();
    }
  },
  write(value: PlanningBoardPreferences): string {
    return JSON.stringify(toStoredPlanningBoardPreferences(value));
  },
};

function readLegacyJson<T>(key: string): T | undefined {
  if (!import.meta.client) {
    return undefined;
  }

  const raw = localStorage.getItem(key);
  if (raw == null) {
    return undefined;
  }

  try {
    return JSON.parse(raw) as T;
  } catch {
    return undefined;
  }
}

/** One-time migration from the old single-key localStorage entries. */
export function readLegacyPlanningBoardPreferences(): Partial<PlanningBoardPreferences> {
  const patch: Partial<PlanningBoardPreferences> = {};

  const snapToBlocks = readLegacyJson<unknown>(LEGACY_PLANNING_SNAP_KEY);
  if (typeof snapToBlocks === 'boolean') {
    patch.snapToBlocks = snapToBlocks;
  }

  return patch;
}

export function clearLegacyPlanningBoardPreferences() {
  if (!import.meta.client) {
    return;
  }

  localStorage.removeItem('planning-layout');
  localStorage.removeItem(LEGACY_PLANNING_SNAP_KEY);
}
