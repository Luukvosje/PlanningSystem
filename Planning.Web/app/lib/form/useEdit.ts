import { markRaw, ref, type Ref } from 'vue';
import type { z } from 'zod';
import type { FormClassOptions } from './Form';
import { useForm } from './Form';
import { registerDirtyForm } from './dirty-registry';
import { createInstanceRegistry } from './instance-registry';

const registry = createInstanceRegistry<EditInstance>();

export interface UseEditOptions<TSchema extends z.ZodType, TEntity>
  extends Pick<FormClassOptions<TSchema>, 'schema' | 'controls' | 'validateOn' | 'genericErrorMessage'> {
  toState: (entity: TEntity) => Partial<z.infer<TSchema>>
  onSubmit: (entity: TEntity, data: z.infer<TSchema>) => Promise<void> | void
}

export interface EditInstance<TSchema extends z.ZodType = z.ZodType, TEntity = unknown> {
  id: symbol
  entity: Ref<TEntity | null>
  form: ReturnType<typeof useForm<TSchema>>
  /** Exposed so a read-only view can render the exact values the form would be filled with. */
  toState: (entity: TEntity) => Partial<z.infer<TSchema>>
  /** Fills the form from an entity and treats those values as the saved state. */
  load: (entity: TEntity) => void
}

/**
 * One editable entity: the form definition plus the entity it is currently bound to.
 *
 * May you edit, then you get the form — there is no read-then-click-Edit step and no modal. A modal
 * would hide the very context you were reading; a swap costs a click for something you are allowed
 * to do anyway. `FormEditableSection` renders the read-only view only for users without the right.
 */
export function useEdit<TSchema extends z.ZodType, TEntity>(
  id: symbol,
  options: UseEditOptions<TSchema, TEntity>,
): EditInstance<TSchema, TEntity> {
  const optionsRef = registry.syncOptions(id, options);

  const existing = registry.find(id);

  if (existing) {
    return existing as EditInstance<TSchema, TEntity>;
  }

  const entity = ref<TEntity | null>(null) as Ref<TEntity | null>;

  const form = markRaw(useForm({
    schema: options.schema,
    initialState: {} as Partial<z.infer<TSchema>>,
    controls: options.controls,
    validateOn: options.validateOn,
    genericErrorMessage: options.genericErrorMessage,
    grid: true,
    // The constructor's registration is scope-based, and this instance outlives the component that
    // happened to create it - so it registers itself below instead, once and for good.
    trackNavigation: false,
    onSubmit: async (data) => {
      if (!entity.value) {
        return;
      }

      await optionsRef.value.onSubmit(entity.value, data);
    },
  }));

  // Registered for the route-leave guard for as long as the app runs; `getDirtyForms()` filters on
  // dirtiness, so a form nobody is editing costs nothing.
  registerDirtyForm(form);

  function load(value: TEntity) {
    entity.value = value;
    form.reset(optionsRef.value.toState(value));
  }

  const instance: EditInstance<TSchema, TEntity> = {
    id,
    entity,
    form,
    toState: (value: TEntity) => optionsRef.value.toState(value),
    load,
  };

  return registry.register(instance) as EditInstance<TSchema, TEntity>;
}
