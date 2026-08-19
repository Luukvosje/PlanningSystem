<script setup lang="ts">
import { formatRelativePlanningDate, formatTimeRange, getIntlLocale } from '~/utils/planning/dateUtils';

const { nextShift, isLoading } = useNextPlanningShift();
const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));
</script>

<template>
	<LayoutCard>
		<div class="flex flex-col gap-4">
			<div class="min-w-0 space-y-3">
				<p class="text-xs font-semibold uppercase tracking-wider text-muted">
					{{ t('dashboard.nextShift') }}
				</p>

				<div v-if="isLoading">
					<UiLoadingIndicator
						:label="t('dashboard.loadingShift')"
						size="sm"
					/>
				</div>

				<div
					v-else-if="nextShift"
					class="space-y-1"
				>
					<p class="text-lg font-semibold">
						{{ formatRelativePlanningDate(nextShift.startUtc, intlLocale, t) }}
					</p>
					<p class="text-sm text-muted">
						{{ formatTimeRange(nextShift.startUtc, nextShift.endUtc, intlLocale) }}
					</p>
					<p
						v-if="nextShift.customerName"
						class="text-sm font-medium"
					>
						{{ nextShift.customerName }}
					</p>
					<p
						v-if="nextShift.title"
						class="text-sm text-muted"
					>
						{{ nextShift.title }}
					</p>
					<p
						v-if="nextShift.notes"
						class="text-sm text-muted"
					>
						<span class="font-medium text-default">{{ t('dashboard.note') }}:</span>
						{{ nextShift.notes }}
					</p>
				</div>

				<div
					v-else
					class="space-y-1"
				>
					<p class="text-sm font-medium">
						{{ t('dashboard.noShiftsPlanned') }}
					</p>
					<p class="text-sm text-muted">
						{{ t('dashboard.noShiftsPlannedDescription') }}
					</p>
				</div>
			</div>

			<div class="flex items-center justify-end gap-3">
				<UIcon
					name="i-lucide-calendar-clock"
					class="size-5 text-brand"
				/>
				<UButton
					to="/planning"
					variant="outline"
					icon="i-lucide-calendar-range"
					:label="t('dashboard.viewPlanning')"
				/>
			</div>
		</div>
	</LayoutCard>
</template>
