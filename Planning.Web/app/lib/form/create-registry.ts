import { ref, shallowRef, type Ref, type ShallowRef } from 'vue'
import type { CreateInstance } from './useCreate'

const registry: Ref<CreateInstance[]> = ref([])
const optionsById = new Map<symbol, ShallowRef<unknown>>()

export function syncCreateOptions<T>(id: symbol, options: T): ShallowRef<T> {
  const existing = optionsById.get(id) as ShallowRef<T> | undefined
  if (existing) {
    existing.value = options
    return existing
  }

  const stored = shallowRef(options)
  optionsById.set(id, stored)
  return stored
}

export function registerCreate(instance: CreateInstance) {
  const existing = registry.value.find(item => item.id === instance.id)
  if (existing) {
    return existing
  }

  registry.value = [...registry.value, instance]
  return instance
}

export function useCreateRegistry() {
  return registry
}
