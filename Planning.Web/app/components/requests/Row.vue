<script setup lang="ts">
import type { PlanningRequest } from '~/types/requests';
import {
	getRequestInitials,
	getRequestKindColor,
	getRequestKindLabel,
	getRequestStatusColor,
	getRequestStatusLabel,
	getRequestSummary,
} from '~/types/requests';
import { formatTimeAgo, getIntlLocale } from '~/utils/planning/dateUtils';

/**
 * One request in the inbox. A pending row offers the two decisions; a decided one is history and
 * says what happened instead, so the list reads the same whether or not there is anything to do.
 */
const props = withDefaults(defineProps<{
	request: PlanningRequest
	decidable?: boolean
	busy?: boolean
}>(), {
	decidable: false,
	busy: false,
});

const emit = defineEmits<{
	approve: []
	reject: []
}>();

const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));

const name = computed(() =>
	`${props.request.employeeFirstName} ${props.request.employeeLastName}`.trim());
const summary = computed(() => getRequestSummary(props.request, t, intlLocale.value));
const submittedAt = computed(() => formatTimeAgo(props.request.createdAtUtc, intlLocale.value));
</script>

<template>
	<div class="flex items-center gap-3 rounded-lg border border-default px-3 py-2.5">
		<UAvatar
			:text="getRequestInitials(request)"
			size="md"
			class="shrink-0"
		/>

		<div class="min-w-0 flex-1">
			<div class="flex min-w-0 flex-wrap items-center gap-2">
				<p class="truncate text-sm font-semibold">
					{{ name }}
				</p>
				<UBadge
					:color="getRequestKindColor(request.kind)"
					variant="subtle"
					size="sm"
					:label="getRequestKindLabel(request.kind, t)"
				/>
			</div>
			<p class="mt-0.5 truncate text-sm text-muted">
				{{ summary }}
			</p>
		</div>

		<p class="shrink-0 text-xs tabular-nums text-dimmed max-sm:hidden">
			{{ submittedAt }}
		</p>

		<div
			v-if="decidable"
			class="flex shrink-0 items-center gap-1"
		>
			<UButton
				variant="ghost"
				color="neutral"
				size="sm"
				:disabled="busy"
				:label="t('requests.actions.reject')"
				@click="emit('reject')"
			/>
			<UButton
				color="brand"
				size="sm"
				trailing-icon="i-lucide-check"
				:loading="busy"
				:label="t('requests.actions.approve')"
				@click="emit('approve')"
			/>
		</div>

		<UBadge
			v-else
			:color="getRequestStatusColor(request.status)"
			variant="subtle"
			size="sm"
			class="shrink-0"
			:label="getRequestStatusLabel(request.status, t)"
		/>
	</div>
</template>
