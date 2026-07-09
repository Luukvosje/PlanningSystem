import { defineStore } from 'pinia'
import type {
  ContextMenuState,
  CreatePlanningDraft,
  CreatePopoverState,
  PlanningFilters,
  PlanningRecord,
  PlanningRowMode,
  PlanningViewMode,
  TimelineZoom,
} from '~/types/planning'
import { getMonday } from '~/utils/planning/dateUtils'
import { MAX_SLOT_SCALE, MIN_SLOT_SCALE } from '~/utils/planning/timelineMath'

export const usePlanningStore = defineStore('planning', () => {
  const currentDate = ref(getMonday(new Date()))
  const viewMode = ref<PlanningViewMode>('weekly')
  const rowMode = ref<PlanningRowMode>('resource')
  const zoom = ref<TimelineZoom>('1h')
  const slotScale = ref(1)

  function adjustSlotScale(factor: number) {
    slotScale.value = Math.min(MAX_SLOT_SCALE, Math.max(MIN_SLOT_SCALE, slotScale.value * factor))
  }

  function resetSlotScale() {
    slotScale.value = 1
  }

  const selectedPlanningId = ref<string | null>(null)
  const sidebarOpen = ref(false)
  const sidebarMode = ref<'edit' | 'create'>('edit')
  const createDraft = ref<CreatePlanningDraft | null>(null)

  const filters = ref<PlanningFilters>({
    userIds: [],
    customerIds: [],
    statuses: [],
    search: '',
  })

  const optimisticPatches = ref<Map<string, Partial<PlanningRecord>>>(new Map())
  const contextMenu = ref<ContextMenuState | null>(null)
  const createPopover = ref<CreatePopoverState | null>(null)

  function selectPlanning(id: string | null, options?: { openSidebar?: boolean }) {
    selectedPlanningId.value = id
    if (options?.openSidebar) {
      sidebarOpen.value = !!id
    }
  }

  function openEdit(id: string) {
    selectedPlanningId.value = id
    sidebarMode.value = 'edit'
    createDraft.value = null
    sidebarOpen.value = true
  }

  function openCreateSidebar(state: CreatePlanningDraft) {
    selectedPlanningId.value = null
    sidebarMode.value = 'create'
    createDraft.value = state
    sidebarOpen.value = true
    closeContextMenu()
    closeCreatePopover()
  }

  function closeSidebar() {
    sidebarOpen.value = false
    sidebarMode.value = 'edit'
    createDraft.value = null
  }

  function clearSelection() {
    selectedPlanningId.value = null
    sidebarOpen.value = false
    sidebarMode.value = 'edit'
    createDraft.value = null
  }

  function applyOptimisticPatch(id: string, patch: Partial<PlanningRecord>) {
    const next = new Map(optimisticPatches.value)
    const current = next.get(id) ?? {}
    next.set(id, { ...current, ...patch })
    optimisticPatches.value = next
  }

  function clearOptimisticPatch(id: string) {
    if (!optimisticPatches.value.has(id)) return
    const next = new Map(optimisticPatches.value)
    next.delete(id)
    optimisticPatches.value = next
  }

  function openContextMenu(state: ContextMenuState) {
    contextMenu.value = state
  }

  function closeContextMenu() {
    contextMenu.value = null
  }

  function openCreatePopover(state: CreatePopoverState) {
    createPopover.value = state
    closeContextMenu()
  }

  function closeCreatePopover() {
    createPopover.value = null
  }

  function goToToday() {
    currentDate.value = getMonday(new Date())
  }

  function navigatePrevious() {
    const days = viewMode.value === 'daily' ? 1 : viewMode.value === 'monthly' ? 30 : 7
    const next = new Date(currentDate.value)
    next.setDate(next.getDate() - days)
    currentDate.value = next
  }

  function navigateNext() {
    const days = viewMode.value === 'daily' ? 1 : viewMode.value === 'monthly' ? 30 : 7
    const next = new Date(currentDate.value)
    next.setDate(next.getDate() + days)
    currentDate.value = next
  }

  return {
    currentDate,
    viewMode,
    rowMode,
    zoom,
    slotScale,
    adjustSlotScale,
    resetSlotScale,
    selectedPlanningId,
    sidebarOpen,
    sidebarMode,
    createDraft,
    filters,
    optimisticPatches,
    contextMenu,
    createPopover,
    selectPlanning,
    openEdit,
    openCreateSidebar,
    closeSidebar,
    clearSelection,
    applyOptimisticPatch,
    clearOptimisticPatch,
    openContextMenu,
    closeContextMenu,
    openCreatePopover,
    closeCreatePopover,
    goToToday,
    navigatePrevious,
    navigateNext,
  }
})
