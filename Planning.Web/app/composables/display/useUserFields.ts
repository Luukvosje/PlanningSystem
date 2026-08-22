import type { FormControl } from '~/lib/form/control-types';

/**
 * The employee fields that are read-only for an administrator: the API only lets you change your
 * own name and e-mail (profile endpoint). Shaped like an edit composable's `controls` so it can
 * move to `useUserEdit` unchanged once the API grows a `PUT /api/users/{id}`.
 */
export function useUserFields() {
  const { t } = useI18n();

  return computed<FormControl[]>(() => [
    { name: 'firstName', label: t('auth.firstName'), type: 'input' },
    { name: 'lastName', label: t('auth.lastName'), type: 'input' },
    { name: 'email', label: t('auth.email'), type: 'email' },
  ]);
}
