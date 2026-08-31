import { useMutation, useQueryClient } from '@tanstack/vue-query';
import {
  deleteApiAvailabilityRulesId,
  postApiAvailabilityRules,
  putApiAvailabilityRulesId,
} from '~/generated/api/availability/availability';
import { ApprovalStatus } from '~/generated/models';
import type { AvailabilityRule, CreateAvailabilityRuleRequest, UpdateAvailabilityRuleRequest } from '~/types/availability';

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

  /**
   * A member who has to request their availability gets a pending rule back - on a change as well,
   * because the earlier approval was about what the rule used to say. Saying "saved" there would
   * claim more than happened.
   */
  function reportSaved(rule: AvailabilityRule, savedKey: 'created' | 'updated') {
    toast.add({
      title: rule.approvalStatus === ApprovalStatus.Pending ?
        t('availability.toast.submitted') :
        t(`availability.toast.${savedKey}`),
      color: 'success',
    });
  }

  const create = useMutation({
    mutationFn: (request: CreateAvailabilityRuleRequest) => postApiAvailabilityRules(request),
    onSuccess: async (rule) => {
      await invalidateAvailability();
      reportSaved(rule, 'created');
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
      reportSaved(rule, 'updated');
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
