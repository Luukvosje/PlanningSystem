import type { UnavailablePeriod } from '~/types/availability';
import type { PlanningRecord } from '~/types/planning';
import { getUnavailableOverlaysForMatrix } from '~/utils/planning/availabilityMath';
import { snapPxToImportantTime } from '~/utils/planning/planningSettings';
import { SNAP_MINUTES, snapPx } from '~/utils/planning/timelineMath';

export interface BlockBounds {
  recordId: string
  leftPx: number
  rightPx: number
}

export interface BlockSnapOptions {
  snapToBlocks?: boolean
  blockSnapPoints?: number[]
  importantTimes?: string[]
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

export function snapPxToTimeline(
  px: number,
  dayWidth: number,
  options: BlockSnapOptions = {},
): number {
  const {
    snapToBlocks = false,
    blockSnapPoints = [],
    importantTimes = [],
    snapMinutes = SNAP_MINUTES,
  } = options;

  const importantSnap = snapPxToImportantTime(px, dayWidth, importantTimes);
  if (importantSnap !== null) {
    return importantSnap;
  }

  if (snapToBlocks && blockSnapPoints.length > 0) {
    const threshold = getSnapThreshold(dayWidth, snapMinutes);
    let nearestBlockPx: number | null = null;
    let nearestBlockDistance = Infinity;

    for (const point of blockSnapPoints) {
      const distance = Math.abs(px - point);
      if (distance <= threshold && distance < nearestBlockDistance) {
        nearestBlockDistance = distance;
        nearestBlockPx = point;
      }
    }

    if (nearestBlockPx !== null) {
      return nearestBlockPx;
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
    blockSnapPoints = [],
    importantTimes = [],
    snapMinutes = SNAP_MINUTES,
  } = options;

  const importantSnap = snapPxToImportantTime(rawLeftPx, dayWidth, importantTimes);
  if (importantSnap !== null) {
    return importantSnap;
  }

  const importantRightSnap = snapPxToImportantTime(rawLeftPx + widthPx, dayWidth, importantTimes);
  if (importantRightSnap !== null) {
    return importantRightSnap - widthPx;
  }

  if (!snapToBlocks || blockSnapPoints.length === 0) {
    return snapPx(rawLeftPx, dayWidth, snapMinutes);
  }

  const threshold = getSnapThreshold(dayWidth, snapMinutes);
  const blockCandidates: number[] = [];

  for (const point of blockSnapPoints) {
    if (Math.abs(rawLeftPx - point) <= threshold) {
      blockCandidates.push(point);
    }

    const leftForRightSnap = point - widthPx;
    if (Math.abs(rawLeftPx + widthPx - point) <= threshold) {
      blockCandidates.push(leftForRightSnap);
    }
  }

  if (blockCandidates.length > 0) {
    return blockCandidates.reduce((best, candidate) =>
      Math.abs(candidate - rawLeftPx) < Math.abs(best - rawLeftPx) ? candidate : best,
    );
  }

  return snapPx(rawLeftPx, dayWidth, snapMinutes);
}
