import { useQueryClient } from '@tanstack/vue-query'
import { getApiAuth } from '~/generated/api/auth/auth'

export function useInfoApi() {
  const queryClient = useQueryClient()

  return {
    getInfo: () => getApiAuth(),
  }
}
