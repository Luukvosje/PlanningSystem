<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';
import InvitesCodeInput from '~/components/invites/CodeInput.vue';
import { createAcceptInviteSchema } from '~/schemas/invite.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const invitesApi = useInvitesApi();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const queryClient = useQueryClient();
const { t } = useI18n();

const acceptInviteSchema = createAcceptInviteSchema(t);
const initialCode = ((route.query.code as string) ?? '').toUpperCase();

const joinForm = useForm({
  schema: acceptInviteSchema,
  initialState: { code: initialCode },
  controls: [
    {
      name: 'code',
      label: t('invites.code'),
      required: true,
      component: InvitesCodeInput,
    },
  ],
  submit: computed(() => ({
    label: auth.isAuthenticated ? t('invites.join') : t('invites.loginToJoin'),
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

    toast.add({ title: t('invites.accepted'), color: 'success' });
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
				{{ t('invites.acceptTitle') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('invites.acceptDescription') }}
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
							:label="t('invites.checkingCode')"
							size="sm"
						/>
					</div>
					<UAlert
						v-else-if="preview"
						:color="preview.isValid ? 'info' : 'warning'"
						variant="subtle"
						:title="preview.organizationName ?? t('nav.organization')"
						:description="preview.isValid
							? t('invites.previewValid')
							: t('invites.previewInvalid')"
					/>
				</div>
			</template>
		</component>

		<template #footer>
			<p class="text-sm text-muted text-center">
				{{ t('auth.noAccountYet') }}
				<NuxtLink
					:to="{ path: '/register', query: { redirect: '/join', code: joinForm.state.code } }"
					class="text-primary font-medium"
				>
					{{ t('auth.register') }}
				</NuxtLink>
			</p>
		</template>
	</UCard>
</template>
