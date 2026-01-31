import { ServiceFactory } from '~/typescript/factory/service.factory'
import type { ResultObject, User, UserAuthResponse, UserAddRequest } from '~/typescript/types'

export function useAuth() {
  const user = useState<User | null>('auth:user', () => null)
  const loading = useState<boolean>('auth:loading', () => false)

  const isAuthenticated = computed(() => Boolean(user.value))

  const {  } = useNuxtApp()

  async function fetchMe(serviceFactory: ServiceFactory): Promise<User | null> {
    loading.value = true
    try {
      const result = await serviceFactory.userService.me()
      user.value = result.success ? result.data : null
      return user.value
    } finally {
      loading.value = false
    }
  }

  async function login(email: string, password: string, serviceFactory: ServiceFactory): Promise<ResultObject<UserAuthResponse>> {
    loading.value = true
    try {
      const result = await serviceFactory.userService.login({ email, password })

      if (result.success) {
        await fetchMe(serviceFactory)
      }

      return result
    } finally {
      loading.value = false
    }
  }

  async function register(name: string, email: string, password: string, serviceFactory: ServiceFactory): Promise<ResultObject<User>> {
    loading.value = true
    try {
      const request: UserAddRequest = {
        guid: crypto.randomUUID(),
        name,
        email,
        password,
      }

      return await serviceFactory.userService.createUser(request)
    } finally {
      loading.value = false
    }
  }

  async function logout(serviceFactory: ServiceFactory): Promise<ResultObject<boolean>> {
    loading.value = true
    try {
      const result = await serviceFactory.userService.logout()
      user.value = null
      return result
    } finally {
      loading.value = false
    }
  }

  return {
    user,
    loading,
    isAuthenticated,
    fetchMe,
    login,
    register,
    logout,
  }
}

