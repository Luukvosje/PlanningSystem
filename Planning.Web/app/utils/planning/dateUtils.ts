import type { Composer } from 'vue-i18n';
import type { PlanningStatus } from '~/types/planning';

type Translate = Composer['t'];

const INTL_LOCALES: Record<string, string> = {
  nl: 'nl-NL',
  en: 'en-US',
};

/** Map an app locale code (e.g. 'nl', 'en') to a full BCP-47 tag for Intl formatters. */
export function getIntlLocale(code: string): string {
  return INTL_LOCALES[code] ?? 'en-US';
}

export const STATUS_COLORS: Record<PlanningStatus, string> = {
  Planned: '#94A3B8',
  Confirmed: '#14B8A6',
  Completed: '#64748B',
  Cancelled: '#F43F5E',
};

/** Concept (Planned) blocks: gray with diagonal stripes. */
export const CONCEPT_BLOCK_STYLE = {
  backgroundColor: '#94A3B8',
  backgroundImage: 'repeating-linear-gradient(-45deg, transparent, transparent 4px, rgba(255,255,255,0.28) 4px, rgba(255,255,255,0.28) 8px)',
} as const;

/**
 * Unavailable time is slate everywhere in the app. Written as a raw colour rather than a theme
 * token because the stripes need one too, and a gradient cannot take a Tailwind class.
 */
const UNAVAILABLE_RGB = '148, 163, 184';

/** The diagonal stripes that mark unavailable time in the timeline. */
const UNAVAILABLE_HATCH_IMAGE =
  `repeating-linear-gradient(-45deg, transparent, transparent 4px, rgba(${UNAVAILABLE_RGB}, 0.35) 4px, rgba(${UNAVAILABLE_RGB}, 0.35) 8px)`;

/** Sits behind planning blocks in the timeline, so it stays nearly transparent. */
export const UNAVAILABLE_OVERLAY_STYLE = {
  backgroundColor: `rgba(${UNAVAILABLE_RGB}, 0.15)`,
  backgroundImage: UNAVAILABLE_HATCH_IMAGE,
} as const;

/** The edge of a blocked item, where a line does the work a fill would otherwise do. */
export const UNAVAILABLE_EDGE_STYLE = {
  borderColor: `rgba(${UNAVAILABLE_RGB}, 0.7)`,
} as const;

/** The bar that stands where a shift carries its colour. */
export const UNAVAILABLE_BAR_STYLE = {
  backgroundColor: `rgba(${UNAVAILABLE_RGB}, 0.6)`,
} as const;

export function getStatusLabel(status: PlanningStatus, t: Translate): string {
  const labels: Record<PlanningStatus, string> = {
    Planned: t('planning.status.planned'),
    Confirmed: t('planning.status.confirmed'),
    Completed: t('planning.status.completed'),
    Cancelled: t('planning.status.cancelled'),
  };
  return labels[status];
}

export function getBlockColor(color?: string | null, status?: PlanningStatus): string {
  if (color) {
    return color;
  }
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

export function toUtcDateTimeIso(date: Date): string {
  return date.toISOString();
}

export function toDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function formatTimeRange(startUtc: string, endUtc: string, locale: string): string {
  const formatter = new Intl.DateTimeFormat(locale, {
    hour: '2-digit',
    minute: '2-digit',
  });
  return `${formatter.format(new Date(startUtc))} – ${formatter.format(new Date(endUtc))}`;
}

export function formatRelativePlanningDate(dateUtc: string, locale: string, t: Translate): string {
  const date = new Date(dateUtc);
  const now = new Date();
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const target = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const diffDays = Math.round((target.getTime() - today.getTime()) / (24 * 60 * 60 * 1000));

  if (diffDays === 0) {
    return t('dateTimeRange.today');
  }
  if (diffDays === 1) {
    return t('dateTimeRange.tomorrow');
  }

  return new Intl.DateTimeFormat(locale, {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
  }).format(date);
}

export function formatDayHeader(date: Date, locale: string): string {
  return new Intl.DateTimeFormat(locale, {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  }).format(date);
}

export function formatCompactDayHeader(date: Date, locale: string): { weekday: string, day: string } {
  return {
    weekday: new Intl.DateTimeFormat(locale, { weekday: 'short' }).format(date),
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
export function formatAgendaDayHeader(date: Date, locale: string): string {
  const weekday = new Intl.DateTimeFormat(locale, { weekday: 'short' })
    .format(date)
    .replace('.', '')
    .toUpperCase();
  const day = String(date.getDate());
  const month = new Intl.DateTimeFormat(locale, { month: 'short' })
    .format(date)
    .replace('.', '')
    .toUpperCase();
  return `${weekday} ${day} ${month}`;
}
