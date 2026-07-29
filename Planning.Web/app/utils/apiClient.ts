import type { FetchOptions } from 'ofetch'
import { parseApiError } from './parseApiError'

export type CustomFetchOptions = RequestInit & {
  params?: Record<string, unknown>
  /** Internal: skip refresh/retry on 401 (used for auth endpoints and retries). */
  _skipAuthRefresh?: boolean
}

const AUTH_REFRESH_PATHS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
]

let refreshPromise: Promise<boolean> | null = null

function isAuthRefreshExcluded(url: string): boolean {
  return AUTH_REFRESH_PATHS.some(path => url.includes(path))
}

function isJsonBody(body: BodyInit | null | undefined): boolean {
  return !!body && !(body instanceof FormData) && typeof body === 'string'
}

async function refreshAccessToken(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise
  }

  refreshPromise = (async () => {
    const config = useRuntimeConfig()
    const authStore = useAuthStore()

    if (!authStore.refreshToken) {
      return false
    }

    try {
      const response = await $fetch<{
        accessToken?: string | null
        refreshToken?: string | null
      }>('/api/auth/refresh', {
        baseURL: config.public.apiBaseUrl,
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: {
          refreshToken: authStore.refreshToken,
        },
      })

      if (!response.accessToken || !response.refreshToken) {
        return false
      }

      authStore.setTokens(response.accessToken, response.refreshToken)
      return true
    }
    catch {
      return false
    }
  })().finally(() => {
    refreshPromise = null
  })

  return refreshPromise
}

export const customFetch = async <T>(
  url: string,
  options: CustomFetchOptions = {},
): Promise<T> => {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()
  const skipAuthRefresh = options._skipAuthRefresh === true

  const fetchOptions: FetchOptions = {
    baseURL: config.public.apiBaseUrl,
    method: options.method ?? 'GET',
    headers: {
      Accept: 'application/json',
      ...(isJsonBody(options.body) ? { 'Content-Type': 'application/json' } : {}),
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
      if (
        response.status === 401
        && !skipAuthRefresh
        && !isAuthRefreshExcluded(url)
        && typeof window !== 'undefined'
      ) {
        const refreshed = await refreshAccessToken()
        if (refreshed) {
          throw Object.assign(new Error('TOKEN_REFRESHED'), { __tokenRefreshed: true })
        }

        authStore.logout()
        await navigateTo('/login')
      }

      throw await parseApiError(response)
    },
  }

  try {
    return await $fetch<T>(url, fetchOptions)
  }
  catch (error) {
    if (
      error
      && typeof error === 'object'
      && '__tokenRefreshed' in error
      && (error as { __tokenRefreshed?: boolean }).__tokenRefreshed
    ) {
      return customFetch<T>(url, { ...options, _skipAuthRefresh: true })
    }

    throw error
  }
}
