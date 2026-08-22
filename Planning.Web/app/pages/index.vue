<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const auth = useAuthStore();
const { t } = useI18n();

if (auth.isAuthenticated) {
  await navigateTo(auth.hasOrganization ? '/dashboard' : '/organization/new');
}
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				Planning
			</h1>
			<p class="text-muted text-sm mt-1">
				{{ t('auth.landingTagline') }}
			</p>
		</template>

		<div class="flex flex-col gap-3">
			<UButton
				to="/login"
				block
				size="lg"
			>
				{{ t('auth.login') }}
			</UButton>
			<UButton
				to="/register"
				variant="outline"
				block
				size="lg"
			>
				{{ t('auth.createAccount') }}
			</UButton>
			<UButton
				to="/join"
				variant="ghost"
				block
			>
				{{ t('invites.enterCode') }}
			</UButton>
		</div>
	</LayoutCard>
</template>
