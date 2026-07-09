import { UserRole } from '~/generated/models'

const roleLabels: Record<UserRole, string> = {
  [UserRole.Owner]: 'Eigenaar',
  [UserRole.Admin]: 'Beheerder',
  [UserRole.Planner]: 'Planner',
  [UserRole.Employee]: 'Medewerker',
}

export function getRoleLabel(role?: UserRole | string | null): string {
  if (!role) {
    return 'Onbekend'
  }

  return roleLabels[role as UserRole] ?? role
}

export function canManageOrganization(role?: UserRole | string | null): boolean {
  return role === UserRole.Owner || role === UserRole.Admin
}

export function canManageInvites(role?: UserRole | string | null): boolean {
  return canManageOrganization(role)
}

export function canManagePlanning(role?: UserRole | string | null): boolean {
  return role === UserRole.Owner
    || role === UserRole.Admin
    || role === UserRole.Planner
}

export const assignableRoleOptions = [
  { label: 'Beheerder', value: UserRole.Admin },
  { label: 'Planner', value: UserRole.Planner },
  { label: 'Medewerker', value: UserRole.Employee },
]

export function canEditUserRole(
  managerRole?: UserRole | string | null,
  targetRole?: UserRole | string | null,
): boolean {
  if (!canManageOrganization(managerRole)) {
    return false
  }

  return targetRole !== UserRole.Owner
}

export function getAssignableRoleOptions(
  currentUserId?: string | null,
  targetUser?: { id?: string, role?: UserRole | string | null },
) {
  if (
    targetUser?.id === currentUserId
    && targetUser?.role === UserRole.Admin
  ) {
    return assignableRoleOptions.filter(option => option.value === UserRole.Admin)
  }

  return assignableRoleOptions
}
