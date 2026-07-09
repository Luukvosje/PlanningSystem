export type ApiErrorCode =
  | 'NOT_FOUND'
  | 'VALIDATION_ERROR'
  | 'INTERNAL_ERROR'
  | 'UNAUTHORIZED'
  | 'CONFLICT'
  | 'FORBIDDEN'
  | 'EXPIRED'
  | 'NO_ORGANIZATION'
  | 'ORGANIZATION_SELECTION_REQUIRED'
  | 'UNKNOWN'

export interface OrganizationMembership {
  organizationId?: string
  organizationName?: string | null
  userId?: string
  role?: string
}

export interface ApiError {
  status: number
  message: string
  code: ApiErrorCode
  validationErrors?: Record<string, string[]>
  traceId?: string
  memberships?: OrganizationMembership[]
}

export function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === 'object'
    && error !== null
    && 'status' in error
    && 'message' in error
  )
}
