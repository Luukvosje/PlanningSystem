<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';
import InvitesCodeInput from '~/components/invites/CodeInput.vue';
import { acceptInviteSchema } from '~/schemas/invite.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const invitesApi = useInvitesApi();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const queryClient = useQueryClient();

const initialCode = ((route.query.code as string) ?? '').toUpperCase();

const joinForm = useForm({
  schema: acceptInviteSchema,
  initialState: { code: initialCode },
  controls: [
    {
      name: 'code',
      label: 'Uitnodigingscode',
      required: true,
      component: InvitesCodeInput,
    },
  ],
  submit: computed(() => ({
    label: auth.isAuthenticated ? 'Deelnemen' : 'Inloggen om deel te nemen',
    block: true,
  })),
  onSubmit: async (data) => {
    if (!auth.isAuthenticated) {
      await router.push({ path: '/login', query: { redirect: '/join', code: data.code } });
      return;
    }

    const response = await invitesApi.accept({ code: data.code.toUpperCase() });
    auth.setOrganizationId(response.organizationId);
    await auth.fetchMe();
    await queryClient.invalidateQueries();

    toast.add({ title: 'Uitnodiging geaccepteerd', color: 'success' });
    await router.push('/users');
  },
});

const canPreview = computed(() => auth.isAuthenticated && joinForm.state.code.length >= 4);
const previewCode = computed(() => canPreview.value ? joinForm.state.code : '');
const { data: preview, isLoading: previewLoading } = useInvitePreview(previewCode);
</script>

<template>
	<UCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				Uitnodiging accepteren
			</h1>
			<p class="text-sm text-muted mt-1">
				Vul de code in die je van je organisator hebt ontvangen.
			</p>
		</template>

		<component
			:is="FormView"
			:form="joinForm"
		>
			<template #after>
				<div v-if="auth.isAuthenticated && joinForm.state.code.length >= 4">
					<div
						v-if="previewLoading"
						class="py-2"
					>
						<UiLoadingIndicator
							label="Code controleren..."
							size="sm"
						/>
					</div>
					<UAlert
						v-else-if="preview"
						:color="preview.isValid ? 'info' : 'warning'"
						variant="subtle"
						:title="preview.organizationName ?? 'Organisatie'"
						:description="preview.isValid
							? 'Je wordt toegevoegd aan deze organisatie als medewerker'
							: 'Deze code is verlopen of al gebruikt'"
					/>
				</div>
			</template>
		</component>

		<template #footer>
			<p class="text-sm text-muted text-center">
				Nog geen account?
				<NuxtLink
					:to="{ path: '/register', query: { redirect: '/join', code: joinForm.state.code } }"
					class="text-primary font-medium"
				>
					Registreren
				</NuxtLink>
			</p>
		</template>
	</UCard>
</template>
