import { z } from 'zod';
import { AppModule, UserRole } from '~/generated/models';

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
