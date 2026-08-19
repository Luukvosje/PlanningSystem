<script setup lang="ts">
import { createRegisterSchema } from '~/schemas/auth.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();
const { t } = useI18n();

const registerSchema = createRegisterSchema(t);

const registerForm = useForm({
  schema: registerSchema,
  initialState: {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
  },
  controls: [
    {
      name: 'firstName',
      label: t('auth.firstName'),
      type: 'input',
      required: true,
      props: { autocomplete: 'given-name' },
    },
    {
      name: 'lastName',
      label: t('auth.lastName'),
      type: 'input',
      required: true,
      hidden: true,
      props: { autocomplete: 'family-name' },
    },
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
      props: { autocomplete: 'new-password' },
    },
  ],
  submit: { label: t('auth.register'), block: true },
  onSubmit: async (data) => {
    await auth.register(data);

    toast.add({ title: t('auth.accountCreated'), description: t('auth.accountCreatedDescription'), color: 'success' });

    const redirect = route.query.redirect as string | undefined;
    const inviteCode = route.query.code as string | undefined;
    await router.push({
      path: '/login',
      query: {
        ...(redirect ? { redirect } : {}),
        ...(inviteCode ? { code: inviteCode } : {}),
      },
    });
  },
});
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('auth.createAccount') }}
			</h1>
		</template>

		<component
			:is="FormView"
			:form="registerForm"
		>
			<template #control-firstName="{ form }">
				<div class="grid grid-cols-2 gap-4">
					<UFormField
						:label="t('auth.firstName')"
						name="firstName"
						required
					>
						<UInput
							v-model="form.state.firstName"
							autocomplete="given-name"
							class="w-full"
							:disabled="form.isSubmitting.value"
						/>
					</UFormField>
					<UFormField
						:label="t('auth.lastName')"
						name="lastName"
						required
					>
						<UInput
							v-model="form.state.lastName"
							autocomplete="family-name"
							class="w-full"
							:disabled="form.isSubmitting.value"
						/>
					</UFormField>
				</div>
			</template>
		</component>

		<template #footer>
			<AuthFooterLink
				:question="t('auth.alreadyHaveAccount')"
				:link-label="t('auth.login')"
				to="/login"
			/>
		</template>
	</LayoutCard>
</template>
