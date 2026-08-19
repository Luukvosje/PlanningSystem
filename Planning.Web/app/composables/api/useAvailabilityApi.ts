import { useMutation, useQueryClient } from '@tanstack/vue-query';
import {
  deleteApiAvailabilityRulesId,
  postApiAvailabilityRules,
  putApiAvailabilityRulesId,
} from '~/generated/api/availability/availability';
import type { CreateAvailabilityRuleRequest, UpdateAvailabilityRuleRequest } from '~/types/availability';

export function useAvailabilityApi() {
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  function invalidateAvailability() {
    return queryClient.invalidateQueries({ queryKey: ['availability'] });
  }

  function warnIfConflicting(rule: { schedulingConflict?: boolean }) {
    if (rule.schedulingConflict) {
      toast.add({
        title: t('availability.conflictWarningTitle'),
        description: t('availability.conflictWarningDescription'),
        color: 'warning',
      });
    }
  }

  const create = useMutation({
    mutationFn: (request: CreateAvailabilityRuleRequest) => postApiAvailabilityRules(request),
    onSuccess: async (rule) => {
      await invalidateAvailability();
      toast.add({ title: t('availability.toast.created'), color: 'success' });
      warnIfConflicting(rule);
    },
    onError: () => {
      toast.add({ title: t('availability.toast.createFailed'), color: 'error' });
    },
  });

  const update = useMutation({
    mutationFn: ({ id, request }: { id: string, request: UpdateAvailabilityRuleRequest }) =>
      putApiAvailabilityRulesId(id, request),
    onSuccess: async (rule) => {
      await invalidateAvailability();
      toast.add({ title: t('availability.toast.updated'), color: 'success' });
      warnIfConflicting(rule);
    },
    onError: () => {
      toast.add({ title: t('availability.toast.updateFailed'), color: 'error' });
    },
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteApiAvailabilityRulesId(id),
    onSuccess: async () => {
      await invalidateAvailability();
      toast.add({ title: t('availability.toast.deleted'), color: 'success' });
    },
    onError: () => {
      toast.add({ title: t('availability.toast.deleteFailed'), color: 'error' });
    },
  });

  return {
    create,
    update,
    remove,
    invalidateAvailability,
  };
}
