<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';
import { organizationSchema } from '~/schemas/organization.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const orgsApi = useOrganizationsApi();
const router = useRouter();
const queryClient = useQueryClient();
const toast = useToast();

const organizationForm = useForm({
  schema: organizationSchema,
  initialState: { name: '', email: '' },
  controls: [
    { name: 'name', label: 'Naam', type: 'input', required: true },
    { name: 'email', label: 'E-mail organisatie', type: 'email', required: true },
  ],
  submit: { label: 'Organisatie aanmaken', block: true },
  onSubmit: async (data) => {
    const response = await orgsApi.create(data);

    if (response.organization?.id) {
      auth.setOrganizationId(response.organization.id);
    }
    await auth.fetchMe();
    await queryClient.invalidateQueries();

    toast.add({ title: 'Organisatie aangemaakt', color: 'success' });
    await router.push('/users');
  },
});
</script>

<template>
	<UCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				Organisatie aanmaken
			</h1>
			<p class="text-sm text-muted mt-1">
				Maak je eerste organisatie aan om te beginnen met plannen.
			</p>
		</template>

		<component
			:is="FormView"
			:form="organizationForm"
		/>

		<template #footer>
			<p class="text-sm text-muted text-center">
				Uitgenodigd?
				<NuxtLink
					to="/join"
					class="text-primary font-medium"
				>
					Code invullen
				</NuxtLink>
			</p>
		</template>
	</UCard>
</template>
