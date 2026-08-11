import { customFetch } from './apiClient'
import type {
  CreatePlanningRequest,
  DuplicatePlanningRequest,
  MovePlanningRequest,
  PlanningListResponse,
  PlanningRecord,
  UpdatePlanningRequest,
} from '~/types/planning'

function ensureUtcIso(value: string): string {
  if (!value) return value
  if (value.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(value)) return value
  return `${value}Z`
}

function normalizeRecord(raw: Record<string, unknown>): PlanningRecord {
  return {
    id: String(raw.id ?? raw.Id),
    organizationId: String(raw.organizationId ?? raw.OrganizationId),
    assignedUserId: String(raw.assignedUserId ?? raw.AssignedUserId),
    assignedUserName: String(raw.assignedUserName ?? raw.AssignedUserName ?? 'Onbekend'),
    customerId: (raw.customerId ?? raw.CustomerId) as string | null | undefined,
    customerName: (raw.customerName ?? raw.CustomerName) as string | null | undefined,
    title: String(raw.title ?? raw.Title ?? ''),
    description: (raw.description ?? raw.Description) as string | null | undefined,
    notes: (raw.notes ?? raw.Notes) as string | null | undefined,
    startUtc: ensureUtcIso(String(raw.startUtc ?? raw.StartUtc)),
    endUtc: ensureUtcIso(String(raw.endUtc ?? raw.EndUtc)),
    status: (raw.status ?? raw.Status ?? 'Planned') as PlanningRecord['status'],
    color: String(raw.color ?? raw.Color ?? '#6366F1'),
    hasOverlap: Boolean(raw.hasOverlap ?? raw.HasOverlap),
    createdAtUtc: (raw.createdAtUtc ?? raw.CreatedAtUtc) as string | undefined,
    updatedAtUtc: (raw.updatedAtUtc ?? raw.UpdatedAtUtc) as string | undefined,
  }
}

function normalizeListResponse(raw: Record<string, unknown>): PlanningListResponse {
  const items = (raw.items ?? raw.Items ?? []) as Record<string, unknown>[]
  return {
    items: items.map(normalizeRecord),
    totalCount: Number(raw.totalCount ?? raw.TotalCount ?? 0),
    page: Number(raw.page ?? raw.Page ?? 1),
    pageSize: Number(raw.pageSize ?? raw.PageSize ?? 500),
    rangeStartUtc: String(raw.rangeStartUtc ?? raw.RangeStartUtc),
    rangeEndUtc: String(raw.rangeEndUtc ?? raw.RangeEndUtc),
  }
}

export interface PlanningListParams {
  startUtc: string
  endUtc: string
  userIds?: string
  customerIds?: string
  statuses?: string
  search?: string
  page?: number
  pageSize?: number
}

function buildPlanningListUrl(params: PlanningListParams) {
  const searchParams = new URLSearchParams()
  searchParams.set('StartUtc', params.startUtc)
  searchParams.set('EndUtc', params.endUtc)
  if (params.userIds) searchParams.set('UserIds', params.userIds)
  if (params.customerIds) searchParams.set('CustomerIds', params.customerIds)
  if (params.statuses) searchParams.set('Statuses', params.statuses)
  if (params.search) searchParams.set('Search', params.search)
  if (params.page) searchParams.set('Page', String(params.page))
  if (params.pageSize) searchParams.set('PageSize', String(params.pageSize))
  return `/api/planning?${searchParams.toString()}`
}

export function getPlanningList(params: PlanningListParams) {
  return customFetch<Record<string, unknown>>(buildPlanningListUrl(params))
    .then(normalizeListResponse)
}

export function getPlanningById(id: string) {
  return customFetch<Record<string, unknown>>(`/api/planning/${id}`)
    .then(normalizeRecord)
}

export function createPlanning(request: CreatePlanningRequest) {
  return customFetch<Record<string, unknown>>('/api/planning', {
    method: 'POST',
    body: JSON.stringify(request),
  }).then(normalizeRecord)
}

export function updatePlanning(id: string, request: UpdatePlanningRequest) {
  return customFetch<Record<string, unknown>>(`/api/planning/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  }).then(normalizeRecord)
}

export function movePlanning(id: string, request: MovePlanningRequest) {
  return customFetch<Record<string, unknown>>(`/api/planning/${id}/move`, {
    method: 'PATCH',
    body: JSON.stringify(request),
  }).then(normalizeRecord)
}

export function confirmPlanning(id: string) {
  return customFetch<Record<string, unknown>>(`/api/planning/${id}/confirm`, {
    method: 'PATCH',
  }).then(normalizeRecord)
}

export function duplicatePlanning(id: string, request: DuplicatePlanningRequest = {}) {
  return customFetch<Record<string, unknown>>(`/api/planning/${id}/duplicate`, {
    method: 'POST',
    body: JSON.stringify(request),
  }).then(normalizeRecord)
}

export function deletePlanning(id: string) {
  return customFetch<void>(`/api/planning/${id}`, { method: 'DELETE' })
}
