import { ref, shallowRef, type Ref, type ShallowRef } from 'vue';

/**
 * Keeps one live instance per form id, plus a reactive handle on its options.
 *
 * A form is declared in one place (a composable) but rendered somewhere else (a modal), so the
 * instance has to survive re-invocation of the composable and stay addressable by id.
 *
 * The create and edit flows used to have byte-identical copies of this, differing only in the
 * word "Create"/"Edit" - so a fix to one silently did not apply to the other.
 */
export function createInstanceRegistry<TInstance extends { id: symbol }>() {
  const instances = ref([]) as Ref<TInstance[]>;
  const optionsById = new Map<symbol, ShallowRef<unknown>>();

  /** Keeps the stored options ref identical across re-invocations, so watchers stay attached. */
  function syncOptions<TOptions>(id: symbol, options: TOptions): ShallowRef<TOptions> {
    const existing = optionsById.get(id) as ShallowRef<TOptions> | undefined;
    if (existing) {
      existing.value = options;
      return existing;
    }

    const stored = shallowRef(options);
    optionsById.set(id, stored as ShallowRef<unknown>);
    return stored;
  }

  /** Registers an instance, or returns the one already registered under this id. */
  function register(instance: TInstance): TInstance {
    const existing = instances.value.find((item) => item.id === instance.id);
    if (existing) {
      return existing;
    }

    instances.value = [...instances.value, instance];
    return instance;
  }

  function find(id: symbol): TInstance | undefined {
    return instances.value.find((item) => item.id === id);
  }

  return { instances, syncOptions, register, find };
}
