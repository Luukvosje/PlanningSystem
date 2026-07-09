import { computed, markRaw, ref, type Component, type ComputedRef, type Ref } from 'vue'
import type { z } from 'zod'
import { LazyFormEditModal } from '#components'
import type { FormControl, FormSubmitConfig, MaybeRefOrGetter } from './control-types'
import { resolveMaybeRefOrGetter } from './control-types'
import { registerEdit, syncEditOptions, useEditRegistry } from './edit-registry'
import { useForm, type FormClassOptions } from './Form'

export const EDIT_INJECTION_KEY = Symbol('form-edit')

export type EditFooterMode = 'submit' | 'close-only'

export interface UseEditOptions<TSchema extends z.ZodType, TEntity>
  extends Pick<FormClassOptions<TSchema>, 'schema' | 'controls' | 'validateOn' | 'genericErrorMessage'> {
  title: MaybeRefOrGetter<string>
  description?: MaybeRefOrGetter<string | undefined>
  submitLabel?: MaybeRefOrGetter<string>
  cancelLabel?: MaybeRefOrGetter<string>
  submit?: MaybeRefOrGetter<FormSubmitConfig>
  footerMode?: MaybeRefOrGetter<EditFooterMode>
  closeOnSuccess?: boolean
  modalUi?: Record<string, unknown>
  bodyExtra?: Component
  extensions?: Record<string, Ref<unknown>>
  toState: (entity: TEntity) => Partial<z.infer<TSchema>>
  onSubmit: (entity: TEntity, data: z.infer<TSchema>) => Promise<void> | void
  onOpen?: (entity: TEntity) => void
  onClose?: () => void
}

export interface EditInstance<TSchema extends z.ZodType = z.ZodType, TEntity = unknown> {
  id: symbol
  entity: Ref<TEntity | null>
  form: ReturnType<typeof useForm<TSchema>>
  title: ComputedRef<string>
  description: ComputedRef<string | undefined>
  submitLabel: ComputedRef<string>
  cancelLabel: ComputedRef<string>
  footerMode: ComputedRef<EditFooterMode>
  modalUi: ComputedRef<Record<string, unknown>>
  bodyExtra: ComputedRef<Component | undefined>
  extensions: ComputedRef<Record<string, Ref<unknown>>>
  open: (entity: TEntity) => void
  close: () => void
}

export function useEdit<TSchema extends z.ZodType, TEntity>(
  id: symbol,
  options: UseEditOptions<TSchema, TEntity>,
): EditInstance<TSchema, TEntity> {
  const optionsRef = syncEditOptions(id, options)

  const existing = useEditRegistry().value.find(item => item.id === id)

  if (existing) {
    return existing as EditInstance<TSchema, TEntity>
  }

  const overlay = useOverlay()
  const modal = overlay.create(LazyFormEditModal)

  const entity = ref<TEntity | null>(null) as Ref<TEntity | null>

  const title = computed(() => resolveMaybeRefOrGetter(optionsRef.value.title))
  const description = computed(() => optionsRef.value.description
    ? resolveMaybeRefOrGetter(optionsRef.value.description)
    : undefined)
  const submitLabel = computed(() => resolveMaybeRefOrGetter(optionsRef.value.submitLabel ?? 'Opslaan'))
  const cancelLabel = computed(() => resolveMaybeRefOrGetter(optionsRef.value.cancelLabel ?? 'Annuleren'))
  const footerMode = computed(() => resolveMaybeRefOrGetter(optionsRef.value.footerMode ?? 'submit'))
  const modalUi = computed(() => optionsRef.value.modalUi ?? { content: 'sm:max-w-lg' })
  const bodyExtra = computed(() => optionsRef.value.bodyExtra)
  const extensions = computed(() => optionsRef.value.extensions ?? {})

  const form = markRaw(useForm({
    schema: options.schema,
    initialState: {} as Partial<z.infer<TSchema>>,
    controls: options.controls,
    validateOn: options.validateOn,
    genericErrorMessage: options.genericErrorMessage,
    submit: options.submit ?? { hidden: true },
    onSubmit: async (data) => {
      if (!entity.value) {
        return
      }

      await optionsRef.value.onSubmit(entity.value, data)

      if (optionsRef.value.closeOnSuccess !== false) {
        close()
      }
    },
  }))

  function handleClosed() {
    form.reset()
    entity.value = null
    optionsRef.value.onClose?.()
  }

  function open(value: TEntity) {
    entity.value = value
    form.reset(optionsRef.value.toState(value))
    optionsRef.value.onOpen?.(value)
    modal.open({ edit: instance as EditInstance }).then(handleClosed)
  }

  function close() {
    modal.close()
  }

  const instance: EditInstance<TSchema, TEntity> = {
    id,
    entity,
    form,
    title,
    description,
    submitLabel,
    cancelLabel,
    footerMode,
    modalUi,
    bodyExtra,
    extensions,
    open,
    close,
  }

  return registerEdit(instance) as EditInstance<TSchema, TEntity>
}
