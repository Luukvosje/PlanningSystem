import { describe, expect, it } from 'vitest';
import {
  SNAP_MINUTES,
  computeDayWindow,
  computeWeekWindows,
  getDayWidth,
  getTimelineWidth,
  pxToUtcIso,
  pxToUtcIsoCompact,
  snapPx,
  timeToPx,
  timeToPxCompact,
} from '~/utils/planning/timelineMath';
import type { OpeningHoursEntry } from '~/utils/planning/planningSettings';

const DAY_WIDTH = 960;
const RANGE_START = new Date('2026-08-17T00:00:00Z');

describe('timeToPx / pxToUtcIso', () => {
  it('places the range start at zero', () => {
    expect(timeToPx(RANGE_START.toISOString(), RANGE_START, DAY_WIDTH)).toBe(0);
  });

  it('places one full day exactly one day-width along', () => {
    expect(timeToPx('2026-08-18T00:00:00Z', RANGE_START, DAY_WIDTH)).toBe(DAY_WIDTH);
  });

  it('scales linearly within a day', () => {
    expect(timeToPx('2026-08-17T12:00:00Z', RANGE_START, DAY_WIDTH)).toBe(DAY_WIDTH / 2);
    expect(timeToPx('2026-08-17T06:00:00Z', RANGE_START, DAY_WIDTH)).toBe(DAY_WIDTH / 4);
  });

  it('handles times before the range start', () => {
    expect(timeToPx('2026-08-16T12:00:00Z', RANGE_START, DAY_WIDTH)).toBe(-DAY_WIDTH / 2);
  });

  it('round-trips a timestamp back to itself', () => {
    for (const iso of [
      '2026-08-17T00:00:00.000Z',
      '2026-08-17T09:30:00.000Z',
      '2026-08-19T23:45:00.000Z',
    ]) {
      const px = timeToPx(iso, RANGE_START, DAY_WIDTH);
      expect(pxToUtcIso(px, RANGE_START, DAY_WIDTH)).toBe(iso);
    }
  });
});

describe('snapPx', () => {
  it('snaps to the nearest grid step', () => {
    const stepPx = (SNAP_MINUTES / (24 * 60)) * DAY_WIDTH;

    expect(snapPx(stepPx * 2, DAY_WIDTH)).toBeCloseTo(stepPx * 2);
    expect(snapPx(stepPx * 2.4, DAY_WIDTH)).toBeCloseTo(stepPx * 2);
    expect(snapPx(stepPx * 2.6, DAY_WIDTH)).toBeCloseTo(stepPx * 3);
  });

  it('snaps relative to the range start, not to the epoch', () => {
    // A range that does not begin on a snap boundary must still snap to offsets from its own
    // start; this is what the inline weekend-hiding path in useTimeline got wrong.
    const offsetStart = new Date('2026-08-17T00:07:00Z');
    const snappedIso = pxToUtcIso(snapPx(0, DAY_WIDTH), offsetStart, DAY_WIDTH);

    expect(snappedIso).toBe(offsetStart.toISOString());
  });

  it('is a no-op when snapping is disabled', () => {
    expect(snapPx(123.456, DAY_WIDTH, 0)).toBe(123.456);
  });

  it('produces whole snap intervals after a round-trip', () => {
    const px = timeToPx('2026-08-17T09:07:00Z', RANGE_START, DAY_WIDTH);
    const iso = pxToUtcIso(px, RANGE_START, DAY_WIDTH, SNAP_MINUTES);

    expect(new Date(iso).getUTCMinutes() % SNAP_MINUTES).toBe(0);
  });
});

describe('getDayWidth', () => {
  it('never returns less than the zoom minimum', () => {
    const narrow = getDayWidth('15m', 7, 300);
    const wide = getDayWidth('15m', 7, 100000);

    expect(narrow).toBeGreaterThan(0);
    expect(wide).toBeGreaterThan(narrow);
  });

  it('falls back to the zoom minimum for an unmeasured container', () => {
    expect(getDayWidth('1h', 7, 0)).toBe(getDayWidth('1h', 0, 0));
  });

  it('multiplies out to the timeline width', () => {
    expect(getTimelineWidth(7, 100)).toBe(700);
  });
});

describe('compact day window', () => {
  const openingHours: OpeningHoursEntry[] = [
    { day: 'Monday', openTime: '09:00', closeTime: '17:00' },
  ];
  const monday = new Date(2026, 7, 17); // 17 Aug 2026 is a Monday, local time

  it('starts from the opening hours when there are no records', () => {
    const window = computeDayWindow([], openingHours, monday);

    expect(window.end.getTime()).toBeGreaterThan(window.start.getTime());
    expect(window.start.getHours()).toBeLessThanOrEqual(9);
    expect(window.end.getHours()).toBeGreaterThanOrEqual(17);
  });

  it('stretches to cover a record that falls outside the opening hours', () => {
    const early = new Date(2026, 7, 17, 6, 0).toISOString();
    const window = computeDayWindow(
      [{ startUtc: early, endUtc: new Date(2026, 7, 17, 7, 0).toISOString() }],
      openingHours,
      monday,
    );

    expect(window.start.getTime()).toBeLessThanOrEqual(new Date(early).getTime());
  });

  it('maps the window start to zero and the window end to the full width', () => {
    const window = computeDayWindow([], openingHours, monday);

    expect(timeToPxCompact(window.start.toISOString(), window, DAY_WIDTH)).toBe(0);
    expect(timeToPxCompact(window.end.toISOString(), window, DAY_WIDTH)).toBeCloseTo(DAY_WIDTH);
  });

  it('round-trips through the compact coordinate system', () => {
    const window = computeDayWindow([], openingHours, monday);
    const target = new Date(window.start.getTime() + 90 * 60 * 1000);

    const px = timeToPxCompact(target.toISOString(), window, DAY_WIDTH);
    const back = pxToUtcIsoCompact(px, window, DAY_WIDTH);

    expect(new Date(back).getTime()).toBeCloseTo(target.getTime(), -2);
  });

  it('builds one window per day', () => {
    const days = [monday, new Date(2026, 7, 18), new Date(2026, 7, 19)];
    const windows = computeWeekWindows(days, [], openingHours);

    expect(windows.size).toBe(3);
    for (const window of windows.values()) {
      expect(window.end.getTime()).toBeGreaterThan(window.start.getTime());
    }
  });
});
