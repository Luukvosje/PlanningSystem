import { z } from 'zod';
import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

export function createCustomerSchema(t: Translate) {
  return z.object({
    name: z
      .string()
      .min(1, t('validation.name.required'))
      .max(200, t('validation.name.tooLong')),
    email: z
      .string()
      .min(1, t('validation.email.required'))
      .email(t('validation.email.invalid'))
      .max(320, t('validation.email.tooLong')),
    address: z
      .string()
      .max(500, t('validation.address.tooLong')),
  });
}

export type CustomerSchema = z.infer<ReturnType<typeof createCustomerSchema>>
