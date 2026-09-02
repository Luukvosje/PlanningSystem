import { z } from 'zod';
import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

export function createLoginCredentialsSchema(t: Translate) {
  return z.object({
    email: z
      .string()
      .min(1, t('validation.email.required'))
      .email(t('validation.email.invalid'))
      .max(320, t('validation.email.tooLong')),
    password: z
      .string()
      .min(1, t('validation.password.required'))
      .max(128, t('validation.password.tooLong')),
  });
}

export function createSelectOrganizationSchema(t: Translate) {
  return z.object({
    organizationId: z.string().min(1, t('validation.organization.required')),
  });
}

export function createRegisterSchema(t: Translate) {
  return z.object({
    firstName: z
      .string()
      .min(1, t('validation.firstName.required'))
      .max(100, t('validation.firstName.tooLong')),
    lastName: z
      .string()
      .min(1, t('validation.lastName.required'))
      .max(100, t('validation.lastName.tooLong')),
    email: z
      .string()
      .min(1, t('validation.email.required'))
      .email(t('validation.email.invalid'))
      .max(320, t('validation.email.tooLong')),
    password: z
      .string()
      .min(8, t('validation.password.minLength'))
      .max(128, t('validation.password.tooLong')),
  });
}

export function createForgotPasswordSchema(t: Translate) {
  return z.object({
    email: z
      .string()
      .min(1, t('validation.email.required'))
      .email(t('validation.email.invalid'))
      .max(320, t('validation.email.tooLong')),
  });
}

export function createResetPasswordSchema(t: Translate) {
  return z
    .object({
      password: z
        .string()
        .min(8, t('validation.password.minLength'))
        .max(128, t('validation.password.tooLong')),
      confirmPassword: z.string().min(1, t('validation.password.required')),
    })
    // Confirmation is a client-side courtesy only - the API takes a single password. It exists
    // because a typo here locks you out of the account you were trying to recover.
    .refine((data) => data.password === data.confirmPassword, {
      message: t('validation.password.mismatch'),
      path: ['confirmPassword'],
    });
}

export type LoginCredentialsSchema = z.infer<ReturnType<typeof createLoginCredentialsSchema>>
export type SelectOrganizationSchema = z.infer<ReturnType<typeof createSelectOrganizationSchema>>
export type RegisterSchema = z.infer<ReturnType<typeof createRegisterSchema>>
export type ForgotPasswordSchema = z.infer<ReturnType<typeof createForgotPasswordSchema>>
export type ResetPasswordSchema = z.infer<ReturnType<typeof createResetPasswordSchema>>

export function createUpdateProfileSchema(t: Translate) {
  return z.object({
    firstName: z
      .string()
      .min(1, t('validation.firstName.required'))
      .max(100, t('validation.firstName.tooLong')),
    lastName: z
      .string()
      .min(1, t('validation.lastName.required'))
      .max(100, t('validation.lastName.tooLong')),
    email: z
      .string()
      .min(1, t('validation.email.required'))
      .email(t('validation.email.invalid'))
      .max(320, t('validation.email.tooLong')),
  });
}

export type UpdateProfileSchema = z.infer<ReturnType<typeof createUpdateProfileSchema>>
