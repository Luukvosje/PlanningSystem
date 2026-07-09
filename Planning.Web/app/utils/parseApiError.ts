import type { ApiError, OrganizationMembership } from '~/types/api-error'

export async function parseApiError(response: Response): Promise<ApiError> {
  let body: Record<string, unknown> = {}

  try {
    body = await response.json()
  }
  catch {
    // Response body may be empty.
  }

  if (body.errors && typeof body.errors === 'object') {
    return {
      status: response.status,
      message: (body.title as string) ?? 'Validatiefout',
      code: 'VALIDATION_ERROR',
      validationErrors: body.errors as Record<string, string[]>,
    }
  }

  const memberships = Array.isArray(body.memberships)
    ? body.memberships as OrganizationMembership[]
    : undefined

  return {
    status: response.status,
    message: (body.error as string) ?? response.statusText,
    code: (body.errorCode as ApiError['code'])
      ?? (response.status === 409 && memberships ? 'ORGANIZATION_SELECTION_REQUIRED' : 'UNKNOWN'),
    traceId: body.traceId as string | undefined,
    memberships,
  }
}
