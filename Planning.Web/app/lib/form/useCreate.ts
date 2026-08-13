import { computed, markRaw, type Component, type ComputedRef, type Ref } from 'vue';
import type { z } from 'zod';
import { LazyFormCreateModal } from '#components';
import { registerCreate, syncCreateOptions, useCreateRegistry } from './create-registry';
import type { FormSubmitConfig, MaybeRefOrGetter } from './control-types';
import { resolveMaybeRefOrGetter } from './control-types';
import { useForm, type FormClassOptions } from './Form';


export const CREATE_INJECTION_KEY = Symbol('form-create');

export type CreateFooterMode = 'submit' | 'close-only'

export interface UseCreateOptions<TSchema extends z.ZodType>
  extends Pick<FormClassOptions<TSchema>, 'schema' | 'initialState' | 'controls' | 'validateOn' | 'genericErrorMessage'> {
  title: MaybeRefOrGetter<string>
  description?: MaybeRefOrGetter<string | undefined>
  submitLabel?: MaybeRefOrGetter<string>
  cancelLabel?: MaybeRefOrGetter<string>
  submit?: MaybeRefOrGetter<FormSubmitConfig>
  footerMode?: MaybeRefOrGetter<CreateFooterMode>
  closeOnSuccess?: boolean
  modalUi?: Record<string, unknown>
  bodyExtra?: Component
  extensions?: Record<string, Ref<unknown>>
  onSubmit: (data: z.infer<TSchema>) => Promise<void> | void
  onOpen?: () => void
  onClose?: () => void
}

export interface CreateInstance<TSchema extends z.ZodType = z.ZodType> {
  id: symbol
  form: Form<TSchema>
  title: ComputedRef<string>
  description: ComputedRef<string | undefined>
  submitLabel: ComputedRef<string>
  cancelLabel: ComputedRef<string>
  footerMode: ComputedRef<CreateFooterMode>
  modalUi: ComputedRef<Record<string, unknown>>
  bodyExtra: ComputedRef<Component | undefined>
  extensions: ComputedRef<Record<string, Ref<unknown>>>
  open: () => void
  close: () => void
}

export function useCreate<TSchema extends z.ZodType>(
  id: symbol,
  options: UseCreateOptions<TSchema>,
): CreateInstance<TSchema> {
  const optionsRef = syncCreateOptions(id, options);

  const existing = useCreateRegistry().value.find((item) => item.id === id);

  if (existing) {
    return existing as CreateInstance<TSchema>;
  }

  const { t } = useI18n();
  const overlay = useOverlay();
  const modal = overlay.create(LazyFormCreateModal);

  const title = computed(() => resolveMaybeRefOrGetter(optionsRef.value.title));
  const description = computed(() => optionsRef.value.description ?
    resolveMaybeRefOrGetter(optionsRef.value.description) :
    undefined);

  const submitLabel = computed(() => resolveMaybeRefOrGetter(optionsRef.value.submitLabel ?? t('common.actions.save')));
  const cancelLabel = computed(() => resolveMaybeRefOrGetter(optionsRef.value.cancelLabel ?? t('common.actions.cancel')));
  const footerMode = computed(() => resolveMaybeRefOrGetter(optionsRef.value.footerMode ?? 'submit'));
  const modalUi = computed(() => optionsRef.value.modalUi ?? { content: 'sm:max-w-lg' });
  const bodyExtra = computed(() => optionsRef.value.bodyExtra);
  const extensions = computed(() => optionsRef.value.extensions ?? {});

  const form = markRaw(useForm({
    schema: options.schema,
    initialState: options.initialState,
    controls: options.controls,
    validateOn: options.validateOn,
    genericErrorMessage: options.genericErrorMessage,
    submit: options.submit ?? { hidden: true },
    onSubmit: async (data) => {
      await optionsRef.value.onSubmit(data);
      if (optionsRef.value.closeOnSuccess !== false) {
        close();
      }
    },
  }));

  function handleClosed() {
    form.reset();
    optionsRef.value.onClose?.();
  }

  function open() {
    form.reset();
    optionsRef.value.onOpen?.();
    modal.open({ create: instance as CreateInstance }).then(handleClosed);
  }

  function close() {
    modal.close();
  }

  const instance: CreateInstance<TSchema> = {
    id,
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
  };

  return registerCreate(instance) as CreateInstance<TSchema>;
}
