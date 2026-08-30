import { UserRole } from '~/generated/models';
import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

function roleLabels(t: Translate): Record<UserRole, string> {
  return {
    [UserRole.Owner]: t('roles.owner'),
    [UserRole.Admin]: t('roles.admin'),
    [UserRole.Planner]: t('roles.planner'),
    [UserRole.Employee]: t('roles.employee'),
  };
}

export function getRoleLabel(role: UserRole | string | null | undefined, t: Translate): string {
  if (!role) {
    return t('common.unknown');
  }

  return roleLabels(t)[role as UserRole] ?? role;
}

export function canManageOrganization(role?: UserRole | string | null): boolean {
  return role === UserRole.Owner || role === UserRole.Admin;
}

export function canManageInvites(role?: UserRole | string | null): boolean {
  return canManageOrganization(role);
}

export function canManagePlanning(role?: UserRole | string | null): boolean {
  return role === UserRole.Owner ||
    role === UserRole.Admin ||
    role === UserRole.Planner;
}

/** Owner, Admin and Planner can manage customers; Employee (viewer) is read-only. */
export function canManageCustomers(role?: UserRole | string | null): boolean {
  return canManagePlanning(role);
}

function assignableRoleOptions(t: Translate) {
  return [
    { label: t('roles.admin'), value: UserRole.Admin },
    { label: t('roles.planner'), value: UserRole.Planner },
    { label: t('roles.employee'), value: UserRole.Employee },
  ];
}

export function canEditUserRole(
  managerRole?: UserRole | string | null,
  targetRole?: UserRole | string | null,
): boolean {
  if (!canManageOrganization(managerRole)) {
    return false;
  }

  return targetRole !== UserRole.Owner;
}

/**
 * Deactivating is a lockout - the account cannot log in and loses its organization context - so
 * the two cases you could not undo from the UI are excluded here as well as on the API: the
 * owner, and yourself.
 */
export function canEditUserStatus(
  manager?: { id?: string | null, role?: UserRole | string | null },
  target?: { id?: string | null, role?: UserRole | string | null },
): boolean {
  return canEditUserRole(manager?.role, target?.role) && manager?.id !== target?.id;
}

export function getAssignableRoleOptions(
  t: Translate,
  currentUserId?: string | null,
  targetUser?: { id?: string, role?: UserRole | string | null },
) {
  const options = assignableRoleOptions(t);

  if (
    targetUser?.id === currentUserId &&
    targetUser?.role === UserRole.Admin
  ) {
    return options.filter((option) => option.value === UserRole.Admin);
  }

  return options;
}
