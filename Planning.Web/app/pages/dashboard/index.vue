<script setup lang="ts">
definePageMeta({ layout: 'default' });

const auth = useAuthStore();
const { t } = useI18n();

onMounted(() => auth.fetchMe());

const { data: users, isLoading } = useUsers();

const teamCount = computed(() => users.value?.length ?? 0);
const organizationName = computed(() => auth.currentUser?.organizationName ?? t('common.unknown'));
const roleLabel = computed(() => getRoleLabel(auth.currentUser?.role, t));
const canManage = computed(() => canManagePlanning(auth.currentUser?.role));
</script>

<template>
	<LayoutPageContainer>
		<LayoutPageHeader
			:subtitle="t('dashboard.overview', { organization: organizationName })"
		/>

		<div class="flex flex-col gap-6">
			<DashboardNextShiftCard />

			<div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
				<LayoutCard>
					<div class="flex items-start justify-between">
						<div>
							<p class="text-xs font-semibold uppercase tracking-wide text-muted">
								{{ t('nav.organization') }}
							</p>
							<p class="mt-2 text-lg font-semibold">
								{{ organizationName }}
							</p>
						</div>
						<UIcon
							name="i-lucide-building-2"
							class="size-5 text-brand"
						/>
					</div>
				</LayoutCard>

				<LayoutCard>
					<div class="flex items-start justify-between">
						<div>
							<p class="text-xs font-semibold uppercase tracking-wide text-muted">
								{{ t('dashboard.teamMembers') }}
							</p>
							<p class="mt-2 text-lg font-semibold">
								<UIcon
									v-if="isLoading"
									name="i-lucide-loader-circle"
									class="size-5 text-brand animate-spin"
								/>
								<span v-else>{{ teamCount }}</span>
							</p>
						</div>
						<UIcon
							name="i-lucide-users"
							class="size-5 text-brand"
						/>
					</div>
				</LayoutCard>

				<LayoutCard>
					<div class="flex items-start justify-between">
						<div>
							<p class="text-xs font-semibold uppercase tracking-wide text-muted">
								{{ t('dashboard.yourRole') }}
							</p>
							<p class="mt-2 text-lg font-semibold">
								{{ roleLabel }}
							</p>
						</div>
						<UIcon
							name="i-lucide-shield"
							class="size-5 text-brand"
						/>
					</div>
				</LayoutCard>
			</div>

			<div class="grid gap-6 sm:grid-cols-2">
				<LayoutCard :title="t('dashboard.quickLinks')">
					<div class="flex flex-col gap-2">
						<UButton
							to="/planning"
							variant="outline"
							icon="i-lucide-calendar-range"
							block
						>
							{{ t('dashboard.viewPlanning') }}
						</UButton>
						<UButton
							v-if="canManageOrganization(auth.currentUser?.role)"
							to="/users"
							variant="outline"
							icon="i-lucide-users"
							block
						>
							{{ t('dashboard.viewTeam') }}
						</UButton>
					</div>
				</LayoutCard>

				<LayoutCard :title="t('dashboard.today')">
					<p class="text-sm text-muted">
						{{ canManage ? t('dashboard.welcomeBackManage') : t('dashboard.welcomeBack') }}
					</p>
				</LayoutCard>
			</div>
		</div>
	</LayoutPageContainer>
</template>
