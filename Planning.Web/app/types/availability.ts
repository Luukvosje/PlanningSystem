import type { Composer } from 'vue-i18n';
import type { AvailabilityRuleResponse, UserRole, Weekday } from '~/generated/models';
import { canManagePlanning } from '~/utils/userRole';

type Translate = Composer['t']

/**
 * The wire shapes below are generated from the OpenAPI spec (`npm run generate:api`) and
 * re-exported here under the names the app already uses, so there is exactly one definition of
 * the contract. Everything after them is UI-only.
 */
export type {
  AvailabilityRuleResponse as AvailabilityRule,
  AvailabilityRulesListResponse,
  UnavailablePeriodResponse as UnavailablePeriod,
  PlanningAvailabilityResponse,
  CreateAvailabilityRuleRequest,
  UpdateAvailabilityRuleRequest,
  AvailabilityRuleType,
  AvailabilityRuleStatus,
  Weekday,
} from '~/generated/models';

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

/**
 * Whether this caller may change or remove the rule. Mirrors AvailabilityRuleService: your own
 * absence is yours to edit and delete, a planner may do it for anyone. The API stays the boundary;
 * this only keeps a button off screen that would always come back 403.
 */
export function canManageAvailabilityRule(
  rule: Pick<AvailabilityRuleResponse, 'employeeId'>,
  user?: { userId?: string | null, role?: UserRole | string | null } | null,
): boolean {
  return canManagePlanning(user?.role) || rule.employeeId === user?.userId;
}
