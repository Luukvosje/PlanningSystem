import { z } from 'zod';
import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

export function createAcceptInviteSchema(t: Translate) {
  return z.object({
    code: z
      .string()
      .min(1, t('validation.inviteCode.required'))
      .max(16, t('validation.inviteCode.tooLong')),
  });
}

export const createInviteSchema = z.object({});

export type AcceptInviteSchema = z.infer<ReturnType<typeof createAcceptInviteSchema>>
export type CreateInviteSchema = z.infer<typeof createInviteSchema>
