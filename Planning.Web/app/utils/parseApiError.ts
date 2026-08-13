import type { FetchResponse } from 'ofetch';
import type { ApiError, OrganizationMembership } from '~/types/api-error';
import { translateBackendMessage } from './backendMessages';

/**
 * `response` here always comes from ofetch's `onResponseError` hook, which has
 * already read and parsed the body into `response._data` — the underlying stream
 * is consumed by then, so calling `response.json()` again silently fails (empty
 * body) and every error falls back to the generic status text. Read the
 * already-parsed data instead of the stream.
 */
export async function parseApiError(response: FetchResponse<unknown>): Promise<ApiError> {
  const { t, locale } = useNuxtApp().$i18n;
  const data = response._data;
  const body: Record<string, unknown> = (data && typeof data === 'object') ?
    data as Record<string, unknown> :
    {};

  if (body.errors && typeof body.errors === 'object') {
    return {
      status: response.status,
      message: translateBackendMessage(body.title as string, locale.value) ?? t('errors.validationFailed'),
      code: 'VALIDATION_ERROR',
      validationErrors: body.errors as Record<string, string[]>,
    };
  }

  const memberships = Array.isArray(body.memberships) ?
    body.memberships as OrganizationMembership[] :
    undefined;

  return {
    status: response.status,
    message: translateBackendMessage(body.error as string, locale.value) ?? response.statusText,
    code: (body.errorCode as ApiError['code']) ??
      (response.status === 409 && memberships ? 'ORGANIZATION_SELECTION_REQUIRED' : 'UNKNOWN'),
    traceId: body.traceId as string | undefined,
    memberships,
  };
}
