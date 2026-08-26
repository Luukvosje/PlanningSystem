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

/**
 * `rejected` means the server looked at the refresh token and said no — the
 * session is genuinely over. `unavailable` means we never got an answer (API
 * down, 5xx, dropped connection), which says nothing about the session and
 * must never cost the user their tokens.
 */
type RefreshOutcome =
  | { result: 'refreshed', tokens: RefreshedTokens }
  | { result: 'rejected' }
  | { result: 'unavailable' };

// Keyed by the refresh token string being spent, not by auth-store instance:
// several independent composable calls in one SSR request can each end up
// with their own auth-store object (a Nuxt/Pinia SSR quirk), so instance
// identity isn't a reliable dedupe key. The token string is the one thing
// they all actually share. Without this, two calls needing a refresh at once
// would both spend the same (single-use, rotating) refresh token — one
// succeeds, the other's now-stale attempt fails and logs out, wiping the
// cookies the first call just correctly set.
const refreshPromises = new Map<string, Promise<RefreshOutcome>>();

// The in-flight map above only covers callers that overlap. Rotation makes
// every *later* holder of an already-spent token a problem too: a second
// browser tab, a request that got its 401 just after the rotation landed, or
// a stale per-request auth store. Each would spend a dead token, get a 401
// and end a session whose current refresh token is perfectly valid — exactly
// the "my cookie is right there but I'm on /login" case. Remembering what a
// spent token turned into lets those callers adopt the rotation instead.
// Successful rotations only, and briefly: on the server this map is shared by
// every request, so it keeps no token longer than one page load needs it.
const SPENT_TOKEN_MEMORY_MS = 30_000;
const spentTokens = new Map<string, RefreshOutcome>();

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
): Promise<RefreshOutcome> {
  const currentRefreshToken = authStore.refreshToken;
  if (!currentRefreshToken) {
    return { result: 'rejected' };
  }

  const alreadySpent = spentTokens.get(currentRefreshToken);
  if (alreadySpent) {
    if (alreadySpent.result === 'refreshed') {
      authStore.setTokens(alreadySpent.tokens.accessToken, alreadySpent.tokens.refreshToken);
    }
    return alreadySpent;
  }

  let promise = refreshPromises.get(currentRefreshToken);
  if (!promise) {
    promise = (async (): Promise<RefreshOutcome> => {
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
          return { result: 'unavailable' };
        }

        return {
          result: 'refreshed',
          tokens: { accessToken: response.accessToken, refreshToken: response.refreshToken },
        };
      } catch (error) {
        // A 4xx is the server rejecting this specific token. Anything else —
        // no response at all, a 5xx, a timeout — means we never got to ask, so
        // the session stays as it is instead of dying on a hiccup.
        const status = (error as { response?: { status?: number } } | null)?.response?.status;
        const rejected = typeof status === 'number' && status >= 400 && status < 500;
        return { result: rejected ? 'rejected' : 'unavailable' };
      }
    })().finally(() => {
      refreshPromises.delete(currentRefreshToken);
    });

    refreshPromises.set(currentRefreshToken, promise);
  }

  const outcome = await promise;

  if (outcome.result === 'refreshed') {
    spentTokens.set(currentRefreshToken, outcome);
    setTimeout(() => spentTokens.delete(currentRefreshToken), SPENT_TOKEN_MEMORY_MS);
    authStore.setTokens(outcome.tokens.accessToken, outcome.tokens.refreshToken);
  }

  return outcome;
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
          const outcome = await refreshAccessToken(authStore, baseURL, locale.value);

          if (outcome.result === 'refreshed') {
            throw Object.assign(new Error('TOKEN_REFRESHED'), { __tokenRefreshed: true });
          }

          // Only an outright rejection ends the session. When the refresh
          // could not be made at all (API down, 5xx), the 401 propagates as a
          // normal error and the tokens stay put, so the next attempt — a
          // retry, a reload — can still recover.
          if (outcome.result === 'rejected') {
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
