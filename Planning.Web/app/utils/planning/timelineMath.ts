import type { TimelineZoom } from '~/types/planning';
import type { OpeningHoursEntry } from '~/utils/planning/planningSettings';
import {
  getOpeningHoursForDate,
  parseTimeToMinutes,
  sortImportantTimes,
} from '~/utils/planning/planningSettings';
import { addDays } from './dateUtils';

export const COMPACT_PADDING_MINUTES = 30;
export const DEFAULT_OFFICE_START = '07:00';
export const DEFAULT_OFFICE_END = '19:00';

export interface DayTimeWindow {
  start: Date
  end: Date
}

export interface RecordLike {
  startUtc: string
  endUtc: string
}

export interface CompactCurrentTimeIndicator {
  px: number
  clamped: 'none' | 'before' | 'after'
}

export interface ComputeDayWindowOptions {
  paddingMinutes?: number
  minWindowMinutes?: number
}

export interface ComputeWeekWindowsOptions extends ComputeDayWindowOptions {
  zoomMinutes?: number
}

export const ROW_LABEL_WIDTH = 200;
export const BLOCK_PADDING = 4;
export const LANE_HEIGHT = 52;
export const LANE_HEIGHT_COMPACT = 36;
export const LANE_HEIGHT_SPACIOUS = 96;
export const LANE_HEIGHT_DETAIL = 80;
export const BASE_ROW_HEIGHT = LANE_HEIGHT + BLOCK_PADDING * 2;
export const SNAP_MINUTES = 15;

const DETAIL_ZOOM_LEVELS: TimelineZoom[] = ['15m', '30m', '1h', '2h', '4h', 'day'];

export type PlanningLayoutMode = 'compact' | 'spacious' | 'default';

/** Returns the lane height for the given zoom level and optional layout mode. */
export function getLaneHeight(zoom: TimelineZoom, layout: PlanningLayoutMode = 'default'): number {
  if (layout === 'compact') return LANE_HEIGHT_COMPACT;
  if (layout === 'spacious') return LANE_HEIGHT_SPACIOUS;
  return DETAIL_ZOOM_LEVELS.includes(zoom) ? LANE_HEIGHT_DETAIL : LANE_HEIGHT;
}

/** Width of one zoom-interval column — fixed at every zoom level; finer zoom = wider day + horizontal scroll. */
export const SLOT_WIDTH = 40;

/** @deprecated Continuous slot scale removed; kept for preference migration compat. */
export const MIN_SLOT_SCALE = 1;
/** @deprecated Continuous slot scale removed; kept for preference migration compat. */
export const MAX_SLOT_SCALE = 1;

export function getEffectiveSlotWidth(slotScale = 1): number {
  return SLOT_WIDTH * slotScale;
}

const ZOOM_MINUTES: Record<TimelineZoom, number> = {
  '15m': 15,
  '30m': 30,
  '1h': 60,
  '2h': 120,
  '4h': 240,
  day: 24 * 60,
  week: 7 * 24 * 60,
  month: 30 * 24 * 60,
};

/** Full discrete zoom ladder from finest to coarsest. */
export const ZOOM_LADDER: readonly TimelineZoom[] = [
  '15m',
  '30m',
  '1h',
  '2h',
  '4h',
  'day',
  'week',
  'month',
] as const;

export const ZOOM_PRESETS: readonly { value: TimelineZoom, label: string, keypress: string }[] = [
  { value: '1h', label: 'Uur', keypress: 'h' },
  { value: '4h', label: 'Dagdeel', keypress: '4' },
  { value: 'day', label: 'Dag', keypress: 'd' },
  { value: 'week', label: 'Week', keypress: 'w' },
  { value: 'month', label: 'Maand', keypress: 'm' },
] as const;

const ZOOM_LABELS: Record<TimelineZoom, string> = {
  '15m': '15m',
  '30m': '30m',
  '1h': 'Uur',
  '2h': '2u',
  '4h': 'Dagdeel',
  day: 'Dag',
  week: 'Week',
  month: 'Maand',
};

export function getZoomLabel(zoom: TimelineZoom): string {
  return ZOOM_LABELS[zoom];
}

export function getZoomLadderIndex(zoom: TimelineZoom): number {
  return ZOOM_LADDER.indexOf(zoom);
}

export function canZoomIn(zoom: TimelineZoom): boolean {
  return getZoomLadderIndex(zoom) > 0;
}

export function canZoomOut(zoom: TimelineZoom): boolean {
  const index = getZoomLadderIndex(zoom);
  return index >= 0 && index < ZOOM_LADDER.length - 1;
}

export function stepZoomIn(zoom: TimelineZoom): TimelineZoom {
  const index = getZoomLadderIndex(zoom);
  if (index <= 0) {
    return zoom;
  }
  return ZOOM_LADDER[index - 1]!;
}

export function stepZoomOut(zoom: TimelineZoom): TimelineZoom {
  const index = getZoomLadderIndex(zoom);
  if (index < 0 || index >= ZOOM_LADDER.length - 1) {
    return zoom;
  }
  return ZOOM_LADDER[index + 1]!;
}

/** Fixed minimum day-widths (px) for the coarse calendar zoom levels. */
const CALENDAR_MIN_DAY_WIDTH: Partial<Record<TimelineZoom, number>> = {
  month: 36,   // ~250px per week (7 × 36 = 252)
  week: 96,    // ~96px per day
  day: 240,    // ~240px per day
};

export function getMinZoomDayWidth(zoom: TimelineZoom, slotScale = 1): number {
  const fixed = CALENDAR_MIN_DAY_WIDTH[zoom];
  if (fixed !== undefined) {
    return fixed;
  }
  const slotMinutes = ZOOM_MINUTES[zoom];
  const effectiveSlotWidth = getEffectiveSlotWidth(slotScale);
  return ((24 * 60) / slotMinutes) * effectiveSlotWidth;
}

export function getDayWidth(
  zoom: TimelineZoom,
  dayCount: number,
  containerWidth: number,
  slotScale = 1,
): number {
  const minZoomDayWidth = getMinZoomDayWidth(zoom, slotScale);
  if (containerWidth <= 0 || dayCount <= 0) {
    return minZoomDayWidth;
  }

  const fitDayWidth = (containerWidth - ROW_LABEL_WIDTH) / dayCount;
  return Math.max(fitDayWidth, minZoomDayWidth);
}

export function getZoomMinutes(zoom: TimelineZoom): number {
  return ZOOM_MINUTES[zoom];
}

/** Second header row / column label granularity. */
export type TimelineHeaderColumnMode = 'day' | 'week' | 'month';

/** Optional top band above column labels. */
export type TimelineHeaderPeriodMode = 'week' | 'month' | null;

/** Where strong vertical borders are drawn in header + row grid. */
export type TimelinePrimaryBorder = 'day' | 'week' | 'month';

export function getHeaderColumnMode(zoom: TimelineZoom): TimelineHeaderColumnMode {
  if (zoom === 'month') {
    // Sub-header shows weeks under the month band
    return 'week';
  }
  // week, day and finer: sub-header shows individual days
  return 'day';
}

export function getHeaderPeriodMode(zoom: TimelineZoom): TimelineHeaderPeriodMode {
  if (zoom === 'month') {
    // Top band: month groups
    return 'month';
  }
  if (zoom === 'week') {
    // Top band: week groups
    return 'week';
  }
  if (zoom === 'day') {
    // Top band: month groups above day columns
    return 'month';
  }
  // Finer zooms: week band above day columns
  return 'week';
}

export function getPrimaryBorderMode(zoom: TimelineZoom): TimelinePrimaryBorder {
  if (zoom === 'month') {
    return 'month';
  }
  if (zoom === 'week') {
    return 'week';
  }
  if (zoom === 'day') {
    return 'month';
  }
  return 'day';
}

/**
 * Shift a date by one navigation period for the given zoom.
 * Month → calendar month, week → 7 days, day and finer → 1 day.
 */
export function shiftDateByZoomPeriod(
  date: Date,
  zoom: TimelineZoom,
  direction: -1 | 1,
): Date {
  const base = new Date(date);
  base.setHours(0, 0, 0, 0);

  if (zoom === 'month') {
    return new Date(base.getFullYear(), base.getMonth() + direction, 1);
  }

  if (zoom === 'week') {
    return addDays(base, 7 * direction);
  }

  return addDays(base, direction);
}

export function getDayCount(start: Date, end: Date): number {
  return Math.ceil((end.getTime() - start.getTime()) / (24 * 60 * 60 * 1000));
}

function isWeekendDate(date: Date): boolean {
  const day = date.getDay();
  return day === 0 || day === 6;
}

/** Weekdays only when showWeekends is false. */
export function countVisibleDaysInRange(start: Date, end: Date, showWeekends: boolean): number {
  const total = getDayCount(start, end);
  if (showWeekends) {
    return total;
  }

  let count = 0;
  for (let i = 0; i < total; i++) {
    const date = addDays(start, i);
    if (!isWeekendDate(date)) {
      count++;
    }
  }
  return count;
}

/** Visible day columns strictly before date (midnight local). */
export function countVisibleDaysBefore(date: Date, rangeStart: Date, showWeekends: boolean): number {
  const dayStart = new Date(date);
  dayStart.setHours(0, 0, 0, 0);
  let count = 0;

  for (let d = new Date(rangeStart); d < dayStart; d = addDays(d, 1)) {
    if (showWeekends || !isWeekendDate(d)) {
      count++;
    }
  }
  return count;
}

export function timelineLeftForDate(
  date: Date,
  rangeStart: Date,
  dayWidth: number,
  showWeekends: boolean,
): number {
  return countVisibleDaysBefore(date, rangeStart, showWeekends) * dayWidth;
}

export function timeToPx(
  utcIso: string,
  rangeStart: Date,
  dayWidth: number,
): number {
  const diffMs = new Date(utcIso).getTime() - rangeStart.getTime();
  return (diffMs / (24 * 60 * 60 * 1000)) * dayWidth;
}

export function snapPx(px: number, dayWidth: number, snapMinutes = SNAP_MINUTES): number {
  if (snapMinutes <= 0) {
    return px;
  }
  const snapWidthPx = (snapMinutes / (24 * 60)) * dayWidth;
  return Math.round(px / snapWidthPx) * snapWidthPx;
}

export function pxToUtcIso(
  px: number,
  rangeStart: Date,
  dayWidth: number,
  snapMinutes = 0,
): string {
  const finalPx = snapMinutes > 0 ? snapPx(px, dayWidth, snapMinutes) : px;
  const msPerPx = (24 * 60 * 60 * 1000) / dayWidth;
  return new Date(rangeStart.getTime() + finalPx * msPerPx).toISOString();
}

export function pxToTime(
  px: number,
  rangeStart: Date,
  dayWidth: number,
): Date {
  const dayIndex = Math.floor(px / dayWidth);
  const dayFraction = (px % dayWidth) / dayWidth;
  const result = addDays(rangeStart, dayIndex);
  const totalMinutes = Math.round(dayFraction * 24 * 60 / SNAP_MINUTES) * SNAP_MINUTES;
  result.setHours(Math.floor(totalMinutes / 60), totalMinutes % 60, 0, 0);
  return result;
}

export function getTimelineWidth(dayCount: number, dayWidth: number): number {
  return dayCount * dayWidth;
}

/** Below this slot width (px) the time label is hidden and shown in a tooltip instead. */
export const MIN_LABEL_SLOT_WIDTH = 32;

export interface TimeSlotMarker {
  leftPx: number
  width: number
  label: string
  showLabel: boolean
}

export function getTimeSlotMarkers(zoom: TimelineZoom, dayWidth: number): TimeSlotMarker[] {
  const slotMinutes = ZOOM_MINUTES[zoom];
  if (slotMinutes >= 24 * 60) {
    return [];
  }

  const slotsPerDay = (24 * 60) / slotMinutes;
  const slotWidth = dayWidth / slotsPerDay;
  const showLabel = slotWidth >= MIN_LABEL_SLOT_WIDTH;

  const timeOptions: Intl.DateTimeFormatOptions = slotMinutes < 60 ?
    { hour: '2-digit', minute: '2-digit' } :
    { hour: '2-digit' };

  const formatter = new Intl.DateTimeFormat('nl-NL', timeOptions);
  const markers: TimeSlotMarker[] = [];

  for (let i = 0; i < slotsPerDay; i++) {
    const totalMinutes = i * slotMinutes;
    const date = new Date(2000, 0, 1, Math.floor(totalMinutes / 60), totalMinutes % 60);
    markers.push({
      leftPx: i * slotWidth,
      width: slotWidth,
      label: formatter.format(date),
      showLabel,
    });
  }

  return markers;
}

export function showsTimeSlots(zoom: TimelineZoom): boolean {
  return ZOOM_MINUTES[zoom] < 24 * 60;
}

const ZOOMED_OUT_LEVELS: TimelineZoom[] = ['2h', '4h', 'day', 'week', 'month'];

export function isZoomedOutView(zoom: TimelineZoom, slotWidth: number): boolean {
  return ZOOMED_OUT_LEVELS.includes(zoom) || slotWidth < MIN_LABEL_SLOT_WIDTH;
}

export interface TimelineGridLine {
  leftPx: number
  width: number
  label?: string
  showLabel: boolean
  isImportant: boolean
}

export function getImportantGridLines(
  dayWidth: number,
  importantTimes: string[],
): TimelineGridLine[] {
  const formatter = new Intl.DateTimeFormat('nl-NL', { hour: '2-digit', minute: '2-digit' });

  return importantTimes.map((time) => {
    const minutes = parseTimeToMinutesFromString(time);
    const date = new Date(2000, 0, 1, Math.floor(minutes / 60), minutes % 60);
    return {
      leftPx: minutesToDayPx(minutes, dayWidth),
      width: 0,
      label: formatter.format(date),
      showLabel: true,
      isImportant: true,
    };
  });
}

function minutesToDayPx(minutes: number, dayWidth: number): number {
  return (minutes / (24 * 60)) * dayWidth;
}

/**
 * Coarse vertical grid lines for calendar zoom levels (day / week / month).
 * - day zoom   → one line every 4 hours within the day
 * - week zoom  → one line per day (already covered by day-border, but added here
 *                so they appear inside rows via CurrentTimeIndicator)
 * - month zoom → one line per day (week separators are handled by dayBorder)
 *
 * Each line's `leftPx` is relative to the start of one day column.
 */
export function getCoarseGridLines(
  zoom: TimelineZoom,
  dayWidth: number,
): TimelineGridLine[] {
  if (zoom === 'day') {
    // Lines every 4 hours; skip index 0 (leftPx = 0 overlaps the day's left border)
    const intervalMinutes = 4 * 60;
    const count = (24 * 60) / intervalMinutes; // 6
    const formatter = new Intl.DateTimeFormat('nl-NL', { hour: '2-digit', minute: '2-digit' });
    return Array.from({ length: count }, (_, i) => {
      const totalMinutes = i * intervalMinutes;
      const date = new Date(2000, 0, 1, Math.floor(totalMinutes / 60), totalMinutes % 60);
      return {
        leftPx: (totalMinutes / (24 * 60)) * dayWidth,
        width: (intervalMinutes / (24 * 60)) * dayWidth,
        label: formatter.format(date),
        showLabel: dayWidth >= 60,
        isImportant: false,
      };
    }).filter((line) => line.leftPx > 0);
  }

  if (zoom === 'week') {
    // Lines every 12 hours (midday divider) — gives a subtle half-day split within each day column
    return [
      {
        leftPx: dayWidth / 2,
        width: dayWidth / 2,
        showLabel: false,
        isImportant: false,
      },
    ];
  }

  if (zoom === 'month') {
    // No intra-day lines needed at month zoom — day borders provide enough structure
    return [];
  }

  return [];
}

export function getTimelineGridLines(
  zoom: TimelineZoom,
  dayWidth: number,
  importantTimes: string[],
): TimelineGridLine[] {
  const slotMinutes = ZOOM_MINUTES[zoom];
  if (slotMinutes >= 24 * 60) {
    return [];
  }

  const slotsPerDay = (24 * 60) / slotMinutes;
  const slotWidth = dayWidth / slotsPerDay;
  const zoomedOut = isZoomedOutView(zoom, slotWidth);
  const showLabels = slotWidth >= MIN_LABEL_SLOT_WIDTH;

  if (zoomedOut) {
    return getImportantGridLines(dayWidth, importantTimes).map((line) => ({
      ...line,
      showLabel: showLabels,
    }));
  }

  const timeOptions: Intl.DateTimeFormatOptions = slotMinutes < 60 ?
    { hour: '2-digit', minute: '2-digit' } :
    { hour: '2-digit' };
  const formatter = new Intl.DateTimeFormat('nl-NL', timeOptions);

  const lines: TimelineGridLine[] = [];

  for (let i = 0; i < slotsPerDay; i++) {
    const totalMinutes = i * slotMinutes;

    const date = new Date(2000, 0, 1, Math.floor(totalMinutes / 60), totalMinutes % 60);
    lines.push({
      leftPx: i * slotWidth,
      width: slotWidth,
      label: formatter.format(date),
      showLabel: showLabels,
      isImportant: false,
    });
  }

  return lines;
}

function parseTimeToMinutesFromString(time: string): number {
  const [hours, minutes] = time.split(':').map(Number);
  return hours! * 60 + minutes!;
}

export function getCurrentTimePx(rangeStart: Date, rangeEnd: Date, dayWidth: number): number | null {
  const now = new Date();
  if (now < rangeStart || now >= rangeEnd) {
    return null;
  }
  return timeToPx(now.toISOString(), rangeStart, dayWidth);
}

export function pxFromPointerEvent(event: PointerEvent, timelineRow: HTMLElement): number {
  const rect = timelineRow.getBoundingClientRect();
  return event.clientX - rect.left;
}

export function toDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function createLocalDateTime(date: Date, time: string): Date {
  const [hours, minutes] = time.split(':').map(Number);
  const result = new Date(date);
  result.setHours(hours ?? 0, minutes ?? 0, 0, 0);
  return result;
}

function dayBounds(date: Date): { start: Date, end: Date } {
  const start = new Date(date);
  start.setHours(0, 0, 0, 0);
  return { start, end: addDays(start, 1) };
}

export function getOfficeHoursWindow(
  date: Date,
  openingHours: OpeningHoursEntry[],
  importantWorkTimes: string[] = [],
): DayTimeWindow {
  const hours = getOpeningHoursForDate(date, openingHours);
  if (hours) {
    return {
      start: createLocalDateTime(date, hours.openTime),
      end: createLocalDateTime(date, hours.closeTime),
    };
  }

  if (importantWorkTimes.length > 0) {
    const sorted = sortImportantTimes(importantWorkTimes);
    return {
      start: createLocalDateTime(date, sorted[0]!),
      end: createLocalDateTime(date, sorted[sorted.length - 1]!),
    };
  }

  return {
    start: createLocalDateTime(date, DEFAULT_OFFICE_START),
    end: createLocalDateTime(date, DEFAULT_OFFICE_END),
  };
}

function recordsForDay(records: RecordLike[], date: Date): RecordLike[] {
  const { start: dayStart, end: dayEnd } = dayBounds(date);
  return records.filter((record) => {
    const start = new Date(record.startUtc);
    const end = new Date(record.endUtc);
    return start < dayEnd && end > dayStart;
  });
}

export function computeDayWindow(
  records: RecordLike[],
  openingHours: OpeningHoursEntry[],
  date: Date,
  importantWorkTimes: string[] = [],
  options: ComputeDayWindowOptions = {},
): DayTimeWindow {
  const paddingMs = (options.paddingMinutes ?? COMPACT_PADDING_MINUTES) * 60 * 1000;
  const minWindowMs = (options.minWindowMinutes ?? 60) * 60 * 1000;

  const office = getOfficeHoursWindow(date, openingHours, importantWorkTimes);
  let windowStart = office.start.getTime();
  let windowEnd = office.end.getTime();

  for (const record of records) {
    windowStart = Math.min(windowStart, new Date(record.startUtc).getTime() - paddingMs);
    windowEnd = Math.max(windowEnd, new Date(record.endUtc).getTime() + paddingMs);
  }

  if (windowEnd - windowStart < minWindowMs) {
    const center = (windowStart + windowEnd) / 2;
    windowStart = center - minWindowMs / 2;
    windowEnd = center + minWindowMs / 2;
  }

  const { start: dayStart, end: dayEnd } = dayBounds(date);
  windowStart = Math.max(windowStart, dayStart.getTime());
  windowEnd = Math.min(windowEnd, dayEnd.getTime());

  if (windowEnd <= windowStart) {
    windowEnd = Math.min(windowStart + minWindowMs, dayEnd.getTime());
  }

  return {
    start: new Date(windowStart),
    end: new Date(windowEnd),
  };
}

export function computeWeekWindows(
  days: Date[],
  allRecords: RecordLike[],
  openingHours: OpeningHoursEntry[],
  importantWorkTimes: string[] = [],
  options: ComputeWeekWindowsOptions = {},
): Map<string, DayTimeWindow> {
  const minWindowMinutes = options.minWindowMinutes ?? (options.zoomMinutes ? options.zoomMinutes * 2 : 60);
  const windows = new Map<string, DayTimeWindow>();

  for (const day of days) {
    const dayRecords = recordsForDay(allRecords, day);
    windows.set(
      toDateKey(day),
      computeDayWindow(dayRecords, openingHours, day, importantWorkTimes, {
        ...options,
        minWindowMinutes,
      }),
    );
  }

  return windows;
}

export function timeToPxCompact(
  utcIso: string,
  dayWindow: DayTimeWindow,
  dayWidth: number,
): number {
  const spanMs = dayWindow.end.getTime() - dayWindow.start.getTime();
  if (spanMs <= 0) {
    return 0;
  }
  const t = new Date(utcIso).getTime();
  return ((t - dayWindow.start.getTime()) / spanMs) * dayWidth;
}

function snapPxCompact(
  px: number,
  dayWidth: number,
  dayWindow: DayTimeWindow,
  snapMinutes: number,
): number {
  if (snapMinutes <= 0) {
    return px;
  }
  const spanMs = dayWindow.end.getTime() - dayWindow.start.getTime();
  if (spanMs <= 0 || dayWidth <= 0) {
    return px;
  }
  const snapWidthPx = ((snapMinutes * 60 * 1000) / spanMs) * dayWidth;
  return Math.round(px / snapWidthPx) * snapWidthPx;
}

export function pxToUtcIsoCompact(
  pxInDay: number,
  dayWindow: DayTimeWindow,
  dayWidth: number,
  snapMinutes = 0,
): string {
  const spanMs = dayWindow.end.getTime() - dayWindow.start.getTime();
  if (spanMs <= 0 || dayWidth <= 0) {
    return dayWindow.start.toISOString();
  }

  const finalPx = snapMinutes > 0 ?
    snapPxCompact(pxInDay, dayWidth, dayWindow, snapMinutes) :
    pxInDay;
  const msPerPx = spanMs / dayWidth;
  return new Date(dayWindow.start.getTime() + finalPx * msPerPx).toISOString();
}

export function getCompactImportantGridLines(
  dayWidth: number,
  window: DayTimeWindow,
  importantTimes: string[],
): TimelineGridLine[] {
  const formatter = new Intl.DateTimeFormat('nl-NL', { hour: '2-digit', minute: '2-digit' });
  const { start: dayStart } = dayBounds(window.start);
  const windowStartMs = window.start.getTime();
  const windowEndMs = window.end.getTime();

  return importantTimes.flatMap((time) => {
    const minutes = parseTimeToMinutes(time);
    const date = new Date(dayStart.getTime() + minutes * 60 * 1000);
    if (date.getTime() < windowStartMs || date.getTime() >= windowEndMs) {
      return [];
    }

    return [{
      leftPx: timeToPxCompact(date.toISOString(), window, dayWidth),
      width: 0,
      label: formatter.format(date),
      showLabel: true,
      isImportant: true,
    }];
  });
}

export function getCompactTimelineGridLines(
  zoom: TimelineZoom,
  dayWidth: number,
  window: DayTimeWindow,
  importantTimes: string[],
): TimelineGridLine[] {
  const slotMinutes = ZOOM_MINUTES[zoom];
  if (slotMinutes >= 24 * 60) {
    return [];
  }

  const windowMs = window.end.getTime() - window.start.getTime();
  const windowMinutes = windowMs / 60_000;
  const slotsInWindow = Math.max(Math.ceil(windowMinutes / slotMinutes), 1);
  const slotWidth = dayWidth / slotsInWindow;
  const zoomedOut = isZoomedOutView(zoom, slotWidth);
  const showLabels = slotWidth >= MIN_LABEL_SLOT_WIDTH;

  if (zoomedOut) {
    return getCompactImportantGridLines(dayWidth, window, importantTimes).map((line) => ({
      ...line,
      showLabel: showLabels,
    }));
  }

  const { start: dayStart } = dayBounds(window.start);
  const windowStartMinutes = (window.start.getTime() - dayStart.getTime()) / 60_000;
  const windowEndMinutes = (window.end.getTime() - dayStart.getTime()) / 60_000;
  const firstSlotMinutes = Math.ceil(windowStartMinutes / slotMinutes) * slotMinutes;

  const timeOptions: Intl.DateTimeFormatOptions = slotMinutes < 60 ?
    { hour: '2-digit', minute: '2-digit' } :
    { hour: '2-digit' };
  const formatter = new Intl.DateTimeFormat('nl-NL', timeOptions);

  const lines: TimelineGridLine[] = [];

  for (let totalMinutes = firstSlotMinutes; totalMinutes < windowEndMinutes; totalMinutes += slotMinutes) {
    const date = new Date(dayStart.getTime() + totalMinutes * 60 * 1000);
    lines.push({
      leftPx: timeToPxCompact(date.toISOString(), window, dayWidth),
      width: slotWidth,
      label: formatter.format(date),
      showLabel: showLabels,
      isImportant: false,
    });
  }

  return lines;
}

export function getCompactCurrentTimePx(
  now: Date,
  dayWindow: DayTimeWindow,
  dayWidth: number,
  columnOffsetPx: number,
): CompactCurrentTimeIndicator | null {
  const { start: dayStart, end: dayEnd } = dayBounds(dayWindow.start);
  const nowMs = now.getTime();

  if (nowMs < dayStart.getTime() || nowMs >= dayEnd.getTime()) {
    return null;
  }

  const startMs = dayWindow.start.getTime();
  const endMs = dayWindow.end.getTime();

  if (nowMs < startMs) {
    return { px: columnOffsetPx, clamped: 'before' };
  }

  if (nowMs >= endMs) {
    return { px: columnOffsetPx + dayWidth, clamped: 'after' };
  }

  return {
    px: columnOffsetPx + timeToPxCompact(now.toISOString(), dayWindow, dayWidth),
    clamped: 'none',
  };
}
