<script setup lang="ts">
/**
 * Every tab reads "<page> · Planning". A plain '%s · Planning' string would leave the
 * separator dangling on the pages that set no title of their own, so the template is a
 * function that falls back to the product name alone.
 */
useHead({
  titleTemplate: (title) => (title ? `${title} · Planning` : 'Planning'),
});
</script>

<template>
	<UApp>
		<NuxtRouteAnnouncer />
		<NuxtLayout>
			<NuxtPage />
		</NuxtLayout>

		<!-- One dialog for every delete in the app; see useDeleteConfirm. -->
		<UiDeleteConfirm />
	</UApp>
</template>