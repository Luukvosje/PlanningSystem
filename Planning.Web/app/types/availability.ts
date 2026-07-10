export type AvailabilityRuleType = 'Weekly' | 'OneTime'
export type AvailabilityRuleStatus = 'Unavailable' | 'Preferred' | 'Available'
export type Weekday = 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday' | 'Sunday'

export interface AvailabilityRule {
  id: string
  employeeId: string
  type: AvailabilityRuleType
  weekday?: Weekday | null
  date?: string | null
  startTime: string
  endTime: string
  status: AvailabilityRuleStatus
  reason?: string | null
  createdAtUtc?: string
  updatedAtUtc?: string
}

export interface AvailabilityRulesListResponse {
  items: AvailabilityRule[]
}

export interface UnavailablePeriod {
  employeeId: string
  date: string
  startTime: string
  endTime: string
  status: AvailabilityRuleStatus
  reason?: string | null
  ruleId: string
}

export interface PlanningAvailabilityResponse {
  periods: UnavailablePeriod[]
}

export interface CreateAvailabilityRuleRequest {
  employeeId: string
  type: AvailabilityRuleType
  weekday?: Weekday | null
  date?: string | null
  startTime: string
  endTime: string
  status: AvailabilityRuleStatus
  reason?: string | null
}

export interface UpdateAvailabilityRuleRequest {
  weekday?: Weekday | null
  date?: string | null
  startTime: string
  endTime: string
  status: AvailabilityRuleStatus
  reason?: string | null
}

export const WEEKDAY_LABELS: Record<Weekday, string> = {
  Monday: 'maandag',
  Tuesday: 'dinsdag',
  Wednesday: 'woensdag',
  Thursday: 'donderdag',
  Friday: 'vrijdag',
  Saturday: 'zaterdag',
  Sunday: 'zondag',
}

export const WEEKDAY_OPTIONS = (Object.keys(WEEKDAY_LABELS) as Weekday[]).map(day => ({
  label: WEEKDAY_LABELS[day].charAt(0).toUpperCase() + WEEKDAY_LABELS[day].slice(1),
  value: day,
}))

export const WHOLE_DAY_START = '00:00:00'
export const WHOLE_DAY_END = '23:59:00'
