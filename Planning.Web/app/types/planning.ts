export type PlanningStatus = 'Planned' | 'Confirmed' | 'Completed' | 'Cancelled'
export type PlanningRowMode = 'resource' | 'customer'
export type TimelineZoom = '15m' | '30m' | '1h' | '2h' | '4h' | 'day' | 'week' | 'month'

export interface PlanningRecord {
  id: string
  organizationId: string
  assignedUserId: string
  assignedUserName: string
  customerId?: string | null
  customerName?: string | null
  title: string
  description?: string | null
  notes?: string | null
  startUtc: string
  endUtc: string
  status: PlanningStatus
  color: string
  hasOverlap: boolean
  createdAtUtc?: string
  updatedAtUtc?: string
}

export interface PlanningListResponse {
  items: PlanningRecord[]
  totalCount: number
  page: number
  pageSize: number
  rangeStartUtc: string
  rangeEndUtc: string
}

export interface CreatePlanningRequest {
  assignedUserId: string
  customerId?: string | null
  title: string
  description?: string | null
  notes?: string | null
  startUtc: string
  endUtc: string
  color?: string | null
  status?: PlanningStatus
}

export interface UpdatePlanningRequest {
  assignedUserId: string
  customerId?: string | null
  title: string
  description?: string | null
  notes?: string | null
  startUtc: string
  endUtc: string
  status: PlanningStatus
  color?: string | null
}

export interface MovePlanningRequest {
  assignedUserId: string
  customerId?: string | null
  startUtc: string
  endUtc: string
}

export interface DuplicatePlanningRequest {
  startUtc?: string | null
  assignedUserId?: string | null
}

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
] as const

export type PlanningColor = typeof PLANNING_COLORS[number]
