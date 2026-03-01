import type { ResultObject, User, UserAuthResponse, UserAddRequest } from '~/typescript/types'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null,
    loading: false,
    init: true,
  }),

  getters: {
    isAuthenticated(state): boolean {
      return Boolean(state.user)
    },
  },

  actions: {
    async fetchMe(): Promise<User | null> {
      this.loading = true
      try {
        const { $serviceFactory } = useNuxtApp()
        const result = await $serviceFactory.userService.me()
        this.user = result.success ? result.data : null
        return this.user 
      } finally {
        this.loading = false
        this.init = false
      }
    },

    async login(email: string, password: string): Promise<ResultObject<UserAuthResponse>> {
      this.loading = true
      try {
        const { $serviceFactory } = useNuxtApp()
        const result = await $serviceFactory.userService.login({ email, password })
        if (result.success) {
          await this.fetchMe()
        }
        return result
      } finally {
        this.loading = false
      }
    },

    async register(name: string, email: string, password: string): Promise<ResultObject<User>> {
      this.loading = true
      try {
        const request: UserAddRequest = {
          guid: crypto.randomUUID(),
          name,
          email,
          password,
        }
        const { $serviceFactory } = useNuxtApp()
        return await $serviceFactory.userService.createUser(request)
      } finally {
        this.loading = false
      }
    },

    async logout(): Promise<ResultObject<boolean>> {
      this.loading = true
      try {
        const { $serviceFactory } = useNuxtApp()
        const result = await $serviceFactory.userService.logout()
        this.user = null
        return result
      } finally {
        this.loading = false
      }
    },
  },
})
