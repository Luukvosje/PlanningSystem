<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');
const { t } = useI18n();
const { form: orgForm, orgOptions, isLoading } = useSelectOrganizationForm();
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('organizations.select.title') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('organizations.select.description') }}
			</p>
		</template>

		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('organizations.loading')"
		/>

		<component
			:is="FormView"
			v-else-if="orgOptions.length > 1"
			:form="orgForm"
		/>
	</LayoutCard>
</template>
