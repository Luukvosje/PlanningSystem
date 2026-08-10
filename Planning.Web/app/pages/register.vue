<script setup lang="ts">
import { registerSchema } from '~/schemas/auth.schema';

definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();
const toast = useToast();

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
      label: 'Voornaam',
      type: 'input',
      required: true,
      props: { autocomplete: 'given-name' },
    },
    {
      name: 'lastName',
      label: 'Achternaam',
      type: 'input',
      required: true,
      hidden: true,
      props: { autocomplete: 'family-name' },
    },
    {
      name: 'email',
      label: 'E-mail',
      type: 'email',
      required: true,
      props: { autocomplete: 'email' },
    },
    {
      name: 'password',
      label: 'Wachtwoord',
      type: 'password',
      required: true,
      props: { autocomplete: 'new-password' },
    },
  ],
  submit: { label: 'Registreren', block: true },
  onSubmit: async (data) => {
    await auth.register(data);

    toast.add({ title: 'Account aangemaakt', description: 'Je kunt nu inloggen.', color: 'success' });

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
	<UCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				Account aanmaken
			</h1>
		</template>

		<component
			:is="FormView"
			:form="registerForm"
		>
			<template #control-firstName="{ form }">
				<div class="grid grid-cols-2 gap-4">
					<UFormField
						label="Voornaam"
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
						label="Achternaam"
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
			<p class="text-sm text-muted text-center">
				Al een account?
				<NuxtLink
					to="/login"
					class="text-primary font-medium"
				>
					Inloggen
				</NuxtLink>
			</p>
		</template>
	</UCard>
</template>
