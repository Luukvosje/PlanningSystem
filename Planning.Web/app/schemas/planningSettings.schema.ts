import { z } from 'zod';
import type { Composer } from 'vue-i18n';

type Translate = Composer['t']

const TIME_PATTERN = /^([01]\d|2[0-3]):[0-5]\d$/;

export function createPlanningSettingsSchema(t: Translate) {
  return z.object({
    importantWorkTimes: z.array(
      z.object({
        label: z.string(),
        startTime: z.string().regex(TIME_PATTERN, t('validation.time.invalid')),
      }),
    ),
    openingHours: z.array(
      z.object({
        day: z.enum(['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']),
        enabled: z.boolean(),
        openTime: z.string().regex(TIME_PATTERN, t('validation.time.invalid')),
        closeTime: z.string().regex(TIME_PATTERN, t('validation.time.invalid')),
      }),
    ),
  });
}

export type PlanningSettingsSchema = z.infer<ReturnType<typeof createPlanningSettingsSchema>>
