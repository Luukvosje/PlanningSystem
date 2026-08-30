import { z } from 'zod';
import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

/** Mirrors PlanningRequestValidator on the API; the server stays the actual boundary. */
export function createPlanningRecordSchema(t: Translate) {
  return z.object({
    title: z
      .string()
      .trim()
      .min(1, t('validation.title.required'))
      .max(200, t('validation.title.tooLong')),
    description: z
      .string()
      .max(4000, t('validation.description.tooLong')),
    notes: z
      .string()
      .max(4000, t('validation.notes.tooLong')),
    assignedUserId: z.string(),
    customerId: z.string().nullable(),
    status: z.enum(['Planned', 'Confirmed', 'Completed', 'Cancelled']),
    color: z.string(),
    startUtc: z.string().min(1),
    endUtc: z.string().min(1),
  }).check((ctx) => {
    // Reported on endUtc so the message lands on the period field instead of coming back
    // as a server error after a pointless round trip.
    if (new Date(ctx.value.endUtc).getTime() <= new Date(ctx.value.startUtc).getTime()) {
      ctx.issues.push({
        code: 'custom',
        message: t('dateTimeRange.endAfterStart'),
        path: ['endUtc'],
        input: ctx.value,
      });
    }
  });
}

export type PlanningRecordSchema = z.infer<ReturnType<typeof createPlanningRecordSchema>>
