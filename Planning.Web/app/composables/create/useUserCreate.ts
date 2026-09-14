import { useQueryClient } from '@tanstack/vue-query';
import { useCreate } from '~/lib/form/useCreate';
import { createCreateUserSchema } from '~/schemas/user.schema';
import { queryKeys } from '~/utils/queryKeys';

const USER_CREATE_KEY = Symbol('user-create');

/**
 * Adding a team member by name, before they have a login. They can be scheduled straight away;
 * the invite that links their account is a separate step from the member's page or the list.
 */
export function useUserCreate() {
  const usersApi = useUsersApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const router = useRouter();
  const { t } = useI18n();

  return useCreate(USER_CREATE_KEY, {
    title: computed(() => t('users.create.title')),
    description: computed(() => t('users.create.description')),
    schema: createCreateUserSchema(t),
    initialState: { firstName: '', lastName: '', email: '' },
    controls: computed(() => [
      { name: 'firstName', label: t('auth.firstName'), type: 'input', required: true, props: { autocomplete: 'off' } },
      { name: 'lastName', label: t('auth.lastName'), type: 'input', required: true, props: { autocomplete: 'off' } },
      {
        name: 'email',
        label: t('users.create.emailLabel'),
        type: 'email',
        description: t('users.create.emailHint'),
        props: { autocomplete: 'off' },
      },
    ]),
    submitLabel: computed(() => t('users.create.submit')),
    onSubmit: async (data) => {
      const user = await usersApi.create({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email?.trim() ? data.email.trim() : null,
      });

      await queryClient.invalidateQueries({ queryKey: queryKeys.users.all });

      toast.add({ title: t('users.created'), color: 'success' });

      if (user.id) {
        await router.push(`/users/${user.id}`);
      }
    },
  });
}
