import { z } from 'zod'

export const customerSchema = z.object({
  name: z
    .string()
    .min(1, 'Naam is verplicht')
    .max(200, 'Naam is te lang'),
  email: z
    .string()
    .min(1, 'E-mail is verplicht')
    .email('Ongeldig e-mailadres')
    .max(320, 'E-mailadres is te lang'),
  address: z
    .string()
    .max(500, 'Adres is te lang'),
})

export type CustomerSchema = z.infer<typeof customerSchema>
