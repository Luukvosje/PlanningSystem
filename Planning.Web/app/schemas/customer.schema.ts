import { z } from 'zod';
import type { Composer } from 'vue-i18n';
import { HEX_COLOR_PATTERN } from '~/types/planning';

type Translate = Composer['t']

export function createCustomerSchema(t: Translate) {
  return z.object({
    name: z
      .string()
      .min(1, t('validation.name.required'))
      .max(200, t('validation.name.tooLong')),
    email: z
      .string()
      .max(320, t('validation.email.tooLong'))
      .email(t('validation.email.invalid'))
      .optional()
      .or(z.literal('')),
    address: z
      .string()
      .max(500, t('validation.address.tooLong')),
    color: z
      .string()
      .regex(HEX_COLOR_PATTERN, t('validation.color.invalid')),
  });
}

export type CustomerSchema = z.infer<ReturnType<typeof createCustomerSchema>>
