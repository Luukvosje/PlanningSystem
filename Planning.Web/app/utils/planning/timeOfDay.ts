import type { Weekday } from '~/types/availability';

/**
 * Primitives for working with a wall-clock time ("09:00") and the weekday of a date.
 *
 * These used to exist in three copies with subtly different behaviour: `parseTimeToMinutes` in
 * planningSettings normalized "9:00" first, `parseTimeToMinutesFromString` in timelineMath did
 * not, and `parseTimeParts` in availabilityMath returned parts instead of minutes. The weekday
 * lookup table was written out twice.
 *
 * They live in their own module rather than in planningSettings or timelineMath because those
 * two import from each other; a shared leaf module keeps that cycle from spreading.
 */

export interface OpeningHoursEntry {
  day: Weekday
  openTime: string
  closeTime: string
}

const JS_DAY_TO_WEEKDAY: Record<number, Weekday> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday',
};

export const MINUTES_PER_DAY = 24 * 60;

/** Pads "9:5" to "09:05" so string comparisons and sorting behave. */
export function normalizeTimeValue(value: string): string {
  const [hours, minutes] = value.split(':');
  return `${hours!.padStart(2, '0')}:${minutes!.padStart(2, '0')}`;
}

export function parseTimeParts(time: string): { hours: number, minutes: number } {
  const [hours, minutes] = normalizeTimeValue(time).split(':').map(Number);
  return { hours: hours ?? 0, minutes: minutes ?? 0 };
}

export function parseTimeToMinutes(time: string): number {
  const { hours, minutes } = parseTimeParts(time);
  return hours * 60 + minutes;
}

/** Fraction of a day column, in pixels. */
export function minutesToDayPx(minutes: number, dayWidth: number): number {
  return (minutes / MINUTES_PER_DAY) * dayWidth;
}

export function weekdayFromDate(date: Date): Weekday {
  return JS_DAY_TO_WEEKDAY[date.getDay()]!;
}

export function getOpeningHoursForDate(
  date: Date,
  openingHours: OpeningHoursEntry[],
): OpeningHoursEntry | null {
  const weekday = weekdayFromDate(date);
  return openingHours.find((entry) => entry.day === weekday) ?? null;
}

/** A local Date on `date`'s calendar day at the given wall-clock time. */
export function atTimeOnDate(date: Date, time: string): Date {
  const { hours, minutes } = parseTimeParts(time);
  const result = new Date(date);
  result.setHours(hours, minutes, 0, 0);
  return result;
}

/** A local Date built from a "YYYY-MM-DD" key and a wall-clock time. */
export function atTimeOnDateKey(dateKey: string, time: string): Date {
  const { hours, minutes } = parseTimeParts(time);
  const [year, month, day] = dateKey.split('-').map(Number);
  return new Date(year!, month! - 1, day!, hours, minutes, 0, 0);
}

export function sortImportantTimes(times: string[]): string[] {
  return [...times]
    .map(normalizeTimeValue)
    .sort((left, right) => parseTimeToMinutes(left) - parseTimeToMinutes(right));
}
