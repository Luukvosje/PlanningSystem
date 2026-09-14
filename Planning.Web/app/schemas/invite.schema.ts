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

export function createInviteSchema(t: Translate) {
  return z.object({
    // Optional: leaving it empty still produces a code to share by hand, which is how invites
    // worked before mail existed.
    email: z
      .string()
      .max(320, t('validation.email.tooLong'))
      .email(t('validation.email.invalid'))
      .optional()
      .or(z.literal('')),
    // Set when the invite is for a member who was added without a login; never typed by hand.
    userId: z.string().optional(),
  });
}

export type AcceptInviteSchema = z.infer<ReturnType<typeof createAcceptInviteSchema>>
export type CreateInviteSchema = z.infer<ReturnType<typeof createInviteSchema>>
