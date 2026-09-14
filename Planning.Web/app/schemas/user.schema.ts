import { z } from 'zod';
import type { Composer } from 'vue-i18n';
import { AppModule, UserRole } from '~/generated/models';

type Translate = Composer['t']

/** A member added by name, before they have a login. Limits mirror CreateUserRequestValidator. */
export function createCreateUserSchema(t: Translate) {
  return z.object({
    firstName: z.string().min(1, t('validation.firstName.required')).max(100, t('validation.firstName.tooLong')),
    lastName: z.string().min(1, t('validation.lastName.required')).max(100, t('validation.lastName.tooLong')),
    email: z
      .string()
      .max(320, t('validation.email.tooLong'))
      .email(t('validation.email.invalid'))
      .optional()
      .or(z.literal('')),
  });
}

export type CreateUserSchema = z.infer<ReturnType<typeof createCreateUserSchema>>

/**
 * What an administrator may change about a team member. Name and e-mail are not in here: the API
 * only exposes those through the profile endpoint, so they stay read-only on the detail page.
 */
export function createUpdateUserSchema() {
  return z.object({
    role: z.enum(UserRole),
    requiresApproval: z.boolean(),
    modules: z.object({
      [AppModule.Planning]: z.boolean(),
      [AppModule.Klant]: z.boolean(),
      [AppModule.Beheer]: z.boolean(),
    }),
  });
}

export type UpdateUserSchema = z.infer<ReturnType<typeof createUpdateUserSchema>>
