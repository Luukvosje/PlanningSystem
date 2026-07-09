import type { QueryClient } from '@tanstack/vue-query'

export function invalidateOrgScopedQueries(queryClient: QueryClient) {
  return queryClient.invalidateQueries({
    predicate: (query) => {
      const root = query.queryKey[0]
      return root !== 'auth' && root !== 'organizations'
    },
  })
}
