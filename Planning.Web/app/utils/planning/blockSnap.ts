import type { UnavailablePeriod } from '~/types/availability';
import type { PlanningRecord } from '~/types/planning';
import { getUnavailableOverlaysForMatrix } from '~/utils/planning/availabilityMath';
import { SNAP_MINUTES, snapPx } from '~/utils/planning/timelineMath';

export interface BlockBounds {
  recordId: string
  leftPx: number
  rightPx: number
}

export interface BlockSnapOptions {
  snapToBlocks?: boolean
  /**
   * Every pixel a dragged edge may land on when snapping is on: block edges, availability
   * boundaries and the organization's important work times. They arrive as pixels rather than
   * times because a compact day window maps pixels to time non-linearly - recomputing an
   * important time from minutes-of-day would put the magnet somewhere other than the line the
   * planner sees.
   */
  snapPoints?: number[]
  snapMinutes?: number
}

export function getRowBlockBounds(
  records: PlanningRecord[],
  timeToPx: (utc: string) => number,
  excludeRecordId?: string,
): BlockBounds[] {
  return records
    .filter((record) => record.id !== excludeRecordId)
    .map((record) => {
      const leftPx = timeToPx(record.startUtc);
      const rightPx = timeToPx(record.endUtc);
      return { recordId: record.id, leftPx, rightPx };
    });
}

export function collectBlockSnapPoints(bounds: BlockBounds[]): number[] {
  const points = new Set<number>();
  for (const bound of bounds) {
    points.add(bound.leftPx);
    points.add(bound.rightPx);
  }
  return [...points];
}

export function collectOverlaySnapPoints(
  overlays: Array<{ leftPx: number, widthPx: number }>,
): number[] {
  const points = new Set<number>();
  for (const overlay of overlays) {
    points.add(overlay.leftPx);
    points.add(overlay.leftPx + overlay.widthPx);
  }
  return [...points];
}

export function mergeSnapPoints(...pointSets: number[][]): number[] {
  const points = new Set<number>();
  for (const set of pointSets) {
    for (const point of set) {
      points.add(point);
    }
  }
  return [...points];
}

export function getRowAvailabilitySnapPoints(
  rowId: string,
  availabilityPeriods: UnavailablePeriod[],
  rangeStart: Date,
  rangeEnd: Date,
  dayWidth: number,
  rowMode: 'resource' | 'customer',
): number[] {
  if (rowMode !== 'resource' || availabilityPeriods.length === 0) {
    return [];
  }

  // Snap points only need leftPx/widthPx; the tooltip text is discarded, so a
  // passthrough translate function is fine here (no Vue/i18n context in this pure util).
  const overlays = getUnavailableOverlaysForMatrix(
    [],
    rowId,
    rangeStart,
    rangeEnd,
    dayWidth,
    availabilityPeriods,
    (key: string) => key,
  );

  return collectOverlaySnapPoints(overlays);
}

function getSnapThreshold(dayWidth: number, snapMinutes = SNAP_MINUTES): number {
  return (snapMinutes / (24 * 60)) * dayWidth;
}

function nearestSnapPoint(px: number, points: number[], threshold: number): number | null {
  let nearest: number | null = null;
  let nearestDistance = Infinity;

  for (const point of points) {
    const distance = Math.abs(px - point);
    if (distance <= threshold && distance < nearestDistance) {
      nearestDistance = distance;
      nearest = point;
    }
  }

  return nearest;
}

export function snapPxToTimeline(
  px: number,
  dayWidth: number,
  options: BlockSnapOptions = {},
): number {
  const {
    snapToBlocks = false,
    snapPoints = [],
    snapMinutes = SNAP_MINUTES,
  } = options;

  if (snapToBlocks && snapPoints.length > 0) {
    const nearest = nearestSnapPoint(px, snapPoints, getSnapThreshold(dayWidth, snapMinutes));
    if (nearest !== null) {
      return nearest;
    }
  }

  return snapPx(px, dayWidth, snapMinutes);
}

export function snapDragLeftPx(
  rawLeftPx: number,
  widthPx: number,
  dayWidth: number,
  options: BlockSnapOptions = {},
): number {
  const {
    snapToBlocks = false,
    snapPoints = [],
    snapMinutes = SNAP_MINUTES,
  } = options;

  if (!snapToBlocks || snapPoints.length === 0) {
    return snapPx(rawLeftPx, dayWidth, snapMinutes);
  }

  const threshold = getSnapThreshold(dayWidth, snapMinutes);
  // A dragged block snaps on either edge, so every point yields two candidate left positions.
  const candidates: number[] = [];

  for (const point of snapPoints) {
    if (Math.abs(rawLeftPx - point) <= threshold) {
      candidates.push(point);
    }
    if (Math.abs(rawLeftPx + widthPx - point) <= threshold) {
      candidates.push(point - widthPx);
    }
  }

  if (candidates.length === 0) {
    return snapPx(rawLeftPx, dayWidth, snapMinutes);
  }

  return candidates.reduce((best, candidate) =>
    Math.abs(candidate - rawLeftPx) < Math.abs(best - rawLeftPx) ? candidate : best,
  );
}
