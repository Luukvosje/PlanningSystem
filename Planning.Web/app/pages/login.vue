<script setup lang="ts">
import { createLoginCredentialsSchema, createSelectOrganizationSchema } from '~/schemas/auth.schema';
import type { OrganizationMembership } from '~/types/api-error';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();
const { t } = useI18n();

const loginCredentialsSchema = createLoginCredentialsSchema(t);
const selectOrganizationSchema = createSelectOrganizationSchema(t);

const memberships = ref<OrganizationMembership[]>([]);
const showOrgPicker = ref(false);

async function navigateAfterLogin() {
  const redirect = route.query.redirect as string | undefined;
  const inviteCode = route.query.code as string | undefined;
  const defaultPath = auth.hasOrganization ? '/dashboard' : (inviteCode ? '/join' : '/organizations/new');
  const target = redirect ?? defaultPath;
  await router.push(inviteCode && target === '/join' ? { path: target, query: { code: inviteCode } } : target);
}

const credentialsForm = useForm({
  schema: loginCredentialsSchema,
  initialState: { email: '', password: '' },
  controls: [
    {
      name: 'email',
      label: t('auth.email'),
      type: 'email',
      required: true,
      props: { autocomplete: 'email' },
    },
    {
      name: 'password',
      label: t('auth.password'),
      type: 'password',
      required: true,
      props: { autocomplete: 'current-password' },
    },
  ],
  submit: { label: t('auth.login'), block: true },
  onSubmit: async (data) => {
    const result = await auth.login(data);

    if (result.requiresOrganizationSelection && result.memberships?.length) {
      memberships.value = result.memberships;
      orgForm.reset({
        organizationId: result.memberships[0]?.organizationId ?? '',
      });
      showOrgPicker.value = true;
      return;
    }

    await auth.fetchMe();
    await navigateAfterLogin();
  },
});

const orgOptions = computed(() =>
  memberships.value.map((m) => ({
    label: m.organizationName ?? t('common.unknown'),
    value: m.organizationId,
  })),
);

const orgForm = useForm({
  schema: selectOrganizationSchema,
  initialState: { organizationId: '' },
  controls: computed(() => [
    {
      name: 'organizationId',
      label: t('nav.organization'),
      type: 'select',
      required: true,
      props: { items: orgOptions.value },
    },
  ]),
  submit: { label: t('auth.continue'), block: true },
  onSubmit: async (data) => {
    await auth.selectOrganization(data.organizationId);
    await auth.fetchMe();
    await navigateAfterLogin();
  },
});
</script>

<template>
	<UCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('auth.login') }}
			</h1>
		</template>

		<component
			:is="FormView"
			v-if="!showOrgPicker"
			:form="credentialsForm"
		/>

		<component
			:is="FormView"
			v-else
			:form="orgForm"
		/>

		<template #footer>
			<p class="text-sm text-muted text-center">
				{{ t('auth.noAccountYet') }}
				<NuxtLink
					to="/register"
					class="text-primary font-medium"
				>
					{{ t('auth.register') }}
				</NuxtLink>
			</p>
		</template>
	</UCard>
</template>
