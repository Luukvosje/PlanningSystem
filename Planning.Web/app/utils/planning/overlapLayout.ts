import type { PlanningRecord, TimelineBlockLayout } from '~/types/planning'
import { BASE_ROW_HEIGHT, BLOCK_PADDING, LANE_HEIGHT } from './timelineMath'

function durationMs(record: PlanningRecord): number {
  return new Date(record.endUtc).getTime() - new Date(record.startUtc).getTime()
}

export function layoutOverlappingBlocks(
  records: PlanningRecord[],
  timeToPx: (utc: string) => number,
): Map<string, TimelineBlockLayout> {
  if (records.length === 0) return new Map()

  const sorted = [...records].sort((a, b) => {
    const startDiff = new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime()
    if (startDiff !== 0) return startDiff
    return durationMs(b) - durationMs(a)
  })

  const lanes: number[] = []
  const assignments = new Map<string, number>()

  for (const record of sorted) {
    const startMs = new Date(record.startUtc).getTime()
    let lane = lanes.findIndex(endMs => endMs <= startMs)
    if (lane === -1) {
      lane = lanes.length
      lanes.push(0)
    }
    lanes[lane] = new Date(record.endUtc).getTime()
    assignments.set(record.id, lane)
  }

  const laneCount = Math.max(lanes.length, 1)
  const layouts = new Map<string, TimelineBlockLayout>()

  for (const record of sorted) {
    const lane = assignments.get(record.id) ?? 0
    const leftPx = timeToPx(record.startUtc)
    const rightPx = timeToPx(record.endUtc)
    const widthPx = Math.max(rightPx - leftPx, 4)

    layouts.set(record.id, {
      recordId: record.id,
      lane,
      laneCount,
      leftPx,
      widthPx,
      topPx: lane * LANE_HEIGHT + BLOCK_PADDING,
      heightPx: LANE_HEIGHT - BLOCK_PADDING * 2,
    })
  }

  return layouts
}

export function getRowHeight(laneCount: number): number {
  const lanes = Math.max(laneCount, 1)
  return Math.max(BASE_ROW_HEIGHT, lanes * LANE_HEIGHT + BLOCK_PADDING * 2)
}
