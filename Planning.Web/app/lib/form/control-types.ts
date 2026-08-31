import type { Component, ComputedRef, Ref } from 'vue';
import { toValue } from 'vue';
import type { z } from 'zod';
import type { Form } from './Form';

export type BuiltInControlType = 'input' | 'email' | 'password' | 'textarea' | 'select' | 'switch'

export interface ControlRenderContext<TSchema extends z.ZodType = z.ZodType> {
  form: Form<TSchema>
  control: FormControl
}

export interface BaseFormControl {
  name: string
  label: string
  required?: boolean
  hidden?: boolean
  description?: string
  /** Extra props passed to UFormField */
  fieldProps?: Record<string, unknown>
  /**
   * Overrides how the value reads in a read-only view (`FormDisplay`). Only needed when the raw
   * value isn't presentable on its own — dates, amounts, enums. Ignored by `FormView`.
   */
  display?: (value: unknown) => string | null
}

export interface BuiltInFormControl extends BaseFormControl {
  type: BuiltInControlType
  props?: Record<string, unknown> | ((ctx: ControlRenderContext) => Record<string, unknown>)
}

export interface CustomFormControl extends BaseFormControl {
  component: Component
  props?: Record<string, unknown> | ((ctx: ControlRenderContext) => Record<string, unknown>)
}

export type FormControl = BuiltInFormControl | CustomFormControl

export function isBuiltInControl(control: FormControl): control is BuiltInFormControl {
  return 'type' in control;
}

export function isCustomControl(control: FormControl): control is CustomFormControl {
  return 'component' in control;
}

export interface FormSubmitConfig {
  label?: string
  block?: boolean
  hidden?: boolean
  props?: Record<string, unknown>
}

export type MaybeRefOrGetter<T> = T | Ref<T> | ComputedRef<T>

export function resolveMaybeRefOrGetter<T>(source: MaybeRefOrGetter<T>): T {
  return toValue(source);
}

export function resolveControlProps(
  control: FormControl,
  ctx: ControlRenderContext,
): Record<string, unknown> {
  if (!control.props) {
    return {};
  }
  return typeof control.props === 'function' ?
    control.props(ctx) :
    control.props;
}
