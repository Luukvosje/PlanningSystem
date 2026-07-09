import type { CurrentUserResponse, LoginRequest, RegisterRequest } from '~/generated/models'
import {
  getApiAuthMe,
  postApiAuthLogin,
  postApiAuthRegister,
} from '~/generated/api/auth/auth'
import type { OrganizationMembership } from '~/types/api-error'
import { isApiError } from '~/types/api-error'

export interface LoginResult {
  requiresOrganizationSelection: boolean
  memberships?: OrganizationMembership[]
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = useCookie<string | null>('planning_access_token', {
    maxAge: 60 * 60 * 24,
    sameSite: 'lax',
  })

  const organizationId = useCookie<string | null>('planning_organization_id', {
    maxAge: 60 * 60 * 24,
    sameSite: 'lax',
  })

  const currentUser = ref<CurrentUserResponse | null>(null)

  const isAuthenticated = computed(() => !!accessToken.value)

  const hasOrganization = computed(() => !!organizationId.value)

  function setToken(token: string | null | undefined) {
    accessToken.value = token ?? null
    if (!token) {
      currentUser.value = null
    }
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
    setToken(response.accessToken)

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
    if (!accessToken.value || !hasOrganization.value) {
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
    setToken(null)
    setOrganizationId(null)
  }

  return {
    accessToken,
    organizationId,
    currentUser,
    isAuthenticated,
    hasOrganization,
    setToken,
    setOrganizationId,
    register,
    login,
    fetchMe,
    selectOrganization,
    logout,
  }
})
