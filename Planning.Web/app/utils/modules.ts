import type { AppModule, ModuleSettingResponse } from '~/generated/models'
import { AppModule as AppModuleEnum, UserRole } from '~/generated/models'

export const MODULE_LABELS: Record<AppModule, string> = {
  [AppModuleEnum.Planning]: 'Planning',
  [AppModuleEnum.Klant]: 'Klanten',
  [AppModuleEnum.Beheer]: 'Beheer',
}

export const ALL_MODULES: AppModule[] = [
  AppModuleEnum.Planning,
  AppModuleEnum.Klant,
  AppModuleEnum.Beheer,
]

export const ORGANIZATION_CONFIGURABLE_MODULES: AppModule[] = [
  AppModuleEnum.Planning,
  AppModuleEnum.Klant,
]

export function isOrganizationConfigurableModule(module: AppModule): boolean {
  return module !== AppModuleEnum.Beheer
}

export interface ModuleToggleState {
  visible: boolean
  checked: boolean
  disabled: boolean
  tooltip?: string
}

export function isAdminRole(role?: UserRole | string | null): boolean {
  return role === UserRole.Owner || role === UserRole.Admin
}

export function hasModule(
  modules: ModuleSettingResponse[] | null | undefined,
  key: AppModule,
): boolean {
  return modules?.some(module => module.key === key && module.isEnabled) ?? false
}

export function isModuleEnabledForOrganization(
  module: AppModule,
  orgModules: ModuleSettingResponse[] | null | undefined,
): boolean {
  if (!isOrganizationConfigurableModule(module)) {
    return true
  }

  return hasModule(orgModules, module)
}

export function canAccessModule(
  module: AppModule,
  effectiveModules: ModuleSettingResponse[] | null | undefined,
): boolean {
  return hasModule(effectiveModules, module)
}

export const hasModuleAccess = canAccessModule

export function canToggleModule(
  role: UserRole | string | null | undefined,
  module: AppModule,
  orgModules: ModuleSettingResponse[] | null | undefined,
): boolean {
  return isModuleEnabledForOrganization(module, orgModules) && !isAdminRole(role)
}

export function getUserModuleToggleStates(
  role: UserRole | string | null | undefined,
  userModules: ModuleSettingResponse[] | null | undefined,
  orgModules: ModuleSettingResponse[] | null | undefined,
): Record<AppModule, ModuleToggleState> {
  const userState = modulesFromSettings(userModules)

  return ALL_MODULES.reduce((states, module) => {
    if (!isModuleEnabledForOrganization(module, orgModules)) {
      states[module] = {
        visible: false,
        checked: false,
        disabled: true,
      }
      return states
    }

    if (isAdminRole(role)) {
      states[module] = {
        visible: true,
        checked: true,
        disabled: true,
        tooltip: 'Beheerders hebben altijd toegang tot alle beschikbare modules',
      }
      return states
    }

    states[module] = {
      visible: true,
      checked: userState[module],
      disabled: false,
    }
    return states
  }, {} as Record<AppModule, ModuleToggleState>)
}

export function getOrganizationModuleToggleStates(
  orgModules: ModuleSettingResponse[] | null | undefined,
): Record<AppModule, ModuleToggleState> {
  const orgState = modulesFromSettings(orgModules)

  return ALL_MODULES.reduce((states, module) => {
    if (!isOrganizationConfigurableModule(module)) {
      states[module] = {
        visible: false,
        checked: false,
        disabled: true,
      }
      return states
    }

    states[module] = {
      visible: true,
      checked: orgState[module],
      disabled: false,
    }
    return states
  }, {} as Record<AppModule, ModuleToggleState>)
}

export function visibleModulesFromToggleStates(
  toggleStates: Record<AppModule, ModuleToggleState>,
): AppModule[] {
  return ALL_MODULES.filter(module => toggleStates[module].visible)
}

export function modulesToRequest(modules: Record<AppModule, boolean>) {
  return {
    planning: modules[AppModuleEnum.Planning],
    klant: modules[AppModuleEnum.Klant],
    beheer: modules[AppModuleEnum.Beheer],
  }
}

export function modulesToOrganizationRequest(modules: Record<AppModule, boolean>) {
  return {
    planning: modules[AppModuleEnum.Planning],
    klant: modules[AppModuleEnum.Klant],
    beheer: true,
  }
}

export function modulesFromSettings(
  settings: ModuleSettingResponse[] | null | undefined,
): Record<AppModule, boolean> {
  const result = {
    [AppModuleEnum.Planning]: false,
    [AppModuleEnum.Klant]: false,
    [AppModuleEnum.Beheer]: false,
  }

  for (const setting of settings ?? []) {
    if (setting.key) {
      result[setting.key] = setting.isEnabled ?? false
    }
  }

  return result
}

const ROUTE_MODULE_MAP: Array<{ prefix: string, module: AppModule }> = [
  { prefix: '/planning', module: AppModuleEnum.Planning },
  { prefix: '/beschikbaarheid', module: AppModuleEnum.Planning },
  { prefix: '/customers', module: AppModuleEnum.Klant },
  { prefix: '/users', module: AppModuleEnum.Beheer },
  { prefix: '/organizations', module: AppModuleEnum.Beheer },
]

export function getRequiredModuleForPath(path: string): AppModule | null {
  for (const entry of ROUTE_MODULE_MAP) {
    if (path === entry.prefix || path.startsWith(`${entry.prefix}/`)) {
      return entry.module
    }
  }

  return null
}

export function canAccessRoute(
  path: string,
  modules: ModuleSettingResponse[] | null | undefined,
): boolean {
  const required = getRequiredModuleForPath(path)
  return !required || canAccessModule(required, modules)
}
