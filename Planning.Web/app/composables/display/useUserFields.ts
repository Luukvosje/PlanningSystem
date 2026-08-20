import type { FormControl } from '~/lib/form/control-types';

/**
 * The employee fields for a read-only view. Shaped like an edit composable's `controls` so it can
 * swap over to `useEdit` unchanged once the API grows a `PUT /api/users/{id}` — today it only
 * exposes role and modules, so there is nothing to edit here yet.
 */
export function useUserFields() {
  const { t } = useI18n();

  return computed<FormControl[]>(() => [
    { name: 'firstName', label: t('auth.firstName'), type: 'input' },
    { name: 'lastName', label: t('auth.lastName'), type: 'input' },
    { name: 'email', label: t('auth.email'), type: 'email' },
  ]);
}
