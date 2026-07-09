import { z } from 'zod'

export const acceptInviteSchema = z.object({
  code: z
    .string()
    .min(1, 'Uitnodigingscode is verplicht')
    .max(16, 'Uitnodigingscode is te lang'),
})

export const createInviteSchema = z.object({})

export type AcceptInviteSchema = z.infer<typeof acceptInviteSchema>
export type CreateInviteSchema = z.infer<typeof createInviteSchema>
