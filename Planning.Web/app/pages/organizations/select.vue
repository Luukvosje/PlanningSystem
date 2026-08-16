<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';
import { createSelectOrganizationSchema } from '~/schemas/auth.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();
const queryClient = useQueryClient();
const { t } = useI18n();

const { data: memberships, isLoading } = useMyOrganizations();

const selectOrganizationSchema = createSelectOrganizationSchema(t);

const orgOptions = computed(() =>
  (memberships.value ?? []).map((m) => ({
    label: m.organizationName ?? t('common.unknown'),
    value: m.organizationId ?? '',
  })),
);

function redirectTarget() {
  const redirect = route.query.redirect as string | undefined;
  return redirect ?? '/dashboard';
}

async function selectAndContinue(organizationId: string) {
  auth.selectOrganization(organizationId);
  await auth.fetchMe();
  await queryClient.invalidateQueries();
  await router.push(redirectTarget());
}

// Defends against landing here directly (e.g. a stale bookmark) once
// memberships have actually loaded: 0 memberships means there's nothing to
// choose from, 1 means there's nothing to choose either.
watch(memberships, async (list) => {
  if (!list) {
    return;
  }

  if (list.length === 0) {
    await router.replace('/organizations/new');
  } else if (list.length === 1 && list[0]?.organizationId) {
    await selectAndContinue(list[0].organizationId);
  }
}, { immediate: true });

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
    await selectAndContinue(data.organizationId);
  },
});
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('organizations.select.title') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('organizations.select.description') }}
			</p>
		</template>

		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('organizations.loading')"
		/>

		<component
			:is="FormView"
			v-else-if="orgOptions.length > 1"
			:form="orgForm"
		/>
	</LayoutCard>
</template>
