import type { ComputedRef, MaybeRefOrGetter } from 'vue';
import type { CustomerResponse, UserResponse } from '~/generated/models';

export type EntityKind = 'user' | 'customer'

export interface EntityOption {
  id: string
  label: string
  /** Second line in the list: `USelectMenu` renders it via `descriptionKey` and searches it too. */
  description: string | null
  isActive: boolean
}

export interface EntitySource {
  options: ComputedRef<EntityOption[]>
  isLoading: ComputedRef<boolean>
  /** False for entities the API has no active/inactive concept for - the switch stays hidden. */
  supportsActive: ComputedRef<boolean>
  icon: ComputedRef<string>
  placeholder: ComputedRef<string>
}

/**
 * Everything the app knows about an entity you can pick from a list: where it comes from, what it
 * is called, what you search in, and whether it can be inactive.
 *
 * The `{ label, value }` mapping behind a select used to be copy-pasted per call site - four
 * copies that had already drifted apart over whether inactive users belong in the list. Adding an
 * entity type is one branch here and nothing anywhere else.
 */
export function useEntitySource(kind: MaybeRefOrGetter<EntityKind>): EntitySource {
  const { t } = useI18n();

  // Both queries run unconditionally: composables cannot be called behind an `if`, and these are
  // the same app-wide cached vue-query entries the planning board already loads.
  const users = useUsers();
  const customers = useCustomers();

  const resolved = computed(() => toValue(kind));

  function toUserOption(user: UserResponse): EntityOption {
    return {
      id: user.id,
      label: `${user.firstName} ${user.lastName}`.trim() || user.email || t('common.unknown'),
      description: user.email ?? null,
      isActive: user.isActive,
    };
  }

  function toCustomerOption(customer: CustomerResponse): EntityOption {
    return {
      id: customer.id,
      label: customer.name || t('common.unknown'),
      description: customer.email || null,
      isActive: true,
    };
  }

  return {
    options: computed(() =>
      resolved.value === 'user' ?
        (users.data.value ?? []).map(toUserOption) :
        (customers.data.value ?? []).map(toCustomerOption)),
    isLoading: computed(() =>
      resolved.value === 'user' ? users.isLoading.value : customers.isLoading.value),
    supportsActive: computed(() => resolved.value === 'user'),
    icon: computed(() => resolved.value === 'user' ? 'i-lucide-user' : 'i-lucide-building-2'),
    placeholder: computed(() =>
      resolved.value === 'user' ? t('planning.fields.employee') : t('planning.fields.customer')),
  };
}
