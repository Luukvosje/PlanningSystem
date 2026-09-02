<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const { form, submitted } = useForgotPasswordForm();

useHead({ title: computed(() => t('auth.forgotPasswordTitle')) });
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('auth.forgotPasswordTitle') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('auth.forgotPasswordDescription') }}
			</p>
		</template>

		<!--
			The confirmation deliberately never says whether the address was known. It replaces the
			form so nobody sits waiting for a mail while looking at an unchanged page.
		-->
		<UAlert
			v-if="submitted"
			color="success"
			variant="subtle"
			icon="i-lucide-mail-check"
			:title="t('auth.resetLinkSentTitle')"
			:description="t('auth.resetLinkSentDescription')"
		/>

		<component
			:is="FormView"
			v-else
			:form="form"
		/>

		<template #footer>
			<AuthFooterLink
				:question="t('auth.rememberedPassword')"
				:link-label="t('auth.login')"
				to="/login"
			/>
		</template>
	</LayoutCard>
</template>
