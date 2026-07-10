import { z } from 'zod'

export const loginCredentialsSchema = z.object({
  email: z
    .string()
    .min(1, 'E-mail is verplicht')
    .email('Ongeldig e-mailadres')
    .max(320, 'E-mailadres is te lang'),
  password: z
    .string()
    .min(1, 'Wachtwoord is verplicht')
    .max(128, 'Wachtwoord is te lang'),
})

export const selectOrganizationSchema = z.object({
  organizationId: z.string().min(1, 'Selecteer een organisatie'),
})

export const registerSchema = z.object({
  firstName: z
    .string()
    .min(1, 'Voornaam is verplicht')
    .max(100, 'Voornaam is te lang'),
  lastName: z
    .string()
    .min(1, 'Achternaam is verplicht')
    .max(100, 'Achternaam is te lang'),
  email: z
    .string()
    .min(1, 'E-mail is verplicht')
    .email('Ongeldig e-mailadres')
    .max(320, 'E-mailadres is te lang'),
  password: z
    .string()
    .min(8, 'Minimaal 8 tekens')
    .max(128, 'Wachtwoord is te lang'),
})

export type LoginCredentialsSchema = z.infer<typeof loginCredentialsSchema>
export type SelectOrganizationSchema = z.infer<typeof selectOrganizationSchema>
export type RegisterSchema = z.infer<typeof registerSchema>

export const updateProfileSchema = z.object({
  firstName: z
    .string()
    .min(1, 'Voornaam is verplicht')
    .max(100, 'Voornaam is te lang'),
  lastName: z
    .string()
    .min(1, 'Achternaam is verplicht')
    .max(100, 'Achternaam is te lang'),
  email: z
    .string()
    .min(1, 'E-mail is verplicht')
    .email('Ongeldig e-mailadres')
    .max(320, 'E-mailadres is te lang'),
})

export type UpdateProfileSchema = z.infer<typeof updateProfileSchema>
