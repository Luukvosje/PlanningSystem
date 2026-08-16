import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

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
  schedulingConflict?: boolean
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

const WEEKDAY_KEYS: Record<Weekday, string> = {
  Monday: 'availability.weekday.monday',
  Tuesday: 'availability.weekday.tuesday',
  Wednesday: 'availability.weekday.wednesday',
  Thursday: 'availability.weekday.thursday',
  Friday: 'availability.weekday.friday',
  Saturday: 'availability.weekday.saturday',
  Sunday: 'availability.weekday.sunday',
};

function capitalize(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export function getWeekdayLabel(day: Weekday, t: Translate): string {
  return capitalize(t(WEEKDAY_KEYS[day]));
}

export function getWeekdayOptions(t: Translate): { label: string, value: Weekday }[] {
  return (Object.keys(WEEKDAY_KEYS) as Weekday[]).map((day) => ({
    label: getWeekdayLabel(day, t),
    value: day,
  }));
}

export const WHOLE_DAY_START = '00:00:00';
export const WHOLE_DAY_END = '23:59:00';
