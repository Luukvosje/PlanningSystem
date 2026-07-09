import { customFetch } from './apiClient'
import type {
  AvailabilityEntry,
  UpsertDayPartRequest,
  UpsertTimeBlockRequest,
  WeekAvailabilityResponse,
} from '~/types/availability'

function normalizeEntry(raw: Record<string, unknown>): AvailabilityEntry {
  return {
    id: String(raw.id ?? raw.Id),
    userId: String(raw.userId ?? raw.UserId),
    date: String(raw.date ?? raw.Date).slice(0, 10),
    type: (raw.type ?? raw.Type) as AvailabilityEntry['type'],
    dayPart: (raw.dayPart ?? raw.DayPart) as AvailabilityEntry['dayPart'],
    startTime: (raw.startTime ?? raw.StartTime) as string | null | undefined,
    endTime: (raw.endTime ?? raw.EndTime) as string | null | undefined,
    isAvailable: Boolean(raw.isAvailable ?? raw.IsAvailable),
    source: (raw.source ?? raw.Source) as AvailabilityEntry['source'],
    lastModifiedByUserId: String(raw.lastModifiedByUserId ?? raw.LastModifiedByUserId),
    lastModifiedByName: String(raw.lastModifiedByName ?? raw.LastModifiedByName ?? 'Onbekend'),
    note: (raw.note ?? raw.Note) as string | null | undefined,
    createdAtUtc: (raw.createdAtUtc ?? raw.CreatedAtUtc) as string | undefined,
    updatedAtUtc: (raw.updatedAtUtc ?? raw.UpdatedAtUtc) as string | undefined,
  }
}

function normalizeWeekResponse(raw: Record<string, unknown>): WeekAvailabilityResponse {
  const items = (raw.items ?? raw.Items ?? []) as Record<string, unknown>[]
  return {
    items: items.map(normalizeEntry),
    rangeStart: String(raw.rangeStart ?? raw.RangeStart).slice(0, 10),
    rangeEnd: String(raw.rangeEnd ?? raw.RangeEnd).slice(0, 10),
  }
}

export interface WeekAvailabilityParams {
  weekStartUtc: string
  userId?: string
  userIds?: string
}

export function getAvailabilityWeek(params: WeekAvailabilityParams) {
  return customFetch<WeekAvailabilityResponse>('/api/availability/week', {
    params: {
      WeekStartUtc: params.weekStartUtc,
      UserId: params.userId,
      UserIds: params.userIds,
    },
  }).then(response => normalizeWeekResponse(response as unknown as Record<string, unknown>))
}

export function upsertDayPartAvailability(request: UpsertDayPartRequest) {
  return customFetch<AvailabilityEntry | null>('/api/availability/day-parts', {
    method: 'PUT',
    body: JSON.stringify(request),
  }).then((response) => {
    if (!response) return null
    return normalizeEntry(response as unknown as Record<string, unknown>)
  })
}

export function upsertTimeBlockAvailability(request: UpsertTimeBlockRequest) {
  return customFetch<AvailabilityEntry>('/api/availability/time-blocks', {
    method: 'PUT',
    body: JSON.stringify(request),
  }).then(response => normalizeEntry(response as unknown as Record<string, unknown>))
}

export function deleteAvailability(id: string) {
  return customFetch<void>(`/api/availability/${id}`, { method: 'DELETE' })
}
