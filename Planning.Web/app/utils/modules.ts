import type { AppModule, ModuleSettingResponse } from '~/generated/models'
import { AppModule as AppModuleEnum } from '~/generated/models'

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

export function hasModule(
  modules: ModuleSettingResponse[] | null | undefined,
  key: AppModule,
): boolean {
  return modules?.some(module => module.key === key && module.isEnabled) ?? false
}

export function modulesToRequest(modules: Record<AppModule, boolean>) {
  return {
    planning: modules[AppModuleEnum.Planning],
    klant: modules[AppModuleEnum.Klant],
    beheer: modules[AppModuleEnum.Beheer],
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
  return !required || hasModule(modules, required)
}
