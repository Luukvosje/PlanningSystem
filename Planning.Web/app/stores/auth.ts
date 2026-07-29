import type { CurrentUserResponse, LoginRequest, RegisterRequest } from '~/generated/models'
import {
  getApiAuthMe,
  postApiAuthLogin,
  postApiAuthRegister,
} from '~/generated/api/auth/auth'
import type { OrganizationMembership } from '~/types/api-error'

export interface LoginResult {
  requiresOrganizationSelection: boolean
  memberships?: OrganizationMembership[]
}

const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 14

export const useAuthStore = defineStore('auth', () => {
  const accessToken = useCookie<string | null>('planning_access_token', {
    maxAge: REFRESH_TOKEN_MAX_AGE,
    sameSite: 'lax',
  })

  const refreshToken = useCookie<string | null>('planning_refresh_token', {
    maxAge: REFRESH_TOKEN_MAX_AGE,
    sameSite: 'lax',
  })

  const organizationId = useCookie<string | null>('planning_organization_id', {
    maxAge: REFRESH_TOKEN_MAX_AGE,
    sameSite: 'lax',
  })

  const currentUser = ref<CurrentUserResponse | null>(null)

  const isAuthenticated = computed(() => !!accessToken.value || !!refreshToken.value)

  const hasOrganization = computed(() => !!organizationId.value)

  function setToken(token: string | null | undefined) {
    accessToken.value = token ?? null
    if (!token) {
      currentUser.value = null
    }
  }

  function setRefreshToken(token: string | null | undefined) {
    refreshToken.value = token ?? null
  }

  function setTokens(access: string | null | undefined, refresh: string | null | undefined) {
    setToken(access)
    setRefreshToken(refresh)
  }

  function setOrganizationId(id: string | null | undefined) {
    organizationId.value = id ?? null
    if (!id) {
      currentUser.value = null
    }
  }

  async function register(request: RegisterRequest) {
    return postApiAuthRegister(request)
  }

  async function login(request: LoginRequest): Promise<LoginResult> {
    const response = await postApiAuthLogin(request)
    setTokens(response.accessToken, response.refreshToken)

    if (response.defaultOrganizationId) {
      setOrganizationId(response.defaultOrganizationId)
    }

    if (response.requiresOrganizationSelection && response.memberships?.length) {
      return {
        requiresOrganizationSelection: true,
        memberships: response.memberships,
      }
    }

    return { requiresOrganizationSelection: false }
  }

  async function fetchMe() {
    if ((!accessToken.value && !refreshToken.value) || !hasOrganization.value) {
      currentUser.value = null
      return null
    }

    const user = await getApiAuthMe()
    currentUser.value = user
    return user
  }

  function selectOrganization(id: string) {
    setOrganizationId(id)
  }

  function logout() {
    setTokens(null, null)
    setOrganizationId(null)
  }

  return {
    accessToken,
    refreshToken,
    organizationId,
    currentUser,
    isAuthenticated,
    hasOrganization,
    setToken,
    setRefreshToken,
    setTokens,
    setOrganizationId,
    register,
    login,
    fetchMe,
    selectOrganization,
    logout,
  }
})
