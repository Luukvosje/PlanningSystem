import type { FetchOptions } from 'ofetch';
import { parseApiError } from './parseApiError';

export type CustomFetchOptions = RequestInit & {
  params?: Record<string, unknown>
  /** Internal: skip refresh/retry on 401 (used for auth endpoints and retries). */
  _skipAuthRefresh?: boolean
}

const AUTH_REFRESH_PATHS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
];

type RefreshedTokens = { accessToken: string, refreshToken: string };

// Keyed by the refresh token string being spent, not by auth-store instance:
// several independent composable calls in one SSR request can each end up
// with their own auth-store object (a Nuxt/Pinia SSR quirk), so instance
// identity isn't a reliable dedupe key. The token string is the one thing
// they all actually share. Without this, two calls needing a refresh at once
// would both spend the same (single-use, rotating) refresh token — one
// succeeds, the other's now-stale attempt fails and logs out, wiping the
// cookies the first call just correctly set.
const refreshPromises = new Map<string, Promise<RefreshedTokens | null>>();

function isAuthRefreshExcluded(url: string): boolean {
  return AUTH_REFRESH_PATHS.some((path) => url.includes(path));
}

function isJsonBody(body: BodyInit | null | undefined): boolean {
  return !!body && !(body instanceof FormData) && typeof body === 'string';
}

// authStore/baseURL/locale are captured by the caller before crossing any
// async boundary and passed in as plain values. Re-deriving them here via
// useAuthStore()/useRuntimeConfig() would resolve against a fresh, empty
// store on the server: the /api/auth/refresh round trip below sits between
// this function being entered and the rest of it running, and re-invoking
// composables after that gap does not reliably land back on the same
// per-request Pinia instance.
async function refreshAccessToken(
  authStore: ReturnType<typeof useAuthStore>,
  baseURL: string,
  locale: string,
): Promise<boolean> {
  const currentRefreshToken = authStore.refreshToken;
  if (!currentRefreshToken) {
    return false;
  }

  let promise = refreshPromises.get(currentRefreshToken);
  if (!promise) {
    promise = (async () => {
      try {
        // Deliberately a raw $fetch and not the generated postApiAuthRefresh: that one routes
        // through customFetch below, which is what triggers this refresh in the first place -
        // going through it here would recurse on every 401.
        const response = await $fetch<{
          accessToken?: string | null
          refreshToken?: string | null
        }>('/api/auth/refresh', {
          baseURL,
          method: 'POST',
          headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
            'Accept-Language': locale,
          },
          body: {
            refreshToken: currentRefreshToken,
          },
        });

        if (!response.accessToken || !response.refreshToken) {
          return null;
        }

        return { accessToken: response.accessToken, refreshToken: response.refreshToken };
      } catch {
        return null;
      }
    })().finally(() => {
      refreshPromises.delete(currentRefreshToken);
    });

    refreshPromises.set(currentRefreshToken, promise);
  }

  const result = await promise;
  if (!result) {
    return false;
  }

  authStore.setTokens(result.accessToken, result.refreshToken);
  return true;
}

export const customFetch = async <T>(
  url: string,
  options: CustomFetchOptions = {},
): Promise<T> => {
  const nuxtApp = useNuxtApp();
  const config = useRuntimeConfig();
  const authStore = useAuthStore();
  const locale = nuxtApp.$i18n.locale;
  const baseURL = (config.public.apiBaseUrl as string) || (config.apiBaseUrl as string);

  // Every composable this request needs is resolved here, before the first
  // await, and the retry below reuses those captured values instead of
  // re-entering customFetch. Re-resolving useNuxtApp()/useAuthStore() after
  // the /api/auth/refresh round trip throws NUXT_E1001 on the server: by then
  // the Nuxt instance is no longer the ambient one, so the retry after a
  // *successful* refresh would fail and log the user out.
  const run = async (skipAuthRefresh: boolean): Promise<T> => {
    const fetchOptions: FetchOptions = {
      baseURL,
      method: options.method ?? 'GET',
      headers: {
        Accept: 'application/json',
        'Accept-Language': locale.value,
        ...(isJsonBody(options.body) ? { 'Content-Type': 'application/json' } : {}),
        ...(authStore.accessToken ?
          { Authorization: `Bearer ${authStore.accessToken}` } :
          {}),
        ...(authStore.organizationId ?
          { 'X-Organization-Id': authStore.organizationId } :
          {}),
        ...(options.headers as Record<string, string> | undefined),
      },
      body: options.body,
      params: options.params,
      onResponseError: async ({ response }) => {
        if (
          response.status === 401 &&
          !skipAuthRefresh &&
          !isAuthRefreshExcluded(url)
        ) {
          const refreshed = await refreshAccessToken(authStore, baseURL, locale.value);
          if (refreshed) {
            throw Object.assign(new Error('TOKEN_REFRESHED'), { __tokenRefreshed: true });
          }

          authStore.logout();

          // navigateTo() expects to run from middleware/setup/a plugin. Calling
          // it from here (an ofetch response hook, several async hops removed
          // from any of those) is only safe on the client, where there is one
          // ambient app instance to redirect. On the server, route middleware
          // (auth.global.ts / module.global.ts) already redirects on this same
          // failure using a supported context, so just let the 401 propagate.
          if (import.meta.client) {
            await navigateTo('/login');
          }
        }

        throw await nuxtApp.runWithContext(() => parseApiError(response));
      },
    };

    try {
      return await $fetch<T>(url, fetchOptions);
    } catch (error) {
      if (
        error &&
        typeof error === 'object' &&
        '__tokenRefreshed' in error &&
        (error as { __tokenRefreshed?: boolean }).__tokenRefreshed
      ) {
        return run(true);
      }

      throw error;
    }
  };

  return run(options._skipAuthRefresh === true);
};
