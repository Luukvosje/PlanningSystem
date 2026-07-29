import type { PlanningStatus } from '~/types/planning';

export const STATUS_COLORS: Record<PlanningStatus, string> = {
  Planned: '#6366F1',
  Confirmed: '#14B8A6',
  Completed: '#64748B',
  Cancelled: '#F43F5E',
};

export const COLOR_PRESETS = [
  '#6366F1',
  '#14B8A8',
  '#F59E0B',
  '#EF4444',
  '#8B5CF6',
  '#EC4899',
  '#06B6D4',
  '#84CC16',
];

export function getStatusLabel(status: PlanningStatus): string {
  const labels: Record<PlanningStatus, string> = {
    Planned: 'Gepland',
    Confirmed: 'Bevestigd',
    Completed: 'Afgerond',
    Cancelled: 'Geannuleerd',
  };
  return labels[status];
}

export function getBlockColor(color?: string | null, status?: PlanningStatus): string {
  if (color)
    return color;
  if (status) {
    return STATUS_COLORS[status];
  }
  return STATUS_COLORS.Planned;
}

export function getMonday(date: Date): Date {
  const copy = new Date(date);
  const day = copy.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  copy.setDate(copy.getDate() + diff);
  copy.setHours(0, 0, 0, 0);
  return copy;
}

export function addDays(date: Date, days: number): Date {
  const copy = new Date(date);
  copy.setDate(copy.getDate() + days);
  return copy;
}

/** First day of the calendar month containing `date` (local midnight). */
export function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

/** Add calendar months, landing on the 1st at local midnight. */
export function addMonths(date: Date, months: number): Date {
  return new Date(date.getFullYear(), date.getMonth() + months, 1);
}

/** Exclusive end of the calendar month containing `date` (1st of next month). */
export function endOfMonthExclusive(date: Date): Date {
  return addMonths(startOfMonth(date), 1);
}

/**
 * List each calendar month in `[rangeStart, rangeEnd)` as `{ start, end }`
 * where both bounds are month starts (end exclusive).
 */
export function eachMonthInRange(rangeStart: Date, rangeEnd: Date): { start: Date, end: Date }[] {
  const months: { start: Date, end: Date }[] = [];
  let cursor = startOfMonth(rangeStart);
  const end = startOfMonth(rangeEnd);
  while (cursor < end) {
    const next = addMonths(cursor, 1);
    months.push({ start: cursor, end: next });
    cursor = next;
  }
  return months;
}

export function toUtcIso(date: Date): string {
  return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate())).toISOString();
}

export function toUtcDateTimeIso(date: Date): string {
  return date.toISOString();
}

export function toDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function snapToMinutes(date: Date, minutes: number): Date {
  const copy = new Date(date);
  const totalMinutes = copy.getHours() * 60 + copy.getMinutes();
  const snapped = Math.round(totalMinutes / minutes) * minutes;
  copy.setHours(Math.floor(snapped / 60), snapped % 60, 0, 0);
  return copy;
}

export function formatTimeRange(startUtc: string, endUtc: string): string {
  const formatter = new Intl.DateTimeFormat('nl-NL', {
    hour: '2-digit',
    minute: '2-digit',
  });
  return `${formatter.format(new Date(startUtc))} – ${formatter.format(new Date(endUtc))}`;
}

export function formatRelativePlanningDate(dateUtc: string): string {
  const date = new Date(dateUtc);
  const now = new Date();
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const target = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const diffDays = Math.round((target.getTime() - today.getTime()) / (24 * 60 * 60 * 1000));

  if (diffDays === 0) {
    return 'Vandaag';
  }
  if (diffDays === 1) {
    return 'Morgen';
  }

  return new Intl.DateTimeFormat('nl-NL', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
  }).format(date);
}

export function formatDayHeader(date: Date): string {
  return new Intl.DateTimeFormat('nl-NL', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  }).format(date);
}

export function formatCompactDayHeader(date: Date): { weekday: string, day: string } {
  return {
    weekday: new Intl.DateTimeFormat('nl-NL', { weekday: 'short' }).format(date),
    day: String(date.getDate()),
  };
}

export function isWeekend(date: Date): boolean {
  const day = date.getDay();
  return day === 0 || day === 6;
}

export function isSameCalendarDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate();
}

export function isToday(date: Date): boolean {
  return isSameCalendarDay(date, new Date());
}

/** Compact day header for employee agenda cards, e.g. "MA 14 JUL". */
export function formatAgendaDayHeader(date: Date): string {
  const weekday = new Intl.DateTimeFormat('nl-NL', { weekday: 'short' })
    .format(date)
    .replace('.', '')
    .toUpperCase();
  const day = String(date.getDate());
  const month = new Intl.DateTimeFormat('nl-NL', { month: 'short' })
    .format(date)
    .replace('.', '')
    .toUpperCase();
  return `${weekday} ${day} ${month}`;
}
