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
    range: (start?: string, end?: string, filterKey?: string) =>
      ['planning', 'range', start, end, filterKey] as const,
    detail: (id: string) => ['planning', 'detail', id] as const,
    nextShift: (userId?: string) => ['planning', 'nextShift', userId] as const,
    forEntity: (kind: 'customer' | 'user', id: string, start?: string, end?: string) =>
      ['planning', 'entity', kind, id, start, end] as const,
  },
  availability: {
    rules: (employeeId?: string) => ['availability', 'rules', employeeId] as const,
    planning: (start?: string, end?: string, employeeIds?: string) =>
      ['availability', 'planning', start, end, employeeIds] as const,
  },
} as const;
