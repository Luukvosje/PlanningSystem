<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const { credentialsForm, orgForm, showOrgPicker } = useLoginForms();
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

		<template #footer>
			<AuthFooterLink
				:question="t('auth.noAccountYet')"
				:link-label="t('auth.register')"
				to="/register"
			/>
		</template>
	</LayoutCard>
</template>
