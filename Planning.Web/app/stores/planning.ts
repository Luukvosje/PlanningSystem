import { defineStore } from 'pinia';
import type {
  ContextMenuState,
  CreatePlanningDraft,
  PlanningFilters,
  PlanningRecord,
  PlanningRowMode,
  TimelineZoom,
} from '~/types/planning';
import {
  addDays,
  addMonths,
  startOfMonth,
} from '~/utils/planning/dateUtils';
import {
  canZoomIn,
  canZoomOut,
  getDayCount,
  ROW_LABEL_WIDTH,
  shiftDateByZoomPeriod,
  stepZoomIn,
  stepZoomOut,
  timelineLeftForDate,
} from '~/utils/planning/timelineMath';

export interface TimelineViewport {
  scrollLeft: number
  clientWidth: number
  dayWidth: number
}

export interface TimelineScrollRequest {
  token: number
  date: Date
}

export const usePlanningStore = defineStore('planning', () => {
  const auth = useAuthStore();
  const { preferences, resetPreferences } = usePlanningBoardPreferences();

  const currentDate = computed({
    get: () => preferences.value.currentDate,
    set: (value: Date) => {
      preferences.value.currentDate = value;
    },
  });

  const rowMode = computed({
    get: () => preferences.value.rowMode,
    set: (value: PlanningRowMode) => {
      preferences.value.rowMode = value;
    },
  });

  const zoom = computed({
    get: () => preferences.value.zoom,
    set: (value: TimelineZoom) => {
      preferences.value.zoom = value;
      preferences.value.slotScale = 1;
    },
  });

  /** Always 1 — continuous scale removed; kept for timeline math call sites. */
  const slotScale = computed(() => 1);

  const snapToBlocks = computed({
    get: () => preferences.value.snapToBlocks,
    set: (value: boolean) => {
      preferences.value.snapToBlocks = value;
    },
  });

  const rowLayout = computed({
    get: () => preferences.value.rowLayout,
    set: (value: 'compact' | 'spacious') => {
      preferences.value.rowLayout = value;
    },
  });

  const showAvailability = computed({
    get: () => preferences.value.showAvailability,
    set: (value: boolean) => {
      preferences.value.showAvailability = value;
    },
  });

  const showConcepts = computed({
    get: () => preferences.value.showConcepts,
    set: (value: boolean) => {
      preferences.value.showConcepts = value;
    },
  });

  const showBlockColor = computed({
    get: () => preferences.value.showBlockColor,
    set: (value: boolean) => {
      preferences.value.showBlockColor = value;
    },
  });

  const showWeekends = computed({
    get: () => preferences.value.showWeekends,
    set: (value: boolean) => {
      preferences.value.showWeekends = value;
    },
  });

  const showFullDay = computed({
    get: () => preferences.value.showFullDay,
    set: (value: boolean) => {
      preferences.value.showFullDay = value;
    },
  });

  const filters = computed({
    get: () => preferences.value.filters,
    set: (value: PlanningFilters) => {
      preferences.value.filters = value;
    },
  });

  /** Inclusive start of the loaded buffer (1st of month, local midnight). */
  const loadedRangeStart = ref(startOfMonth(currentDate.value));
  /** Exclusive end of the loaded buffer (1st of month after last loaded month). */
  const loadedRangeEnd = ref(addMonths(startOfMonth(currentDate.value), 1));

  const canZoomInLevel = computed(() => canZoomIn(zoom.value));
  const canZoomOutLevel = computed(() => canZoomOut(zoom.value));

  const timelineViewport = ref<TimelineViewport>({
    scrollLeft: 0,
    clientWidth: 0,
    dayWidth: 0,
  });
  const scrollRequest = ref<TimelineScrollRequest | null>(null);

  function zoomIn() {
    if (!canZoomIn(zoom.value)) {
      return;
    }
    zoom.value = stepZoomIn(zoom.value);
  }

  function zoomOut() {
    if (!canZoomOut(zoom.value)) {
      return;
    }
    zoom.value = stepZoomOut(zoom.value);
  }

  function resetLoadedRangeToMonth(date: Date = currentDate.value) {
    const start = startOfMonth(date);
    loadedRangeStart.value = start;
    loadedRangeEnd.value = addMonths(start, 1);
  }

  function startOfDay(date: Date): Date {
    const copy = new Date(date);
    copy.setHours(0, 0, 0, 0);
    return copy;
  }

  function updateTimelineViewport(viewport: TimelineViewport) {
    timelineViewport.value = viewport;
  }

  function requestScrollToDate(date: Date) {
    const target = startOfDay(date);
    currentDate.value = target;
    scrollRequest.value = {
      token: (scrollRequest.value?.token ?? 0) + 1,
      date: target,
    };
  }

  function getViewportAnchorDate(): Date {
    const { scrollLeft, clientWidth, dayWidth } = timelineViewport.value;
    if (dayWidth <= 0 || clientWidth <= 0) {
      return startOfDay(currentDate.value);
    }

    const centerPx = scrollLeft + clientWidth / 2;
    const daysPx = Math.max(0, centerPx - ROW_LABEL_WIDTH);
    const dayIndex = Math.floor(daysPx / dayWidth);
    return addDays(loadedRangeStart.value, dayIndex);
  }

  const isTodayInView = computed(() => {
    const { scrollLeft, clientWidth, dayWidth } = timelineViewport.value;
    if (dayWidth <= 0 || clientWidth <= 0) {
      return true;
    }

    const today = startOfDay(new Date());
    if (today < loadedRangeStart.value || today >= loadedRangeEnd.value) {
      return false;
    }

    const todayLeft = ROW_LABEL_WIDTH + timelineLeftForDate(
      today,
      loadedRangeStart.value,
      dayWidth,
      showWeekends.value,
    );
    const todayRight = todayLeft + dayWidth;
    const visibleLeft = scrollLeft + ROW_LABEL_WIDTH;
    const visibleRight = scrollLeft + clientWidth;

    return todayLeft < visibleRight && todayRight > visibleLeft;
  });

  /** Prepend one calendar month. Returns days added (for scroll preservation). */
  function extendPreviousMonth(): number {
    const previousStart = addMonths(loadedRangeStart.value, -1);
    const addedDays = getDayCount(previousStart, loadedRangeStart.value);
    loadedRangeStart.value = previousStart;
    return addedDays;
  }

  /** Append one calendar month. */
  function extendNextMonth(): void {
    loadedRangeEnd.value = addMonths(loadedRangeEnd.value, 1);
  }

  const selectedPlanningId = ref<string | null>(null);
  const sidebarOpen = ref(false);
  const sidebarMode = ref<'edit' | 'create'>('edit');
  const createDraft = ref<CreatePlanningDraft | null>(null);

  const optimisticPatches = ref<Map<string, Partial<PlanningRecord>>>(new Map());
  const contextMenu = ref<ContextMenuState | null>(null);

  function selectPlanning(id: string | null, options?: { openSidebar?: boolean }) {
    selectedPlanningId.value = id;
    if (options?.openSidebar) {
      sidebarOpen.value = !!id;
    }
  }

  function openEdit(id: string) {
    selectedPlanningId.value = id;
    sidebarMode.value = 'edit';
    createDraft.value = null;
    sidebarOpen.value = true;
  }

  function openCreateSidebar(state: CreatePlanningDraft) {
    selectedPlanningId.value = null;
    sidebarMode.value = 'create';
    createDraft.value = state;
    sidebarOpen.value = true;
    closeContextMenu();
  }

  function closeSidebar() {
    sidebarOpen.value = false;
    sidebarMode.value = 'edit';
    createDraft.value = null;
  }

  function clearSelection() {
    selectedPlanningId.value = null;
    sidebarOpen.value = false;
    sidebarMode.value = 'edit';
    createDraft.value = null;
  }

  function applyOptimisticPatch(id: string, patch: Partial<PlanningRecord>) {
    const next = new Map(optimisticPatches.value);
    const current = next.get(id) ?? {};
    next.set(id, { ...current, ...patch });
    optimisticPatches.value = next;
  }

  function clearOptimisticPatch(id: string) {
    if (!optimisticPatches.value.has(id)) {
      return;
    }
    const next = new Map(optimisticPatches.value);
    next.delete(id);
    optimisticPatches.value = next;
  }

  function openContextMenu(state: ContextMenuState) {
    contextMenu.value = state;
  }

  function closeContextMenu() {
    contextMenu.value = null;
  }

  function goToToday() {
    const today = startOfDay(new Date());
    if (today < loadedRangeStart.value || today >= loadedRangeEnd.value) {
      currentDate.value = startOfMonth(today);
      resetLoadedRangeToMonth(today);
    }
    requestScrollToDate(today);
  }

  function shiftPeriod(direction: -1 | 1) {
    const anchor = getViewportAnchorDate();
    const target = shiftDateByZoomPeriod(anchor, zoom.value, direction);
    requestScrollToDate(target);
  }

  function resetAllPreferences() {
    resetPreferences();
    resetLoadedRangeToMonth(currentDate.value);
  }

  watch(
    () => auth.organizationId,
    () => {
      resetLoadedRangeToMonth(currentDate.value);
    },
  );

  return {
    currentDate,
    rowMode,
    zoom,
    slotScale,
    snapToBlocks,
    rowLayout,
    showAvailability,
    showConcepts,
    showBlockColor,
    showWeekends,
    showFullDay,
    filters,
    loadedRangeStart,
    loadedRangeEnd,
    resetLoadedRangeToMonth,
    extendPreviousMonth,
    extendNextMonth,
    canZoomInLevel,
    canZoomOutLevel,
    zoomIn,
    zoomOut,
    timelineViewport,
    updateTimelineViewport,
    scrollRequest,
    isTodayInView,
    requestScrollToDate,
    shiftPeriod,
    resetPreferences: resetAllPreferences,
    selectedPlanningId,
    sidebarOpen,
    sidebarMode,
    createDraft,
    optimisticPatches,
    contextMenu,
    selectPlanning,
    openEdit,
    openCreateSidebar,
    closeSidebar,
    clearSelection,
    applyOptimisticPatch,
    clearOptimisticPatch,
    openContextMenu,
    closeContextMenu,
    goToToday,
  };
});
