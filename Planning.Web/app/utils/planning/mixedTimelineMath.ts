import type { PlanningRecord } from '~/types/planning';

export const MIXED_START_HOUR = 6;
export const MIXED_END_HOUR = 23;
export const MIXED_HOUR_WIDTH = 80;
export const MIXED_TIME_AXIS_HEIGHT = 40;

export function getMixedTimelineWidth(): number {
  return (MIXED_END_HOUR - MIXED_START_HOUR) * MIXED_HOUR_WIDTH;
}

function minutesFromMidnight(date: Date): number {
  return date.getHours() * 60 + date.getMinutes();
}

export function timeToMixedPx(utcIso: string): number {
  const time = new Date(utcIso);
  const minutes = minutesFromMidnight(time) - MIXED_START_HOUR * 60;
  return (minutes / 60) * MIXED_HOUR_WIDTH;
}

export function getMixedBlockLayout(
  record: PlanningRecord,
  timelineWidth: number,
): { leftPx: number, widthPx: number } {
  const leftPx = Math.max(timeToMixedPx(record.startUtc), 0);
  const rightPx = Math.min(timeToMixedPx(record.endUtc), timelineWidth);
  return { leftPx, widthPx: Math.max(rightPx - leftPx, 4) };
}

export function filterRecordsForDay(records: PlanningRecord[], day: Date): PlanningRecord[] {
  const dayStart = new Date(day);
  dayStart.setHours(0, 0, 0, 0);
  const dayEnd = new Date(dayStart);
  dayEnd.setDate(dayEnd.getDate() + 1);

  return records.filter((record) => {
    const start = new Date(record.startUtc);
    const end = new Date(record.endUtc);
    if (start >= dayEnd || end <= dayStart) {
return false;
}

    const visibleStart = Math.max(timeToMixedPx(record.startUtc), 0);
    const visibleEnd = Math.min(timeToMixedPx(record.endUtc), getMixedTimelineWidth());
    return visibleEnd > visibleStart;
  });
}

export function getMixedCurrentTimePx(selectedDay: Date): number | null {
  const now = new Date();
  const dayStart = new Date(selectedDay);
  dayStart.setHours(0, 0, 0, 0);
  const dayEnd = new Date(dayStart);
  dayEnd.setDate(dayEnd.getDate() + 1);

  if (now < dayStart || now >= dayEnd) {
return null;
}

  const px = timeToMixedPx(now.toISOString());
  if (px < 0 || px > getMixedTimelineWidth()) {
return null;
}
  return px;
}

export function isOpenShift(record: PlanningRecord): boolean {
  const id = record.assignedUserId;
  return !id || id === 'undefined' || id === 'null' || id === '0';
}

export function getHourMarkers(locale: string): { hour: number, leftPx: number, label: string }[] {
  const formatter = new Intl.DateTimeFormat(locale, { hour: '2-digit', minute: '2-digit' });
  const markers: { hour: number, leftPx: number, label: string }[] = [];

  for (let hour = MIXED_START_HOUR; hour <= MIXED_END_HOUR; hour++) {
    const date = new Date();
    date.setHours(hour, 0, 0, 0);
    markers.push({
      hour,
      leftPx: (hour - MIXED_START_HOUR) * MIXED_HOUR_WIDTH,
      label: formatter.format(date),
    });
  }

  return markers;
}
