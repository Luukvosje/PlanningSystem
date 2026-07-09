import type { PlanningViewMode, TimelineZoom } from '~/types/planning'
import { addDays, getMonday } from './dateUtils'

export const ROW_LABEL_WIDTH = 200
export const BLOCK_PADDING = 4
export const LANE_HEIGHT = 52
export const BASE_ROW_HEIGHT = LANE_HEIGHT + BLOCK_PADDING * 2
export const SNAP_MINUTES = 15

/** Width of one zoom-interval column — fixed at every zoom level; finer zoom = wider day + horizontal scroll. */
export const SLOT_WIDTH = 40

export const MIN_SLOT_SCALE = 0.25
export const MAX_SLOT_SCALE = 4

export function getEffectiveSlotWidth(slotScale: number): number {
  return SLOT_WIDTH * slotScale
}

const ZOOM_MINUTES: Record<TimelineZoom, number> = {
  '15m': 15,
  '30m': 30,
  '1h': 60,
  '2h': 120,
  '4h': 240,
  day: 24 * 60,
  week: 7 * 24 * 60,
}

export function getMinZoomDayWidth(zoom: TimelineZoom, slotScale = 1): number {
  const slotMinutes = ZOOM_MINUTES[zoom]
  const effectiveSlotWidth = getEffectiveSlotWidth(slotScale)
  if (slotMinutes >= 24 * 60) return effectiveSlotWidth
  return ((24 * 60) / slotMinutes) * effectiveSlotWidth
}

export function getDayWidth(
  zoom: TimelineZoom,
  dayCount: number,
  containerWidth: number,
  slotScale = 1,
): number {
  const minZoomDayWidth = getMinZoomDayWidth(zoom, slotScale)
  if (containerWidth <= 0 || dayCount <= 0) return minZoomDayWidth

  const fitDayWidth = (containerWidth - ROW_LABEL_WIDTH) / dayCount
  return Math.max(fitDayWidth, minZoomDayWidth)
}

export function getZoomMinutes(zoom: TimelineZoom): number {
  return ZOOM_MINUTES[zoom]
}

export function computeDateRange(currentDate: Date, viewMode: PlanningViewMode) {
  if (viewMode === 'daily') {
    const start = new Date(currentDate)
    start.setHours(0, 0, 0, 0)
    return { start, end: addDays(start, 1) }
  }

  if (viewMode === 'monthly') {
    const start = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1)
    const end = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1)
    return { start, end }
  }

  const start = getMonday(currentDate)
  return { start, end: addDays(start, 7) }
}

export function getDayCount(start: Date, end: Date): number {
  return Math.ceil((end.getTime() - start.getTime()) / (24 * 60 * 60 * 1000))
}

export function timeToPx(
  utcIso: string,
  rangeStart: Date,
  dayWidth: number,
): number {
  const diffMs = new Date(utcIso).getTime() - rangeStart.getTime()
  return (diffMs / (24 * 60 * 60 * 1000)) * dayWidth
}

export function snapPx(px: number, dayWidth: number, snapMinutes = SNAP_MINUTES): number {
  if (snapMinutes <= 0) return px
  const snapWidthPx = (snapMinutes / (24 * 60)) * dayWidth
  return Math.round(px / snapWidthPx) * snapWidthPx
}

export function pxToUtcIso(
  px: number,
  rangeStart: Date,
  dayWidth: number,
  snapMinutes = 0,
): string {
  const finalPx = snapMinutes > 0 ? snapPx(px, dayWidth, snapMinutes) : px
  const msPerPx = (24 * 60 * 60 * 1000) / dayWidth
  return new Date(rangeStart.getTime() + finalPx * msPerPx).toISOString()
}

export function pxToTime(
  px: number,
  rangeStart: Date,
  dayWidth: number,
): Date {
  const dayIndex = Math.floor(px / dayWidth)
  const dayFraction = (px % dayWidth) / dayWidth
  const result = addDays(rangeStart, dayIndex)
  const totalMinutes = Math.round(dayFraction * 24 * 60 / SNAP_MINUTES) * SNAP_MINUTES
  result.setHours(Math.floor(totalMinutes / 60), totalMinutes % 60, 0, 0)
  return result
}

export function getTimelineWidth(dayCount: number, dayWidth: number): number {
  return dayCount * dayWidth
}

/** Below this slot width (px) the time label is hidden and shown in a tooltip instead. */
export const MIN_LABEL_SLOT_WIDTH = 32

export interface TimeSlotMarker {
  leftPx: number
  width: number
  label: string
  showLabel: boolean
}

export function getTimeSlotMarkers(zoom: TimelineZoom, dayWidth: number): TimeSlotMarker[] {
  const slotMinutes = ZOOM_MINUTES[zoom]
  if (slotMinutes >= 24 * 60) return []

  const slotsPerDay = (24 * 60) / slotMinutes
  const slotWidth = dayWidth / slotsPerDay
  const showLabel = slotWidth >= MIN_LABEL_SLOT_WIDTH

  const timeOptions: Intl.DateTimeFormatOptions = slotMinutes < 60
    ? { hour: '2-digit', minute: '2-digit' }
    : { hour: '2-digit' }

  const formatter = new Intl.DateTimeFormat('nl-NL', timeOptions)
  const markers: TimeSlotMarker[] = []

  for (let i = 0; i < slotsPerDay; i++) {
    const totalMinutes = i * slotMinutes
    const date = new Date(2000, 0, 1, Math.floor(totalMinutes / 60), totalMinutes % 60)
    markers.push({
      leftPx: i * slotWidth,
      width: slotWidth,
      label: formatter.format(date),
      showLabel,
    })
  }

  return markers
}

export function showsTimeSlots(zoom: TimelineZoom): boolean {
  return ZOOM_MINUTES[zoom] < 24 * 60
}

export function getCurrentTimePx(rangeStart: Date, rangeEnd: Date, dayWidth: number): number | null {
  const now = new Date()
  if (now < rangeStart || now >= rangeEnd) return null
  return timeToPx(now.toISOString(), rangeStart, dayWidth)
}

export function pxFromPointerEvent(event: PointerEvent, timelineRow: HTMLElement): number {
  const rect = timelineRow.getBoundingClientRect()
  return event.clientX - rect.left
}
