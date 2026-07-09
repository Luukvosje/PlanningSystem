import type { FetchOptions } from 'ofetch'
import { parseApiError } from './parseApiError'

export type CustomFetchOptions = RequestInit & {
  params?: Record<string, unknown>
}

export const customFetch = async <T>(
  url: string,
  options: CustomFetchOptions = {},
): Promise<T> => {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  const fetchOptions: FetchOptions = {
    baseURL: config.public.apiBaseUrl,
    method: options.method ?? 'GET',
    headers: {
      Accept: 'application/json',
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...(authStore.accessToken
        ? { Authorization: `Bearer ${authStore.accessToken}` }
        : {}),
      ...(authStore.organizationId
        ? { 'X-Organization-Id': authStore.organizationId }
        : {}),
      ...(options.headers as Record<string, string> | undefined),
    },
    body: options.body,
    params: options.params,
    onResponseError: async ({ response }) => {
      const apiError = await parseApiError(response)

      if (response.status === 401 && typeof window !== 'undefined') {
        authStore.logout()
        await navigateTo('/login')
      }

      throw apiError
    },
  }

  return $fetch<T>(url, fetchOptions)
}
