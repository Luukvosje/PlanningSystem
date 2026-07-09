export const queryKeys = {
  auth: {
    me: ['auth', 'me'] as const,
  },
  organizations: {
    mine: ['organizations', 'mine'] as const,
    current: ['organizations', 'current'] as const,
  },
  users: {
    all: ['users'] as const,
    detail: (id: string) => ['users', id] as const,
  },
  customers: {
    all: ['customers'] as const,
    detail: (id: string) => ['customers', id] as const,
  },
  invites: {
    preview: (code: string) => ['invites', 'preview', code] as const,
  },
  planning: {
    week: (start?: string) => ['planning', 'week', start] as const,
    range: (start?: string, end?: string, filterKey?: string) =>
      ['planning', 'range', start, end, filterKey] as const,
    detail: (id: string) => ['planning', 'detail', id] as const,
  },
  availability: {
    week: (start?: string, userId?: string, userIds?: string) =>
      ['availability', 'week', start, userId, userIds] as const,
  },
} as const
