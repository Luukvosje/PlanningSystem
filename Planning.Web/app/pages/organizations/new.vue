<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';
import { createOrganizationSchema } from '~/schemas/organization.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const orgsApi = useOrganizationsApi();
const router = useRouter();
const queryClient = useQueryClient();
const toast = useToast();
const { t } = useI18n();

const organizationSchema = createOrganizationSchema(t);

const organizationForm = useForm({
  schema: organizationSchema,
  initialState: { name: '', email: '' },
  controls: [
    { name: 'name', label: t('organizations.fields.name'), type: 'input', required: true },
    { name: 'email', label: t('organizations.fields.email'), type: 'email', required: true },
  ],
  submit: { label: t('organizations.create.title'), block: true },
  onSubmit: async (data) => {
    const response = await orgsApi.create(data);

    if (response.organization?.id) {
      auth.setOrganizationId(response.organization.id);
    }
    await auth.fetchMe();
    await queryClient.invalidateQueries();

    toast.add({ title: t('organizations.created'), color: 'success' });
    await router.push('/users');
  },
});
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('organizations.create.title') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('organizations.create.firstOrgDescription') }}
			</p>
		</template>

		<component
			:is="FormView"
			:form="organizationForm"
		/>

		<template #footer>
			<AuthFooterLink
				:question="t('organizations.invited')"
				:link-label="t('invites.enterCode')"
				to="/join"
			/>
		</template>
	</LayoutCard>
</template>
