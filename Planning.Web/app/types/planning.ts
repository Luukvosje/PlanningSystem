/**
 * The wire shapes below are generated from the OpenAPI spec (`npm run generate:api`) and
 * re-exported here under the names the app already uses, so there is exactly one definition of
 * the contract. Everything after them is UI-only state that never crosses the wire.
 */
import type { PlanningResponse as PlanningRecord, PlanningStatus } from '~/generated/models';

export type {
  PlanningResponse as PlanningRecord,
  PlanningListResponse,
  CreatePlanningRequest,
  UpdatePlanningRequest,
  MovePlanningRequest,
  DuplicatePlanningRequest,
  PlanningStatus,
} from '~/generated/models';

export type PlanningRowMode = 'resource' | 'customer'
export type TimelineZoom = '15m' | '30m' | '1h' | '2h' | '4h' | 'day' | 'week' | 'month'

export interface PlanningFilters {
  userIds: string[]
  customerIds: string[]
  statuses: PlanningStatus[]
  search: string
}

export interface TimelineBlockLayout {
  recordId: string
  lane: number
  laneCount: number
  leftPx: number
  widthPx: number
  topPx: number
  heightPx: number
}

export interface TimelineRow {
  id: string
  label: string
  records: PlanningRecord[]
}

export interface CreatePlanningDraft {
  assignedUserId: string
  customerId?: string | null
  status?: PlanningStatus
  startUtc: string
  endUtc: string
}

export interface ContextMenuState {
  x: number
  y: number
  recordId: string
}

export interface PlanningFormData {
  title: string
  description: string
  notes: string
  assignedUserId: string
  customerId: string | null
  status: PlanningStatus
  color: string
  startUtc: string
  endUtc: string
}

/** Mirrors PlanningRecord.DefaultColor on the server. */
export const DEFAULT_PLANNING_COLOR = '#6366F1';

export const PLANNING_COLORS = [
  '#6366F1', // Indigo
  '#8B5CF6', // Violet
  '#EC4899', // Pink
  '#EF4444', // Red
  '#F97316', // Orange
  '#EAB308', // Yellow
  '#22C55E', // Green
  '#14B8A6', // Teal
  '#3B82F6', // Blue
  '#64748B', // Slate
] as const;

export type PlanningColor = typeof PLANNING_COLORS[number]
