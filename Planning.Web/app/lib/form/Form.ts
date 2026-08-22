import { computed, defineComponent, h, onScopeDispose, reactive, ref, type Component, type ComputedRef, type Ref } from 'vue';
import type { z } from 'zod';
import type { Form as UFormInstance, FormSubmitEvent } from '#ui/types';
import { isApiError } from '~/types/api-error';
import type { FormControl, FormSubmitConfig, MaybeRefOrGetter } from './control-types';
import { resolveMaybeRefOrGetter } from './control-types';
import { toFormErrors, type ServerErrorPayload } from './types';
import { translateBackendMessage } from '~/utils/backendMessages';
import { registerDirtyForm, unregisterDirtyForm } from './dirty-registry';
import FormView from '~/components/form/View.vue';
import FormFooterContent from '~/components/form/FooterContent.vue';

export interface FormClassOptions<TSchema extends z.ZodType> {
  schema: TSchema
  initialState: Partial<z.infer<TSchema>>
  onSubmit: (data: z.infer<TSchema>) => Promise<void> | void
  controls?: MaybeRefOrGetter<FormControl[]>
  submit?: MaybeRefOrGetter<FormSubmitConfig>
  /** Default: blur only (submit always validates via UForm). */
  validateOn?: Array<'blur' | 'input' | 'change'>
  genericErrorMessage?: string
  /**
   * Opts into the "settings-style" form: label-left/value-right grid layout, a
   * dirty-aware save/cancel footer (`FormFooter`), and route-leave-guard
   * registration. Default: false (vertical stack + simple submit button).
   */
  grid?: boolean
  /** Rendered by `form.render` above the controls loop. */
  header?: Component
  /** Extra content rendered by `form.render` below the default footer. */
  footer?: Component
  /**
   * Internal escape hatch: `useCreate`/`useEdit` set this to `false` so their
   * modal forms don't register for the route-leave guard (modals guard their
   * own close flow instead). Default: true.
   */
  trackNavigation?: boolean
}

export class Form<TSchema extends z.ZodType> {
  schema: TSchema;
  state: z.infer<TSchema>;
  validateOn: Array<'blur' | 'input' | 'change'>;
  isSubmitting: Ref<boolean> = ref(false);
  submitError: Ref<string | null> = ref(null);
  formRef: Ref<UFormInstance<z.infer<TSchema>> | null> = ref(null);
  controls: ComputedRef<FormControl[]>;
  submitConfig: ComputedRef<FormSubmitConfig>;
  grid: boolean;
  header: Component | undefined;
  footer: Component | undefined;
  isDirty: ComputedRef<boolean>;

  private readonly onSubmitFn: (data: z.infer<TSchema>) => Promise<void> | void;
  private readonly genericErrorMessage: string | undefined;
  private readonly t: ReturnType<typeof useI18n>['t'];
  private readonly locale: ReturnType<typeof useI18n>['locale'];
  private readonly initialState: Partial<z.infer<TSchema>>;
  private readonly controlsSource: MaybeRefOrGetter<FormControl[]>;
  private readonly submitSource: MaybeRefOrGetter<FormSubmitConfig>;
  private lastSavedSnapshot: Ref<string>;
  private lastSubmitSucceeded: boolean | null = null;
  private renderComponent: Component | undefined;
  private renderFooterComponent: Component | undefined;

  constructor(options: FormClassOptions<TSchema>) {
    this.schema = options.schema;
    this.initialState = options.initialState;
    this.state = reactive(structuredClone(options.initialState)) as z.infer<TSchema>;
    this.onSubmitFn = options.onSubmit;
    // One default for every form: blur covers text fields, change covers selects and checkboxes.
    // Validating on every keystroke ('input') was set on the customer forms only, so the same
    // kind of field behaved differently depending on which screen you were on.
    this.validateOn = options.validateOn ?? ['blur', 'change'];
    const i18n = useI18n();
    this.t = i18n.t;
    this.locale = i18n.locale;
    this.genericErrorMessage = options.genericErrorMessage;
    this.controlsSource = options.controls ?? [];
    this.submitSource = options.submit ?? computed(() => ({ label: this.t('common.actions.save') }));
    this.controls = computed(() => resolveMaybeRefOrGetter(this.controlsSource));
    this.submitConfig = computed(() => resolveMaybeRefOrGetter(this.submitSource));
    this.grid = options.grid ?? false;
    this.header = options.header;
    this.footer = options.footer;
    this.lastSavedSnapshot = ref(JSON.stringify(this.state));
    this.isDirty = computed(() => JSON.stringify(this.state) !== this.lastSavedSnapshot.value);

    if (this.grid && options.trackNavigation !== false) {
      registerDirtyForm(this);
      onScopeDispose(() => unregisterDirtyForm(this));
    }
  }

  markClean = () => {
    this.lastSavedSnapshot.value = JSON.stringify(this.state);
  };

  handleSubmit = async (event: FormSubmitEvent<z.infer<TSchema>>) => {
    this.submitError.value = null;
    this.isSubmitting.value = true;

    try {
      await this.onSubmitFn(event.data);
      this.lastSubmitSucceeded = true;
      this.markClean();
    } catch (err: unknown) {
      this.lastSubmitSucceeded = false;
      this.applyServerErrors(err);
    } finally {
      this.isSubmitting.value = false;
    }
  };

  /** Template ref binding for `<UForm>`; owned by the class so the view never writes to its prop. */
  bindFormRef = (instance: unknown) => {
    this.formRef.value = (instance ?? null) as UFormInstance<z.infer<TSchema>> | null;
  };

  applyServerErrors = (err: unknown) => {
    if (isApiError(err) && err.validationErrors) {
      this.formRef.value?.setErrors(this.translateFieldErrors(err.validationErrors));
      if (err.message && err.message !== this.t('errors.validationFailed')) {
        this.submitError.value = err.message;
      }
      return;
    }

    const statusCode = getErrorStatus(err);
    const data = getErrorData(err);

    if ((statusCode === 400 || statusCode === 422) && data?.errors) {
      this.formRef.value?.setErrors(this.translateFieldErrors(data.errors));
      return;
    }

    this.submitError.value = translateBackendMessage(data?.message, this.locale.value) ??
      translateBackendMessage(data?.title, this.locale.value) ??
      getErrorMessage(err) ??
      this.genericErrorMessage ??
      this.t('errors.genericFormError');
  };

  private translateFieldErrors = (errors: Record<string, string | string[]>) =>
    toFormErrors(errors).map((fieldError) => ({
      ...fieldError,
      message: translateBackendMessage(fieldError.message, this.locale.value) ?? fieldError.message,
    }));

  reset = (newState?: Partial<z.infer<TSchema>>) => {
    this.formRef.value?.clear();
    Object.assign(this.state as object, structuredClone(newState ?? this.initialState));
    this.submitError.value = null;
    this.markClean();
  };

  /** Reverts unsaved edits back to the last clean (constructed/reset/saved) snapshot. */
  discard = () => {
    this.formRef.value?.clear();
    Object.assign(this.state as object, JSON.parse(this.lastSavedSnapshot.value));
    this.submitError.value = null;
  };

  /** Resolves `true` only if the submit actually ran and `onSubmit` didn't throw. */
  submit = async (): Promise<boolean> => {
    this.lastSubmitSucceeded = null;
    await this.formRef.value?.submit();
    return this.lastSubmitSucceeded === true;
  };

  /**
   * A component with `form` already bound, for `<component :is="form.render">`.
   * Slots (including per-control `#control-${name}` overrides) pass through untouched.
   */
  get render(): Component {
    if (!this.renderComponent) {
      this.renderComponent = defineComponent({
        name: 'FormRender',
        inheritAttrs: false,
        setup: (_, { slots, attrs }) => () => h(FormView, { form: this, ...attrs }, slots),
      });
    }

    return this.renderComponent;
  }

  /**
   * A component rendering just the save/cancel footer (`FormFooter`) plus any
   * custom `footer` component, for placing into a parent `<template #footer>`
   * (e.g. `LayoutCard`'s footer slot) instead of inline below the fields.
   */
  get renderFooter(): Component {
    if (!this.renderFooterComponent) {
      this.renderFooterComponent = defineComponent({
        name: 'FormFooterRender',
        setup: () => () => h(FormFooterContent, { form: this }),
      });
    }

    return this.renderFooterComponent;
  }
}

export function useForm<TSchema extends z.ZodType>(options: FormClassOptions<TSchema>) {
  return new Form(options);
}

function getErrorStatus(err: unknown): number | undefined {
  if (typeof err !== 'object' || err === null) {
    return undefined;
  }

  const record = err as Record<string, unknown>;
  const status = record.status ?? record.statusCode;
  const response = record.response as { status?: number } | undefined;

  return typeof status === 'number' ?
    status :
    response?.status;
}

function getErrorData(err: unknown): ServerErrorPayload | undefined {
  if (typeof err !== 'object' || err === null) {
    return undefined;
  }

  const record = err as Record<string, unknown>;
  const data = record.data ?? (record.response as { _data?: ServerErrorPayload } | undefined)?._data;

  return typeof data === 'object' && data !== null ?
    data as ServerErrorPayload :
    undefined;
}

function getErrorMessage(err: unknown): string | undefined {
  if (err instanceof Error) {
    return err.message;
  }

  if (typeof err === 'object' && err !== null && 'message' in err) {
    const message = (err as { message: unknown }).message;
    return typeof message === 'string' ? message : undefined;
  }

  return undefined;
}
