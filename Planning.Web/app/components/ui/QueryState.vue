<script setup lang="ts">
/**
 * Fixed order for the states around a query: error first, then loading, then content.
 *
 * Eight pages used to spell this out themselves in three different orders, and three of them
 * rendered no error state at all - a failing query simply showed an empty page.
 */
const props = withDefaults(defineProps<{
	error?: unknown
	loading?: boolean
	loadingLabel?: string | null
	/** Keep previously rendered content visible while refetching. */
	keepContentOnLoading?: boolean
}>(), {
	error: undefined,
	loading: false,
	loadingLabel: undefined,
	keepContentOnLoading: false,
});

const { message } = useApiError(computed(() => props.error));

const showContent = computed(() => !props.error && (props.keepContentOnLoading || !props.loading));
</script>

<template>
	<UAlert
		v-if="error"
		class="shrink-0"
		color="error"
		:title="message"
	/>

	<UiLoadingIndicator
		v-else-if="loading"
		class="shrink-0"
		:label="loadingLabel"
	/>

	<slot v-if="showContent" />
</template>
