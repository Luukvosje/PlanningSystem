import type { Composer } from 'vue-i18n';
import type {
  AvailabilityRule,
  UnavailablePeriod,
  Weekday,
} from '~/types/availability';
import { timeToPx } from '~/utils/planning/timelineMath';
import { atTimeOnDateKey, weekdayFromDate } from '~/utils/planning/timeOfDay';
import { toDateKey } from '~/utils/planning/dateUtils';

type Translate = Composer['t'] | ((key: string, named?: Record<string, unknown>) => string)

export interface UnavailablePeriodView {
  employeeId: string
  start: Date
  end: Date
  label: string
  tooltip: string
}

export interface UnavailableOverlay {
  leftPx: number
  widthPx: number
  tooltip: string
}

function expandRuleForDate(rule: AvailabilityRule, date: Date): UnavailablePeriod | null {
  const dateKey = toDateKey(date);

  if (rule.type === 'Weekly') {
    if (rule.weekday !== weekdayFromDate(date)) {
      return null;
    }
  } else if (rule.date !== dateKey) {
    return null;
  }

  if (rule.status !== 'Unavailable') {
    return null;
  }

  return {
    employeeId: rule.employeeId,
    date: dateKey,
    startTime: rule.startTime,
    endTime: rule.endTime,
    status: rule.status,
    reason: rule.reason,
    ruleId: rule.id,
  };
}

export function expandRulesForDate(
  rules: AvailabilityRule[],
  employeeId: string,
  day: Date,
): UnavailablePeriod[] {
  return rules
    .filter((rule) => rule.employeeId === employeeId)
    .map((rule) => expandRuleForDate(rule, day))
    .filter((period): period is UnavailablePeriod => period !== null);
}

export function expandPlanningPeriodsForDate(
  periods: UnavailablePeriod[],
  employeeId: string,
  day: Date,
): UnavailablePeriod[] {
  const dateKey = toDateKey(day);
  return periods.filter((period) =>
    period.employeeId === employeeId &&
    period.date === dateKey &&
    period.status === 'Unavailable',
  );
}

function buildTooltip(period: UnavailablePeriod, periodLabel: string, t: Translate): string {
  const parts = [t('availability.unavailable'), periodLabel];
  if (period.reason) {
    parts.push(period.reason);
  }
  return parts.join(' · ');
}

function periodToView(period: UnavailablePeriod, day: Date, t: Translate): UnavailablePeriodView {
  const dateKey = toDateKey(day);
  const start = atTimeOnDateKey(dateKey, period.startTime);
  const end = atTimeOnDateKey(dateKey, period.endTime);
  const timeLabel = `${period.startTime.slice(0, 5)}–${period.endTime.slice(0, 5)}`;
  return {
    employeeId: period.employeeId,
    start,
    end,
    label: timeLabel,
    tooltip: buildTooltip(period, timeLabel, t),
  };
}

export function getUnavailablePeriodsForDayFromRules(
  rules: AvailabilityRule[],
  employeeId: string,
  day: Date,
  t: Translate,
): UnavailablePeriodView[] {
  return expandRulesForDate(rules, employeeId, day).map((period) => periodToView(period, day, t));
}

export function getUnavailablePeriodsForDayFromPlanning(
  periods: UnavailablePeriod[],
  employeeId: string,
  day: Date,
  t: Translate,
): UnavailablePeriodView[] {
  return expandPlanningPeriodsForDate(periods, employeeId, day).map((period) => periodToView(period, day, t));
}

export function getUnavailableOverlaysForMatrix(
  rules: AvailabilityRule[],
  employeeId: string,
  rangeStart: Date,
  rangeEnd: Date,
  dayWidth: number,
  planningPeriods: UnavailablePeriod[] | undefined,
  t: Translate,
): UnavailableOverlay[] {
  const overlays: UnavailableOverlay[] = [];
  const cursor = new Date(rangeStart);
  cursor.setHours(0, 0, 0, 0);

  while (cursor < rangeEnd) {
    const periods = planningPeriods ?
      getUnavailablePeriodsForDayFromPlanning(planningPeriods, employeeId, cursor, t) :
      getUnavailablePeriodsForDayFromRules(rules, employeeId, cursor, t);

    for (const period of periods) {
      overlays.push({
        leftPx: timeToPx(period.start.toISOString(), rangeStart, dayWidth),
        widthPx: Math.max(timeToPx(period.end.toISOString(), rangeStart, dayWidth) - timeToPx(period.start.toISOString(), rangeStart, dayWidth), 4),
        tooltip: period.tooltip,
      });
    }
    cursor.setDate(cursor.getDate() + 1);
  }

  return overlays;
}

export function hasAvailabilityConflict(
  employeeId: string,
  startUtc: string,
  endUtc: string,
  rules: AvailabilityRule[],
  t: Translate,
  employeeName?: string,
  planningPeriods?: UnavailablePeriod[],
): { hasConflict: boolean, message?: string } {
  const shiftStart = new Date(startUtc);
  const shiftEnd = new Date(endUtc);
  const day = new Date(shiftStart);
  day.setHours(0, 0, 0, 0);

  const periods = planningPeriods ?
    expandPlanningPeriodsForDate(planningPeriods, employeeId, day) :
    expandRulesForDate(rules, employeeId, day);

  const conflict = periods.find((period) => {
    const periodStart = atTimeOnDateKey(period.date, period.startTime);
    const periodEnd = atTimeOnDateKey(period.date, period.endTime);
    return shiftStart < periodEnd && shiftEnd > periodStart;
  });

  if (!conflict) {
    return { hasConflict: false };
  }

  const name = employeeName ?? t('planning.fields.employee');
  return {
    hasConflict: true,
    message: t('availability.conflictMessage', { name }),
  };
}

type RuleTimeRangeKind = 'allDay' | 'until' | 'after' | 'range'

function classifyRuleTimeRange(startTime: string, endTime: string): RuleTimeRangeKind {
  const start = startTime.slice(0, 5);
  const end = endTime.slice(0, 5);
  if (start === '00:00' && (end === '23:59' || end === '24:00')) {
    return 'allDay';
  }
  if (start === '00:00') {
    return 'until';
  }
  if (end === '23:59' || end === '24:00') {
    return 'after';
  }
  return 'range';
}

export function formatRuleTimeRange(startTime: string, endTime: string, t: Translate): string {
  const start = startTime.slice(0, 5);
  const end = endTime.slice(0, 5);
  switch (classifyRuleTimeRange(startTime, endTime)) {
    case 'allDay':
      return t('availability.time.allDay');
    case 'until':
      return t('availability.time.until', { end });
    case 'after':
      return t('availability.time.after', { start });
    default:
      return `${start}–${end}`;
  }
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

export function formatWeeklyRuleLabel(
  weekday: Weekday,
  startTime: string,
  endTime: string,
  t: Translate,
): string {
  const dayLabel = t(WEEKDAY_KEYS[weekday]);
  const timeLabel = formatRuleTimeRange(startTime, endTime, t);

  switch (classifyRuleTimeRange(startTime, endTime)) {
    case 'allDay':
      return t('availability.weekly.allDay', { day: dayLabel });
    case 'until':
    case 'after':
      return t('availability.weekly.partial', { day: dayLabel, time: timeLabel });
    default:
      return t('availability.weekly.range', { day: dayLabel, time: timeLabel });
  }
}

export function formatOneTimeRuleLabel(
  date: string,
  startTime: string,
  endTime: string,
  t: Translate,
  locale: string,
  reason?: string | null,
): string {
  const dateLabel = new Date(`${date}T12:00:00`).toLocaleDateString(locale, {
    day: 'numeric',
    month: 'long',
  });
  const timeLabel = formatRuleTimeRange(startTime, endTime, t);
  const base = classifyRuleTimeRange(startTime, endTime) === 'allDay' ?
    dateLabel :
    `${dateLabel} ${timeLabel}`;
  return reason ? `${base} · ${reason}` : base;
}
