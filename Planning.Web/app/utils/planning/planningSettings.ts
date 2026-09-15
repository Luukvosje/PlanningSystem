import type { DayTimeWindow } from '~/utils/planning/timelineMath';
import type { OpeningHoursEntry } from '~/utils/planning/timeOfDay';
import { timeToPxCompact } from '~/utils/planning/timelineMath';
import {
  getOpeningHoursForDate,
  minutesToDayPx,
  parseTimeToMinutes,
  sortImportantTimes,
} from '~/utils/planning/timeOfDay';

// Re-exported so the many existing importers of this module keep working; the definitions live
// in timeOfDay because timelineMath needs them too and these two modules import each other.
export type { OpeningHoursEntry } from '~/utils/planning/timeOfDay';
export {
  getOpeningHoursForDate,
  minutesToDayPx,
  normalizeTimeValue,
  parseTimeToMinutes,
  sortImportantTimes,
  weekdayFromDate,
} from '~/utils/planning/timeOfDay';

/** Plain time-of-day fallback, used by consumers that only care about grid/snap positions. */
export const DEFAULT_IMPORTANT_WORK_TIMES = ['06:00', '09:00', '13:00', '17:00', '21:00'];

export interface ImportantWorkTimeRow {
  label: string
  startTime: string
}

/** Starting content for a new organization's important-times editor, before it has saved any. */
export function getDefaultImportantWorkTimes(): ImportantWorkTimeRow[] {
  return DEFAULT_IMPORTANT_WORK_TIMES.map((time) => ({ label: '', startTime: time }));
}

/** Projects important-time rows down to the plain time-of-day strings the grid/snap math uses. */
export function toTimeStrings(entries: Pick<ImportantWorkTimeRow, 'startTime'>[]): string[] {
  return entries.map((entry) => entry.startTime);
}

export function getOutsideOpeningOverlays(
  date: Date,
  dayWidth: number,
  openingHours: OpeningHoursEntry[],
): { left: number, width: number }[] {
  const hours = getOpeningHoursForDate(date, openingHours);
  if (!hours) {
    return [];
  }

  const openPx = minutesToDayPx(parseTimeToMinutes(hours.openTime), dayWidth);
  const closePx = minutesToDayPx(parseTimeToMinutes(hours.closeTime), dayWidth);
  const overlays: { left: number, width: number }[] = [];

  if (openPx > 0) {
    overlays.push({ left: 0, width: openPx });
  }

  if (closePx < dayWidth) {
    overlays.push({ left: closePx, width: dayWidth - closePx });
  }

  return overlays;
}

function timeToCompactDayPx(date: Date, dayWidth: number, window: DayTimeWindow): number {
  return timeToPxCompact(date.toISOString(), window, dayWidth);
}

export function getOutsideOpeningOverlaysCompact(
  date: Date,
  dayWidth: number,
  openingHours: OpeningHoursEntry[],
  window: DayTimeWindow,
): { left: number, width: number }[] {
  const hours = getOpeningHoursForDate(date, openingHours);
  if (!hours) {
    return [];
  }

  const dayStart = new Date(date);
  dayStart.setHours(0, 0, 0, 0);
  const openDate = new Date(dayStart.getTime() + parseTimeToMinutes(hours.openTime) * 60_000);
  const closeDate = new Date(dayStart.getTime() + parseTimeToMinutes(hours.closeTime) * 60_000);

  const openPx = timeToCompactDayPx(openDate, dayWidth, window);
  const closePx = timeToCompactDayPx(closeDate, dayWidth, window);
  const overlays: { left: number, width: number }[] = [];

  if (openPx > 0) {
    overlays.push({ left: 0, width: openPx });
  }

  if (closePx < dayWidth) {
    overlays.push({ left: closePx, width: dayWidth - closePx });
  }

  return overlays;
}

export function isImportantTimeSlot(
  slotMinutes: number,
  importantTimes: string[],
): boolean {
  return importantTimes.some((time) => parseTimeToMinutes(time) === slotMinutes);
}

export function getISOWeekNumber(date: Date): number {
  const copy = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
  copy.setUTCDate(copy.getUTCDate() + 4 - (copy.getUTCDay() || 7));
  const yearStart = new Date(Date.UTC(copy.getUTCFullYear(), 0, 1));
  return Math.ceil((((copy.getTime() - yearStart.getTime()) / 86_400_000) + 1) / 7);
}

export function formatWeekDateRange(start: Date, end: Date, locale: string): string {
  if (start.getMonth() === end.getMonth() && start.getFullYear() === end.getFullYear()) {
    const dayFmt = new Intl.DateTimeFormat(locale, { day: 'numeric' });
    const monthFmt = new Intl.DateTimeFormat(locale, { month: 'short' });
    return `${dayFmt.format(start)} – ${dayFmt.format(end)} ${monthFmt.format(end)}`;
  }

  const formatter = new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short' });
  return `${formatter.format(start)} – ${formatter.format(end)}`;
}

export function uniqueImportantTimes(times: string[]): string[] {
  const seen = new Set<string>();
  return sortImportantTimes(times).filter((time) => {
    if (seen.has(time)) {
      return false;
    }
    seen.add(time);
    return true;
  });
}
