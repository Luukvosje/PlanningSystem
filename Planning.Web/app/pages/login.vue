<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const { credentialsForm, orgForm, showOrgPicker } = useLoginForms();

useHead({ title: computed(() => t('auth.login')) });
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
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

		<!-- Hidden during organization selection: at that point you are already signed in. -->
		<p
			v-if="!showOrgPicker"
			class="text-center text-sm mt-4"
		>
			<NuxtLink
				to="/forgot-password"
				class="font-medium text-brand"
			>
				{{ t('auth.forgotPasswordLink') }}
			</NuxtLink>
		</p>

		<template #footer>
			<AuthFooterLink
				:question="t('auth.noAccountYet')"
				:link-label="t('auth.register')"
				to="/register"
			/>
		</template>
	</LayoutCard>
</template>
