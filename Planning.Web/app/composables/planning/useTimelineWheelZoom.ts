import { ROW_LABEL_WIDTH } from '~/utils/planning/timelineMath'

const ZOOM_FACTOR = 1.1

export function useTimelineWheelZoom(containerRef: Ref<HTMLElement | null>) {
  const store = usePlanningStore()

  function onWheel(event: WheelEvent) {
    if (!event.ctrlKey && !event.metaKey) return

    const container = containerRef.value
    if (!container) return

    event.preventDefault()

    const scaleBefore = store.slotScale
    const factor = event.deltaY > 0 ? 1 / ZOOM_FACTOR : ZOOM_FACTOR
    store.adjustSlotScale(factor)

    if (store.slotScale === scaleBefore) return

    const rect = container.getBoundingClientRect()
    const pointerX = event.clientX - rect.left
    const contentX = container.scrollLeft + pointerX
    const timelineX = contentX - ROW_LABEL_WIDTH
    const ratio = store.slotScale / scaleBefore
    const timelineXAfter = timelineX * ratio
    const contentXAfter = timelineXAfter + ROW_LABEL_WIDTH

    container.scrollLeft = contentXAfter - pointerX
  }

  onMounted(() => {
    containerRef.value?.addEventListener('wheel', onWheel, { passive: false })
  })

  onUnmounted(() => {
    containerRef.value?.removeEventListener('wheel', onWheel)
  })
}
