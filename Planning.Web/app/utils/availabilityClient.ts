import { customFetch } from './apiClient'
import type {
  AvailabilityRule,
  AvailabilityRulesListResponse,
  CreateAvailabilityRuleRequest,
  PlanningAvailabilityResponse,
  UpdateAvailabilityRuleRequest,
} from '~/types/availability'

function normalizeRule(raw: Record<string, unknown>): AvailabilityRule {
  return {
    id: String(raw.id ?? raw.Id),
    employeeId: String(raw.employeeId ?? raw.EmployeeId),
    type: (raw.type ?? raw.Type) as AvailabilityRule['type'],
    weekday: (raw.weekday ?? raw.Weekday) as AvailabilityRule['weekday'],
    date: raw.date ?? raw.Date ? String(raw.date ?? raw.Date).slice(0, 10) : null,
    startTime: String(raw.startTime ?? raw.StartTime).slice(0, 8),
    endTime: String(raw.endTime ?? raw.EndTime).slice(0, 8),
    status: (raw.status ?? raw.Status) as AvailabilityRule['status'],
    reason: (raw.reason ?? raw.Reason) as string | null | undefined,
    createdAtUtc: (raw.createdAtUtc ?? raw.CreatedAtUtc) as string | undefined,
    updatedAtUtc: (raw.updatedAtUtc ?? raw.UpdatedAtUtc) as string | undefined,
  }
}

function normalizePeriod(raw: Record<string, unknown>) {
  return {
    employeeId: String(raw.employeeId ?? raw.EmployeeId),
    date: String(raw.date ?? raw.Date).slice(0, 10),
    startTime: String(raw.startTime ?? raw.StartTime).slice(0, 8),
    endTime: String(raw.endTime ?? raw.EndTime).slice(0, 8),
    status: (raw.status ?? raw.Status) as PlanningAvailabilityResponse['periods'][number]['status'],
    reason: (raw.reason ?? raw.Reason) as string | null | undefined,
    ruleId: String(raw.ruleId ?? raw.RuleId),
  }
}

export function getAvailabilityRules(employeeId: string) {
  return customFetch<AvailabilityRulesListResponse>('/api/availability/rules', {
    params: { EmployeeId: employeeId },
  }).then((response) => {
    const raw = response as unknown as Record<string, unknown>
    const items = (raw.items ?? raw.Items ?? []) as Record<string, unknown>[]
    return { items: items.map(normalizeRule) }
  })
}

export function getPlanningAvailability(params: {
  startDate: string
  endDate: string
  employeeIds?: string
}) {
  return customFetch<PlanningAvailabilityResponse>('/api/availability/rules/for-planning', {
    params: {
      StartDate: params.startDate,
      EndDate: params.endDate,
      EmployeeIds: params.employeeIds,
    },
  }).then((response) => {
    const raw = response as unknown as Record<string, unknown>
    const periods = (raw.periods ?? raw.Periods ?? []) as Record<string, unknown>[]
    return { periods: periods.map(normalizePeriod) }
  })
}

export function createAvailabilityRule(request: CreateAvailabilityRuleRequest) {
  return customFetch<AvailabilityRule>('/api/availability/rules', {
    method: 'POST',
    body: JSON.stringify(request),
  }).then(response => normalizeRule(response as unknown as Record<string, unknown>))
}

export function updateAvailabilityRule(id: string, request: UpdateAvailabilityRuleRequest) {
  return customFetch<AvailabilityRule>(`/api/availability/rules/${id}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  }).then(response => normalizeRule(response as unknown as Record<string, unknown>))
}

export function deleteAvailabilityRule(id: string) {
  return customFetch<void>(`/api/availability/rules/${id}`, { method: 'DELETE' })
}
