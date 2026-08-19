<script setup lang="ts">
const { t } = useI18n();
const { form, isLoading } = usePlanningSettingsForm();
</script>

<template>
	<LayoutSection
		:title="t('organizations.planningSettings.title')"
		:description="t('organizations.planningSettings.description')"
	>
		<UiLoadingIndicator
			v-if="isLoading"
			:label="t('organizations.planningSettings.loading')"
		/>

		<component
			:is="form.render"
			v-else
		>
			<template #footer>
				<component :is="form.renderFooter" />
			</template>
		</component>
	</LayoutSection>
</template>
