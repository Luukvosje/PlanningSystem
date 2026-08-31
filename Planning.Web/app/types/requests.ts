import type { Composer } from 'vue-i18n';
import type { RequestResponse } from '~/generated/models';
import { ApprovalStatus, RequestKind } from '~/generated/models';
import { formatOneTimeRuleLabel, formatWeeklyRuleLabel } from '~/utils/planning/availabilityMath';

type Translate = Composer['t']

/**
 * The wire shape is generated (`npm run generate:api`); everything below is how the inbox reads
 * one row out loud. Same split as types/availability.ts.
 */
export type { RequestResponse as PlanningRequest } from '~/generated/models';

const KIND_KEYS: Record<RequestKind, string> = {
  [RequestKind.Leave]: 'requests.kind.leave',
  [RequestKind.Availability]: 'requests.kind.availability',
};

const STATUS_KEYS: Record<ApprovalStatus, string> = {
  [ApprovalStatus.Pending]: 'requests.status.pending',
  [ApprovalStatus.Approved]: 'requests.status.approved',
  [ApprovalStatus.Rejected]: 'requests.status.rejected',
};

export function getRequestKindLabel(kind: RequestKind, t: Translate): string {
  return t(KIND_KEYS[kind]);
}

export function getRequestStatusLabel(status: ApprovalStatus, t: Translate): string {
  return t(STATUS_KEYS[status]);
}

/** Leave is the thing a planner acts on most, so it is the one that gets a colour. */
export function getRequestKindColor(kind: RequestKind): 'info' | 'neutral' {
  return kind === RequestKind.Leave ? 'info' : 'neutral';
}

export function getRequestStatusColor(status: ApprovalStatus): 'warning' | 'success' | 'error' {
  switch (status) {
    case ApprovalStatus.Approved:
      return 'success';
    case ApprovalStatus.Rejected:
      return 'error';
    default:
      return 'warning';
  }
}

/**
 * What is being asked, in one line. Reuses the labels the availability editor already renders, so
 * the same blockade does not read differently in two places.
 */
export function getRequestSummary(request: RequestResponse, t: Translate, locale: string): string {
  if (request.date) {
    return formatOneTimeRuleLabel(
      request.date,
      request.startTime,
      request.endTime,
      t,
      locale,
      request.reason,
    );
  }

  if (request.weekday) {
    const base = formatWeeklyRuleLabel(request.weekday, request.startTime, request.endTime, t);
    return request.reason ? `${base} · ${request.reason}` : base;
  }

  return request.reason ?? '';
}

export function getRequestInitials(request: RequestResponse): string {
  return `${request.employeeFirstName.charAt(0)}${request.employeeLastName.charAt(0)}`.toUpperCase();
}
