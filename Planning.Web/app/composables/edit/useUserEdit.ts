import { useQueryClient } from '@tanstack/vue-query';
import ModulesToggles from '~/components/modules/Toggles.vue';
import type { AppModule, UserResponse } from '~/generated/models';
import { UserRole } from '~/generated/models';
import type { FormControl } from '~/lib/form/control-types';
import type { EditInstance } from '~/lib/form/useEdit';
import { useEdit } from '~/lib/form/useEdit';
import type { UpdateUserSchema } from '~/schemas/user.schema';
import { createUpdateUserSchema } from '~/schemas/user.schema';
import {
  ALL_MODULES,
  getModuleLabel,
  getUserModuleToggleStates,
  modulesFromSettings,
  modulesToRequest,
  resolveModuleSelection,
} from '~/utils/modules';
import { queryKeys } from '~/utils/queryKeys';

const USER_EDIT_KEY = Symbol('user-edit');

type UserEdit = EditInstance<ReturnType<typeof createUpdateUserSchema>, UserResponse>

/**
 * What an administrator may change about a team member: role and modules. Both sit behind their own
 * endpoint, so one save writes only the part that actually changed. Name and e-mail are not here -
 * the API only exposes those through the profile endpoint.
 */
export function useUserEdit(): UserEdit {
  const auth = useAuthStore();
  const usersApi = useUsersApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();
  const { data: organization } = useCurrentOrganization();

  function toggleStatesFor(user: UserResponse | null, role?: UserRole) {
    return getUserModuleToggleStates(
      role ?? user?.role,
      user?.modules,
      organization.value?.modules,
      t,
    );
  }

  function moduleSummary(user: UserResponse | null, value: unknown) {
    const toggleStates = toggleStatesFor(user);
    const selection = resolveModuleSelection(toggleStates, (value ?? {}) as Record<AppModule, boolean>);

    const labels = ALL_MODULES
      .filter((module) => toggleStates[module].visible && selection[module])
      .map((module) => getModuleLabel(module, t));

    return labels.length ? labels.join(', ') : null;
  }

  // Annotated because the controls getter reads `edit.entity` - by the time it runs, the assignment
  // below has happened.
  const edit: UserEdit = useEdit(USER_EDIT_KEY, {
    schema: createUpdateUserSchema(),
    controls: computed<FormControl[]>(() => {
      const user = edit.entity.value;
      // The role being edited, so the modules follow the select before you save rather than after.
      const role = (edit.form.state as UpdateUserSchema).role ?? user?.role;

      return [
        {
          name: 'role',
          label: t('users.fields.role'),
          type: 'select',
          required: true,
          props: {
            items: getAssignableRoleOptions(t, auth.currentUser?.userId, {
              id: user?.id,
              role: user?.role,
            }),
          },
          display: (value) => getRoleLabel(value as UserRole, t),
        },
        {
          name: 'modules',
          label: t('users.fields.modules'),
          description: t('users.modules.description'),
          component: ModulesToggles,
          // Only the roles whose access modules actually decide: an owner or admin always has
          // every module, so three forced switches would say nothing.
          hidden: role !== UserRole.Planner && role !== UserRole.Employee,
          props: ({ form }) => ({
            toggleStates: toggleStatesFor(user, (form.state as UpdateUserSchema).role),
          }),
          display: (value) => moduleSummary(user, value),
        },
      ];
    }),
    toState: (user: UserResponse) => ({
      role: user.role,
      modules: modulesFromSettings(user.modules),
    }),
    onSubmit: async (user, data) => {
      const toggleStates = toggleStatesFor(user, data.role);
      const modules = resolveModuleSelection(toggleStates, data.modules);
      const saved = modulesFromSettings(user.modules);

      if (data.role !== user.role) {
        await usersApi.updateRole(user.id, { role: data.role });
      }

      if (ALL_MODULES.some((module) => modules[module] !== saved[module])) {
        await usersApi.updateModules(user.id, modulesToRequest(modules));
      }

      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.users.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(user.id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.auth.me }),
      ]);

      if (user.id === auth.currentUser?.userId) {
        await auth.fetchMe();
      }

      toast.add({ title: t('users.updated'), color: 'success' });
    },
  });

  return edit;
}
