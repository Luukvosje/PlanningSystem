import type { PlanningRecord } from '~/types/planning'
import { pxFromPointerEvent, SNAP_MINUTES, snapPx } from '~/utils/planning/timelineMath'
import { suppressPlanningBlockClick } from '~/composables/planning/useDragPlanning'

const DRAG_THRESHOLD_PX = 4
const MIN_WIDTH_PX = 4

type ResizeState = {
  recordId: string
  edge: 'start' | 'end'
  leftPx: number
  widthPx: number
  assignedUserId: string
  customerId: string | null
}

function computeSnappedResize(
  edge: 'start' | 'end',
  mousePx: number,
  anchorLeftPx: number,
  anchorRightPx: number,
  dayWidth: number,
  minWidth: number,
): Pick<ResizeState, 'edge' | 'leftPx' | 'widthPx'> {
  const snappedPx = snapPx(mousePx, dayWidth)

  if (edge === 'start') {
    let leftPx = Math.min(snappedPx, anchorRightPx - minWidth)
    let widthPx = anchorRightPx - leftPx

    if (widthPx < minWidth) {
      widthPx = minWidth
      leftPx = anchorRightPx - minWidth
    }

    return { edge: 'start', leftPx, widthPx }
  }

  let widthPx = snappedPx - anchorLeftPx

  if (widthPx < minWidth) {
    if (snappedPx < anchorLeftPx) {
      return {
        edge: 'start',
        leftPx: snappedPx,
        widthPx: minWidth,
      }
    }
    widthPx = minWidth
  }

  return { edge: 'end', leftPx: anchorLeftPx, widthPx }
}

export function useResizePlanning() {
  const store = usePlanningStore()
  const api = usePlanningApi()
  const toast = useToast()
  const { canManage } = usePlanningPermissions()
  const { pxToUtcIso, dayWidth } = useTimeline()

  const minDurationPx = computed(() => (SNAP_MINUTES / (24 * 60)) * dayWidth.value)

  const resizingId = ref<string | null>(null)
  const resizePreview = ref<{ recordId: string, leftPx: number, widthPx: number } | null>(null)

  function startResize(
    event: PointerEvent,
    record: PlanningRecord,
    edge: 'start' | 'end',
    layout: { leftPx: number, widthPx: number },
  ) {
    if (!canManage.value) return

    const rowEl = (event.target as HTMLElement).closest<HTMLElement>('[data-timeline-row]')
    if (!rowEl) return

    event.preventDefault()
    event.stopPropagation()

    store.selectPlanning(record.id, { openSidebar: false })

    resizingId.value = record.id

    const anchorLeftPx = layout.leftPx
    const anchorRightPx = layout.leftPx + layout.widthPx
    const startMousePx = pxFromPointerEvent(event, rowEl)

    let state: ResizeState = {
      recordId: record.id,
      edge,
      leftPx: layout.leftPx,
      widthPx: layout.widthPx,
      assignedUserId: record.assignedUserId,
      customerId: record.customerId ?? null,
    }

    let movedPx = 0

    const updatePreview = () => {
      resizePreview.value = {
        recordId: record.id,
        leftPx: state.leftPx,
        widthPx: state.widthPx,
      }
    }

    updatePreview()

    const onMove = (e: PointerEvent) => {
      const mousePx = pxFromPointerEvent(e, rowEl)
      movedPx = Math.max(movedPx, Math.abs(mousePx - startMousePx))

      const minWidth = Math.max(minDurationPx.value, MIN_WIDTH_PX)
      const resized = computeSnappedResize(
        state.edge,
        mousePx,
        anchorLeftPx,
        anchorRightPx,
        dayWidth.value,
        minWidth,
      )
      state = { ...state, ...resized }
      updatePreview()
    }

    const onUp = async (e: PointerEvent) => {
      document.removeEventListener('pointermove', onMove)
      document.removeEventListener('pointerup', onUp)

      const mousePx = pxFromPointerEvent(e, rowEl)
      const minWidth = Math.max(minDurationPx.value, MIN_WIDTH_PX)
      const resized = computeSnappedResize(
        state.edge,
        mousePx,
        anchorLeftPx,
        anchorRightPx,
        dayWidth.value,
        minWidth,
      )
      state = { ...state, ...resized }

      resizingId.value = null
      resizePreview.value = null

      if (state.widthPx < minWidth) {
        return
      }

      if (movedPx >= DRAG_THRESHOLD_PX) {
        suppressPlanningBlockClick(record.id)
      }

      const startUtc = pxToUtcIso(state.leftPx)
      const endUtc = pxToUtcIso(state.leftPx + state.widthPx)

      store.applyOptimisticPatch(record.id, { startUtc, endUtc })

      try {
        await api.move(record.id, {
          assignedUserId: state.assignedUserId,
          customerId: state.customerId,
          startUtc,
          endUtc,
        })
        await api.invalidatePlanning()
        store.clearOptimisticPatch(record.id)
      }
      catch {
        store.clearOptimisticPatch(record.id)
        toast.add({ title: 'Aanpassen mislukt', color: 'error' })
      }
    }

    document.addEventListener('pointermove', onMove)
    document.addEventListener('pointerup', onUp)
  }

  return {
    resizingId,
    resizePreview,
    startResize,
  }
}
