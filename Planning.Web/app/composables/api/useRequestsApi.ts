import { useMutation, useQueryClient } from '@tanstack/vue-query';
import { postApiRequestsApprove, postApiRequestsReject } from '~/generated/api/requests/requests';
import type { DecideRequestsResponse, RequestRef } from '~/generated/models';

export function useRequestsApi() {
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  function invalidateRequests() {
    return Promise.all([
      queryClient.invalidateQueries({ queryKey: ['requests'] }),
      // A decision changes what counts against the planning, so every availability query that
      // fed a conflict warning is stale too.
      queryClient.invalidateQueries({ queryKey: ['availability'] }),
    ]);
  }

  /**
   * Skipped means the row was already decided by someone else while this list was open, so the
   * planner is told rather than left wondering why the count is off.
   */
  async function report(result: DecideRequestsResponse, key: 'approved' | 'rejected') {
    await invalidateRequests();

    toast.add({
      title: result.decided === 1 ?
        t(`requests.toast.${key}`) :
        t(`requests.toast.${key}Many`, { count: result.decided }),
      description: result.skipped > 0 ?
        t('requests.toast.skipped', { count: result.skipped }) :
        undefined,
      color: result.decided > 0 ? 'success' : 'warning',
    });
  }

  const approve = useMutation({
    mutationFn: (requests: RequestRef[]) => postApiRequestsApprove({ requests }),
    onSuccess: (result) => report(result, 'approved'),
    onError: () => {
      toast.add({ title: t('requests.toast.approveFailed'), color: 'error' });
    },
  });

  const reject = useMutation({
    mutationFn: (requests: RequestRef[]) => postApiRequestsReject({ requests }),
    onSuccess: (result) => report(result, 'rejected'),
    onError: () => {
      toast.add({ title: t('requests.toast.rejectFailed'), color: 'error' });
    },
  });

  return { approve, reject, invalidateRequests };
}
