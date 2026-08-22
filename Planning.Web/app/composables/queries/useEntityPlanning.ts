import { useQuery } from '@tanstack/vue-query';
import { getApiPlanning } from '~/generated/api/planning/planning';
import { PLANNING_PAGE_SIZE } from '~/utils/planning/constants';
import { queryKeys } from '~/utils/queryKeys';

const LOOKBEHIND_DAYS = 30;
const LOOKAHEAD_DAYS = 90;

export interface EntityPlanningTarget {
  kind: 'customer' | 'user'
  id: Ref<string | null | undefined>
}

/**
 * The shifts of one customer or one employee, for the Planning tab on their detail page.
 *
 * A fixed window (a month back, a quarter ahead) rather than the whole history: it is the same
 * shape `useNextPlanningShift` uses, and it keeps one page of results enough.
 */
export function useEntityPlanning(target: EntityPlanningTarget) {
  const auth = useAuthStore();

  const startUtc = computed(() => {
    const start = new Date();
    start.setDate(start.getDate() - LOOKBEHIND_DAYS);
    return start.toISOString();
  });

  const endUtc = computed(() => {
    const end = new Date();
    end.setDate(end.getDate() + LOOKAHEAD_DAYS);
    return end.toISOString();
  });

  const query = useQuery({
    queryKey: computed(() =>
      queryKeys.planning.forEntity(target.kind, target.id.value ?? '', startUtc.value, endUtc.value)),
    queryFn: () =>
      getApiPlanning({
        StartUtc: startUtc.value,
        EndUtc: endUtc.value,
        CustomerIds: target.kind === 'customer' ? target.id.value ?? undefined : undefined,
        UserIds: target.kind === 'user' ? target.id.value ?? undefined : undefined,
        PageSize: PLANNING_PAGE_SIZE,
      }),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization && !!target.id.value),
  });

  const shifts = computed(() =>
    [...(query.data.value?.items ?? [])]
      .sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime()));

  return { ...query, shifts };
}
