import type { Weekday } from '~/types/availability'
import type { DayTimeWindow } from '~/utils/planning/timelineMath'
import { timeToPxCompact } from '~/utils/planning/timelineMath'

export interface OpeningHoursEntry {
  day: Weekday
  openTime: string
  closeTime: string
}

export const SMART_SNAP_TOLERANCE_MINUTES = 5

export const DEFAULT_IMPORTANT_WORK_TIMES = ['06:00', '09:00', '13:00', '17:00', '21:00']

const JS_DAY_TO_WEEKDAY: Record<number, Weekday> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday',
}

export function normalizeTimeValue(value: string): string {
  const [hours, minutes] = value.split(':')
  return `${hours!.padStart(2, '0')}:${minutes!.padStart(2, '0')}`
}

export function parseTimeToMinutes(time: string): number {
  const normalized = normalizeTimeValue(time)
  const [hours, minutes] = normalized.split(':').map(Number)
  return hours! * 60 + minutes!
}

export function minutesToDayPx(minutes: number, dayWidth: number): number {
  return (minutes / (24 * 60)) * dayWidth
}

export function weekdayFromDate(date: Date): Weekday {
  return JS_DAY_TO_WEEKDAY[date.getDay()]!
}

export function getOpeningHoursForDate(
  date: Date,
  openingHours: OpeningHoursEntry[],
): OpeningHoursEntry | null {
  const weekday = weekdayFromDate(date)
  return openingHours.find(entry => entry.day === weekday) ?? null
}

export function getOutsideOpeningOverlays(
  date: Date,
  dayWidth: number,
  openingHours: OpeningHoursEntry[],
): { left: number, width: number }[] {
  const hours = getOpeningHoursForDate(date, openingHours)
  if (!hours) return []

  const openPx = minutesToDayPx(parseTimeToMinutes(hours.openTime), dayWidth)
  const closePx = minutesToDayPx(parseTimeToMinutes(hours.closeTime), dayWidth)
  const overlays: { left: number, width: number }[] = []

  if (openPx > 0) {
    overlays.push({ left: 0, width: openPx })
  }

  if (closePx < dayWidth) {
    overlays.push({ left: closePx, width: dayWidth - closePx })
  }

  return overlays
}

function timeToCompactDayPx(date: Date, dayWidth: number, window: DayTimeWindow): number {
  return timeToPxCompact(date.toISOString(), window, dayWidth)
}

export function getOutsideOpeningOverlaysCompact(
  date: Date,
  dayWidth: number,
  openingHours: OpeningHoursEntry[],
  window: DayTimeWindow,
): { left: number, width: number }[] {
  const hours = getOpeningHoursForDate(date, openingHours)
  if (!hours) return []

  const dayStart = new Date(date)
  dayStart.setHours(0, 0, 0, 0)
  const openDate = new Date(dayStart.getTime() + parseTimeToMinutes(hours.openTime) * 60_000)
  const closeDate = new Date(dayStart.getTime() + parseTimeToMinutes(hours.closeTime) * 60_000)

  const openPx = timeToCompactDayPx(openDate, dayWidth, window)
  const closePx = timeToCompactDayPx(closeDate, dayWidth, window)
  const overlays: { left: number, width: number }[] = []

  if (openPx > 0) {
    overlays.push({ left: 0, width: openPx })
  }

  if (closePx < dayWidth) {
    overlays.push({ left: closePx, width: dayWidth - closePx })
  }

  return overlays
}

export function getImportantTimePxInDay(time: string, dayWidth: number): number {
  return minutesToDayPx(parseTimeToMinutes(time), dayWidth)
}

export function collectImportantTimeSnapPoints(
  importantTimes: string[],
  dayCount: number,
  dayWidth: number,
): number[] {
  const points: number[] = []

  for (let dayIndex = 0; dayIndex < dayCount; dayIndex++) {
    const dayOffset = dayIndex * dayWidth
    for (const time of importantTimes) {
      points.push(dayOffset + getImportantTimePxInDay(time, dayWidth))
    }
  }

  return points
}

export function snapPxToImportantTime(
  px: number,
  dayWidth: number,
  importantTimes: string[],
  toleranceMinutes = SMART_SNAP_TOLERANCE_MINUTES,
): number | null {
  if (importantTimes.length === 0 || dayWidth <= 0) return null

  const dayIndex = Math.floor(px / dayWidth)
  const pxInDay = px - dayIndex * dayWidth
  const minutesInDay = (pxInDay / dayWidth) * 24 * 60

  let nearestPx: number | null = null
  let nearestDistance = Infinity

  for (const time of importantTimes) {
    const targetMinutes = parseTimeToMinutes(time)
    const distance = Math.abs(minutesInDay - targetMinutes)

    if (distance <= toleranceMinutes && distance < nearestDistance) {
      nearestDistance = distance
      nearestPx = dayIndex * dayWidth + getImportantTimePxInDay(time, dayWidth)
    }
  }

  return nearestPx
}

export function isImportantTimeSlot(
  slotMinutes: number,
  importantTimes: string[],
): boolean {
  return importantTimes.some(time => parseTimeToMinutes(time) === slotMinutes)
}

export function getISOWeekNumber(date: Date): number {
  const copy = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()))
  copy.setUTCDate(copy.getUTCDate() + 4 - (copy.getUTCDay() || 7))
  const yearStart = new Date(Date.UTC(copy.getUTCFullYear(), 0, 1))
  return Math.ceil((((copy.getTime() - yearStart.getTime()) / 86_400_000) + 1) / 7)
}

export function formatWeekDateRange(start: Date, end: Date): string {
  const formatter = new Intl.DateTimeFormat('nl-NL', { day: 'numeric', month: 'short' })
  return `${formatter.format(start)} – ${formatter.format(end)}`
}

export function sortImportantTimes(times: string[]): string[] {
  return [...times]
    .map(normalizeTimeValue)
    .sort((left, right) => parseTimeToMinutes(left) - parseTimeToMinutes(right))
}

export function uniqueImportantTimes(times: string[]): string[] {
  const seen = new Set<string>()
  return sortImportantTimes(times).filter((time) => {
    if (seen.has(time)) return false
    seen.add(time)
    return true
  })
}
