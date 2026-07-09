import { computed, reactive, ref, type ComputedRef, type Ref } from 'vue'
import type { z } from 'zod'
import type { Form as UFormInstance, FormSubmitEvent } from '#ui/types'
import { isApiError } from '~/types/api-error'
import type { FormControl, FormSubmitConfig, MaybeRefOrGetter } from './control-types'
import { resolveMaybeRefOrGetter } from './control-types'
import { toFormErrors, type ServerErrorPayload } from './types'

export interface FormClassOptions<TSchema extends z.ZodType> {
  schema: TSchema
  initialState: Partial<z.infer<TSchema>>
  onSubmit: (data: z.infer<TSchema>) => Promise<void> | void
  controls?: MaybeRefOrGetter<FormControl[]>
  submit?: MaybeRefOrGetter<FormSubmitConfig>
  /** Default: blur only (submit always validates via UForm). */
  validateOn?: Array<'blur' | 'input' | 'change'>
  genericErrorMessage?: string
}

export class Form<TSchema extends z.ZodType> {
  schema: TSchema
  state: z.infer<TSchema>
  validateOn: Array<'blur' | 'input' | 'change'>
  isSubmitting: Ref<boolean> = ref(false)
  submitError: Ref<string | null> = ref(null)
  formRef: Ref<UFormInstance<z.infer<TSchema>> | null> = ref(null)
  controls: ComputedRef<FormControl[]>
  submitConfig: ComputedRef<FormSubmitConfig>

  private readonly onSubmitFn: (data: z.infer<TSchema>) => Promise<void> | void
  private readonly genericErrorMessage: string
  private readonly initialState: Partial<z.infer<TSchema>>
  private readonly controlsSource: MaybeRefOrGetter<FormControl[]>
  private readonly submitSource: MaybeRefOrGetter<FormSubmitConfig>

  constructor(options: FormClassOptions<TSchema>) {
    this.schema = options.schema
    this.initialState = options.initialState
    this.state = reactive(structuredClone(options.initialState)) as z.infer<TSchema>
    this.onSubmitFn = options.onSubmit
    this.validateOn = options.validateOn ?? ['blur']
    this.genericErrorMessage = options.genericErrorMessage
      ?? 'Er is iets misgegaan. Probeer het later opnieuw.'
    this.controlsSource = options.controls ?? []
    this.submitSource = options.submit ?? { label: 'Opslaan' }
    this.controls = computed(() => resolveMaybeRefOrGetter(this.controlsSource))
    this.submitConfig = computed(() => resolveMaybeRefOrGetter(this.submitSource))
  }

  handleSubmit = async (event: FormSubmitEvent<z.infer<TSchema>>) => {
    this.submitError.value = null
    this.isSubmitting.value = true

    try {
      await this.onSubmitFn(event.data)
    }
    catch (err: unknown) {
      this.applyServerErrors(err)
    }
    finally {
      this.isSubmitting.value = false
    }
  }

  applyServerErrors = (err: unknown) => {
    if (isApiError(err) && err.validationErrors) {
      this.formRef.value?.setErrors(toFormErrors(err.validationErrors))
      if (err.message && err.message !== 'Validatiefout') {
        this.submitError.value = err.message
      }
      return
    }

    const statusCode = getErrorStatus(err)
    const data = getErrorData(err)

    if ((statusCode === 400 || statusCode === 422) && data?.errors) {
      this.formRef.value?.setErrors(toFormErrors(data.errors))
      return
    }

    this.submitError.value = data?.message
      ?? data?.title
      ?? getErrorMessage(err)
      ?? this.genericErrorMessage
  }

  reset = (newState?: Partial<z.infer<TSchema>>) => {
    this.formRef.value?.clear()
    Object.assign(this.state as object, structuredClone(newState ?? this.initialState))
    this.submitError.value = null
  }

  submit = async () => {
    await this.formRef.value?.submit()
  }
}

export function useForm<TSchema extends z.ZodType>(options: FormClassOptions<TSchema>) {
  return new Form(options)
}

function getErrorStatus(err: unknown): number | undefined {
  if (typeof err !== 'object' || err === null) {
    return undefined
  }

  const record = err as Record<string, unknown>
  const status = record.status ?? record.statusCode
  const response = record.response as { status?: number } | undefined

  return typeof status === 'number'
    ? status
    : response?.status
}

function getErrorData(err: unknown): ServerErrorPayload | undefined {
  if (typeof err !== 'object' || err === null) {
    return undefined
  }

  const record = err as Record<string, unknown>
  const data = record.data ?? (record.response as { _data?: ServerErrorPayload } | undefined)?._data

  return typeof data === 'object' && data !== null
    ? data as ServerErrorPayload
    : undefined
}

function getErrorMessage(err: unknown): string | undefined {
  if (err instanceof Error) {
    return err.message
  }

  if (typeof err === 'object' && err !== null && 'message' in err) {
    const message = (err as { message: unknown }).message
    return typeof message === 'string' ? message : undefined
  }

  return undefined
}
