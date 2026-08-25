<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const registerForm = useRegisterForm();

useHead({ title: computed(() => t('auth.createAccount')) });
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
