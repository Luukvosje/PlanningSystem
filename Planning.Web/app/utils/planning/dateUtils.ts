import type { PlanningStatus } from '~/types/planning'

export const STATUS_COLORS: Record<PlanningStatus, string> = {
  Planned: '#6366F1',
  Confirmed: '#14B8A6',
  Completed: '#64748B',
  Cancelled: '#F43F5E',
}

export const COLOR_PRESETS = [
  '#6366F1',
  '#14B8A8',
  '#F59E0B',
  '#EF4444',
  '#8B5CF6',
  '#EC4899',
  '#06B6D4',
  '#84CC16',
]

export function getStatusLabel(status: PlanningStatus): string {
  const labels: Record<PlanningStatus, string> = {
    Planned: 'Gepland',
    Confirmed: 'Bevestigd',
    Completed: 'Afgerond',
    Cancelled: 'Geannuleerd',
  }
  return labels[status]
}

export function getBlockColor(color?: string | null, status?: PlanningStatus): string {
  if (color) return color
  if (status) return STATUS_COLORS[status]
  return STATUS_COLORS.Planned
}

export function getMonday(date: Date): Date {
  const copy = new Date(date)
  const day = copy.getDay()
  const diff = day === 0 ? -6 : 1 - day
  copy.setDate(copy.getDate() + diff)
  copy.setHours(0, 0, 0, 0)
  return copy
}

export function addDays(date: Date, days: number): Date {
  const copy = new Date(date)
  copy.setDate(copy.getDate() + days)
  return copy
}

export function toUtcIso(date: Date): string {
  return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate())).toISOString()
}

export function toUtcDateTimeIso(date: Date): string {
  return date.toISOString()
}

export function snapToMinutes(date: Date, minutes: number): Date {
  const copy = new Date(date)
  const totalMinutes = copy.getHours() * 60 + copy.getMinutes()
  const snapped = Math.round(totalMinutes / minutes) * minutes
  copy.setHours(Math.floor(snapped / 60), snapped % 60, 0, 0)
  return copy
}

export function formatTimeRange(startUtc: string, endUtc: string): string {
  const formatter = new Intl.DateTimeFormat('nl-NL', {
    hour: '2-digit',
    minute: '2-digit',
  })
  return `${formatter.format(new Date(startUtc))} – ${formatter.format(new Date(endUtc))}`
}

export function formatDayHeader(date: Date): string {
  return new Intl.DateTimeFormat('nl-NL', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  }).format(date)
}

export function isWeekend(date: Date): boolean {
  const day = date.getDay()
  return day === 0 || day === 6
}
