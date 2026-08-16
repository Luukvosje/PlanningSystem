import type { z } from 'zod';
import type { Form } from './Form';

const dirtyForms = new Set<Form<z.ZodType>>();

export function registerDirtyForm(form: Form<z.ZodType>) {
  dirtyForms.add(form);
}

export function unregisterDirtyForm(form: Form<z.ZodType>) {
  dirtyForms.delete(form);
}

export function getDirtyForms(): Form<z.ZodType>[] {
  return Array.from(dirtyForms).filter((form) => form.isDirty.value);
}
