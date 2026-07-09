import type { AvailabilityEntry, DayPart } from '~/types/availability'
import { MIXED_END_HOUR, MIXED_START_HOUR } from '~/utils/planning/mixedTimelineMath'
import { timeToPx } from '~/utils/planning/timelineMath'

export const DAY_PART_LABELS: Record<DayPart, string> = {
  Morning: 'Ochtend',
  Afternoon: 'Middag',
  Evening: 'Avond',
}

export const DAY_PART_RANGES: Record<DayPart, { startHour: number, endHour: number }> = {
  Morning: { startHour: 6, endHour: 12 },
  Afternoon: { startHour: 12, endHour: 17 },
  Evening: { startHour: 17, endHour: 23 },
}

export interface UnavailablePeriod {
  userId: string
  start: Date
  end: Date
  label: string
  tooltip: string
}

export interface UnavailableOverlay {
  leftPx: number
  widthPx: number
  tooltip: string
}

function parseTimeParts(time: string): { hours: number, minutes: number } {
  const [hours, minutes] = time.split(':').map(Number)
  return { hours: hours ?? 0, minutes: minutes ?? 0 }
}

function toDateKey(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function createLocalDateTime(dateKey: string, time: string): Date {
  const { hours, minutes } = parseTimeParts(time)
  const [year, month, day] = dateKey.split('-').map(Number)
  return new Date(year!, month! - 1, day, hours, minutes, 0, 0)
}

function buildTooltip(entry: AvailabilityEntry, periodLabel: string): string {
  const parts = ['Niet beschikbaar', periodLabel]
  if (entry.source === 'Manager') {
    parts.push(`Aangepast door ${entry.lastModifiedByName}`)
  }
  if (entry.note) {
    parts.push(entry.note)
  }
  return parts.join(' · ')
}

export function getUnavailablePeriodsForDay(
  entries: AvailabilityEntry[],
  userId: string,
  day: Date,
): UnavailablePeriod[] {
  const dateKey = toDateKey(day)
  const periods: UnavailablePeriod[] = []

  for (const entry of entries) {
    if (entry.userId !== userId || entry.date !== dateKey || entry.isAvailable) {
      continue
    }

    if (entry.type === 'DayPart' && entry.dayPart) {
      const range = DAY_PART_RANGES[entry.dayPart]
      const start = new Date(day)
      start.setHours(range.startHour, 0, 0, 0)
      const end = new Date(day)
      end.setHours(range.endHour, 0, 0, 0)
      periods.push({
        userId,
        start,
        end,
        label: DAY_PART_LABELS[entry.dayPart],
        tooltip: buildTooltip(entry, DAY_PART_LABELS[entry.dayPart]),
      })
      continue
    }

    if (entry.type === 'TimeBlock' && entry.startTime && entry.endTime) {
      const start = createLocalDateTime(dateKey, entry.startTime)
      const end = createLocalDateTime(dateKey, entry.endTime)
      const timeLabel = `${entry.startTime.slice(0, 5)}–${entry.endTime.slice(0, 5)}`
      periods.push({
        userId,
        start,
        end,
        label: timeLabel,
        tooltip: buildTooltip(entry, timeLabel),
      })
    }
  }

  return periods
}

export function getUnavailableOverlaysForMatrix(
  entries: AvailabilityEntry[],
  userId: string,
  rangeStart: Date,
  rangeEnd: Date,
  dayWidth: number,
): UnavailableOverlay[] {
  const overlays: UnavailableOverlay[] = []
  const cursor = new Date(rangeStart)
  cursor.setHours(0, 0, 0, 0)

  while (cursor < rangeEnd) {
    const periods = getUnavailablePeriodsForDay(entries, userId, cursor)
    for (const period of periods) {
      overlays.push({
        leftPx: timeToPx(period.start.toISOString(), rangeStart, dayWidth),
        widthPx: Math.max(timeToPx(period.end.toISOString(), rangeStart, dayWidth) - timeToPx(period.start.toISOString(), rangeStart, dayWidth), 4),
        tooltip: period.tooltip,
      })
    }
    cursor.setDate(cursor.getDate() + 1)
  }

  return overlays
}

export function getUnavailableOverlaysForMixed(
  entries: AvailabilityEntry[],
  userId: string,
  day: Date,
  timeToMixedPx: (utcIso: string) => number,
): UnavailableOverlay[] {
  return getUnavailablePeriodsForDay(entries, userId, day).map((period) => {
    const leftPx = Math.max(timeToMixedPx(period.start.toISOString()), 0)
    const rightPx = Math.min(
      timeToMixedPx(period.end.toISOString()),
      (MIXED_END_HOUR - MIXED_START_HOUR) * 80,
    )
    return {
      leftPx,
      widthPx: Math.max(rightPx - leftPx, 4),
      tooltip: period.tooltip,
    }
  })
}

/**
 * Overlap-detectie tussen een dienst en onbeschikbare periodes.
 * Twee tijdsintervallen overlappen als: shiftStart < unavailableEnd && shiftEnd > unavailableStart
 */
export function hasAvailabilityConflict(
  userId: string,
  startUtc: string,
  endUtc: string,
  entries: AvailabilityEntry[],
  userName?: string,
): { hasConflict: boolean, message?: string } {
  const shiftStart = new Date(startUtc)
  const shiftEnd = new Date(endUtc)
  const day = new Date(shiftStart)
  day.setHours(0, 0, 0, 0)

  const periods = getUnavailablePeriodsForDay(entries, userId, day)
  const conflict = periods.find(period =>
    shiftStart < period.end && shiftEnd > period.start,
  )

  if (!conflict) {
    return { hasConflict: false }
  }

  const name = userName ?? 'Medewerker'
  return {
    hasConflict: true,
    message: `${name} heeft aangegeven niet beschikbaar te zijn op dit tijdstip`,
  }
}

export function getDayPartAvailability(
  entries: AvailabilityEntry[],
  userId: string,
  dateKey: string,
  dayPart: DayPart,
): AvailabilityEntry | undefined {
  const dayPartEntry = entries.find(entry =>
    entry.userId === userId
    && entry.date === dateKey
    && entry.type === 'DayPart'
    && entry.dayPart === dayPart,
  )

  if (dayPartEntry) {
    return dayPartEntry
  }

  return entries.find(entry =>
    entry.userId === userId
    && entry.date === dateKey
    && entry.type === 'TimeBlock'
    && entry.startTime
    && entry.endTime
    && !entry.isAvailable
    && timeBlockCoversDayPart(entry.startTime, entry.endTime, dayPart),
  )
}

function timeToMinutes(time: string): number {
  const { hours, minutes } = parseTimeParts(time)
  return hours * 60 + minutes
}

function timeBlockCoversDayPart(startTime: string, endTime: string, dayPart: DayPart): boolean {
  const range = DAY_PART_RANGES[dayPart]
  const blockStart = timeToMinutes(startTime)
  const blockEnd = timeToMinutes(endTime)
  const partStart = range.startHour * 60
  const partEnd = range.endHour * 60
  return blockStart <= partStart && blockEnd >= partEnd
}

export function getTimeBlocksForDay(
  entries: AvailabilityEntry[],
  userId: string,
  dateKey: string,
): AvailabilityEntry[] {
  return entries.filter(entry =>
    entry.userId === userId
    && entry.date === dateKey
    && entry.type === 'TimeBlock',
  )
}
