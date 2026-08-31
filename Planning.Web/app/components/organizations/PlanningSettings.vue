<script setup lang="ts">
import { toTimeStrings } from '~/utils/planning/planningSettings';

const { t } = useI18n();
const { form, isLoading } = usePlanningSettingsForm();

const previewTimes = computed(() => toTimeStrings(form.state.importantWorkTimes));
</script>

<template>
	<LayoutCard
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
			<template #after>
				<OrganizationsImportantWorkTimesPreview :important-work-times="previewTimes" />
			</template>
		</component>

		<template
			v-if="!isLoading"
			#footer
		>
			<component :is="form.renderFooter" />
		</template>
	</LayoutCard>
</template>
