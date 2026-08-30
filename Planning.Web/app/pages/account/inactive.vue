<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const auth = useAuthStore();
const { t } = useI18n();
const { data: memberships } = useMyOrganizations();

/**
 * You only land here with memberships that are all inactive, so naming them tells you who to
 * chase. It stays a fallback though: the list is a cached query and may not have arrived.
 */
const organizations = computed(() =>
  (memberships.value ?? [])
    .filter((membership) => !membership.isActive)
    .map((membership) => membership.organizationName)
    .join(', '));

async function onLogout() {
  auth.logout();
  await navigateTo('/login');
}
</script>

<template>
	<LayoutCard class="mx-auto w-full lg:max-w-lg">
		<template #header>
			<div class="flex items-start gap-3">
				<UIcon
					name="i-lucide-user-x"
					class="mt-1 size-5 shrink-0 text-warning"
				/>
				<div class="min-w-0">
					<h1 class="text-xl font-semibold">
						{{ t('account.inactive.title') }}
					</h1>
					<p class="mt-1 text-sm text-muted">
						{{ organizations
							? t('account.inactive.description', { organizations })
							: t('account.inactive.descriptionUnknown') }}
					</p>
				</div>
			</div>
		</template>

		<p class="text-sm text-muted">
			{{ t('account.inactive.contactAdmin') }}
		</p>

		<template #footer>
			<UButton
				icon="i-lucide-log-out"
				color="neutral"
				variant="outline"
				:label="t('layout.userMenu.logout')"
				@click="() => { onLogout() }"
			/>
		</template>
	</LayoutCard>
</template>
