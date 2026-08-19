import { useQuery } from '@tanstack/vue-query';
import { getApiAvailabilityRules } from '~/generated/api/availability/availability';
import { queryKeys } from '~/utils/queryKeys';

export function useAvailabilityRules(options: {
  employeeId: Ref<string | null | undefined>
  enabled?: Ref<boolean>
}) {
  const auth = useAuthStore();

  return useQuery({
    queryKey: computed(() => queryKeys.availability.rules(options.employeeId.value ?? undefined)),
    queryFn: () => getApiAvailabilityRules({ employeeId: options.employeeId.value! }),
    enabled: computed(() =>
      (options.enabled?.value ?? true) &&
      !!options.employeeId.value &&
      auth.isAuthenticated &&
      auth.hasOrganization,
    ),
  });
}
