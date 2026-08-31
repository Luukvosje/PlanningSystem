<script setup lang="ts">
import type { EntityTab } from '~/composables/useEntityTabs';
import { ApprovalStatus, RequestKind } from '~/generated/models';
import type { PlanningRequest } from '~/types/requests';

definePageMeta({ layout: false });

const auth = useAuthStore();
const { t } = useI18n();

onMounted(async () => {
	await auth.fetchMe();

	if (!canManagePlanning(auth.currentUser?.role)) {
		await navigateTo('/dashboard');
	}
});

type TabValue = 'all' | 'swaps' | 'leave'

const TAB_VALUES: TabValue[] = ['all', 'swaps', 'leave'];

/**
 * Which kinds a tab shows. `null` is "everything", so a weekly availability change is visible under
 * Alles without a tab of its own. Swaps match nothing until they exist - the tab is there so the
 * inbox does not silently change shape when they land.
 */
const TAB_KINDS: Record<TabValue, RequestKind[] | null> = {
	all: null,
	swaps: [],
	leave: [RequestKind.Leave],
};

function inTab(request: PlanningRequest, tab: TabValue) {
	const kinds = TAB_KINDS[tab];
	return kinds === null || kinds.includes(request.kind);
}

// One query for both lists: what was decided stays on screen under the open requests, so the
// planner can see their own last calls without asking for them.
const query = useRequests({ includeDecided: ref(true) });

const api = useRequestsApi();

const items = computed(() => query.data.value?.items ?? []);
const pending = computed(() => items.value.filter((request) => request.status === ApprovalStatus.Pending));
const decided = computed(() => items.value.filter((request) => request.status !== ApprovalStatus.Pending));

const { tab, items: tabItems } = useEntityTabs(computed<EntityTab[]>(() =>
	TAB_VALUES.map((value) => ({
		value,
		label: t(`requests.tabs.${value}`),
		// `|| undefined`: a zero would render an empty-looking badge, the component treats 0 as a value.
		badge: pending.value.filter((request) => inTab(request, value)).length || undefined,
	}))));

const activeTab = computed(() => tab.value as TabValue);
const visiblePending = computed(() => pending.value.filter((request) => inTab(request, activeTab.value)));
const visibleDecided = computed(() => decided.value.filter((request) => inTab(request, activeTab.value)));

const decidingId = ref<string | null>(null);
const approveAllOpen = ref(false);

async function decide(request: PlanningRequest, approve: boolean) {
	decidingId.value = request.id;

	try {
		const refs = [{ kind: request.kind, id: request.id }];
		await (approve ? api.approve : api.reject).mutateAsync(refs);
	} catch {
		// The mutation's onError already told the user; nothing to add here.
	} finally {
		decidingId.value = null;
	}
}

async function approveAll() {
	approveAllOpen.value = false;
	const refs = visiblePending.value.map((request) => ({ kind: request.kind, id: request.id }));

	if (!refs.length) {
		return;
	}

	try {
		await api.approve.mutateAsync(refs);
	} catch {
		// The mutation's onError already told the user; nothing to add here.
	}
}
</script>

<template>
	<NuxtLayout name="default">
		<template #actions>
			<UButton
				v-if="visiblePending.length > 1"
				icon="i-lucide-check-check"
				variant="outline"
				:label="t('requests.actions.approveAll')"
				@click="approveAllOpen = true"
			/>
		</template>

		<template #tabs>
			<LayoutPageTabs
				v-model="tab"
				:items="tabItems"
			/>
		</template>

		<LayoutPageContainer>
			<UiQueryState
				:error="query.error.value"
				:loading="query.isLoading.value"
				:loading-label="t('requests.loading')"
			>
				<div class="flex flex-col gap-6">
					<div
						v-if="visiblePending.length"
						class="flex flex-col gap-2"
					>
						<RequestsRow
							v-for="request in visiblePending"
							:key="request.id"
							:request="request"
							decidable
							:busy="decidingId === request.id"
							@approve="decide(request, true)"
							@reject="decide(request, false)"
						/>
					</div>

					<UiEmptyState
						v-else
						icon="i-lucide-inbox"
						:title="activeTab === 'swaps' ? t('requests.emptySwaps') : t('requests.empty')"
						:description="activeTab === 'swaps' ? t('requests.emptySwapsDescription') : undefined"
					/>

					<section class="flex flex-col gap-2 border-t border-default pt-5">
						<h2 class="text-sm font-semibold uppercase tracking-wide text-muted">
							{{ t('requests.decided') }}
						</h2>

						<div
							v-if="visibleDecided.length"
							class="flex flex-col gap-2 opacity-70"
						>
							<RequestsRow
								v-for="request in visibleDecided"
								:key="request.id"
								:request="request"
							/>
						</div>

						<UiEmptyState
							v-else
							:title="t('requests.noDecided')"
						/>
					</section>
				</div>
			</UiQueryState>
		</LayoutPageContainer>

		<UiConfirmModal
			v-model:open="approveAllOpen"
			:title="t('requests.approveAll.title')"
			:description="t('requests.approveAll.description', { count: visiblePending.length })"
			:confirm-label="t('requests.actions.approveAll')"
			confirm-color="brand"
			:loading="api.approve.isPending.value"
			@confirm="approveAll"
		/>
	</NuxtLayout>
</template>
