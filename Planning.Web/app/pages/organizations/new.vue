<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const organizationForm = useCreateOrganizationForm();
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('organizations.create.title') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('organizations.create.firstOrgDescription') }}
			</p>
		</template>

		<component
			:is="FormView"
			:form="organizationForm"
		/>

		<template #footer>
			<AuthFooterLink
				:question="t('organizations.invited')"
				:link-label="t('invites.enterCode')"
				to="/join"
			/>
		</template>
	</LayoutCard>
</template>
