<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const route = useRoute();

const token = computed(() => (route.query.token as string | undefined) ?? '');
const form = useResetPasswordForm(token);

useHead({ title: computed(() => t('auth.resetPasswordTitle')) });
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('auth.resetPasswordTitle') }}
			</h1>
			<p
				v-if="token"
				class="text-sm text-muted mt-1"
			>
				{{ t('auth.resetPasswordDescription') }}
			</p>
		</template>

		<!--
			Landing here without a token means the link was truncated somewhere between the mail
			client and the browser. Showing the form anyway would only fail on submit.
		-->
		<UAlert
			v-if="!token"
			color="warning"
			variant="subtle"
			icon="i-lucide-triangle-alert"
			:title="t('auth.resetLinkInvalidTitle')"
			:description="t('auth.resetLinkInvalidDescription')"
		/>

		<component
			:is="FormView"
			v-else
			:form="form"
		/>

		<template #footer>
			<AuthFooterLink
				:question="t('auth.needNewLink')"
				:link-label="t('auth.forgotPasswordLink')"
				to="/forgot-password"
			/>
		</template>
	</LayoutCard>
</template>
