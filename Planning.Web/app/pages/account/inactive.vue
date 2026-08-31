<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const auth = useAuthStore();
const { t } = useI18n();
const { data: memberships } = useMyOrganizations();

/**
 * You only land here with memberships that are all inactive, so naming them tells you who to
 * chase. Client-only: the query is not awaited during SSR, and rendering the names on both sides
 * would be a hydration mismatch every time.
 */
const organizations = computed(() =>
  (memberships.value ?? [])
    .filter((membership) => !membership.isActive)
    .map((membership) => membership.organizationName));

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
						{{ t('account.inactive.description') }}
					</p>
				</div>
			</div>
		</template>

		<div class="space-y-3">
			<p class="text-sm text-muted">
				{{ t('account.inactive.contactAdmin') }}
			</p>

			<ClientOnly>
				<div
					v-if="organizations.length"
					class="flex flex-wrap gap-1.5"
				>
					<UBadge
						v-for="name in organizations"
						:key="name"
						color="neutral"
						variant="subtle"
						icon="i-lucide-building-2"
					>
						{{ name }}
					</UBadge>
				</div>
			</ClientOnly>
		</div>

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
