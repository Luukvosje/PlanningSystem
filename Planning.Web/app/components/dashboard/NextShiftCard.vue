<script setup lang="ts">
import { formatRelativePlanningDate, formatTimeRange } from '~/utils/planning/dateUtils';

const { nextShift, isLoading } = useNextPlanningShift();
</script>

<template>
	<UCard :ui="{ body: 'p-6' }">
		<div class="flex flex-col gap-4">
			<div class="min-w-0 space-y-3">
				<p class="text-xs font-semibold uppercase tracking-wider text-muted">
					Volgende dienst
				</p>

				<div v-if="isLoading">
					<UiLoadingIndicator
						label="Dienst laden..."
						size="sm"
					/>
				</div>

				<div
					v-else-if="nextShift"
					class="space-y-1"
				>
					<p class="text-lg font-semibold">
						{{ formatRelativePlanningDate(nextShift.startUtc) }}
					</p>
					<p class="text-sm text-muted">
						{{ formatTimeRange(nextShift.startUtc, nextShift.endUtc) }}
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
						<span class="font-medium text-default">Notitie:</span>
						{{ nextShift.notes }}
					</p>
				</div>

				<div
					v-else
					class="space-y-1"
				>
					<p class="text-sm font-medium">
						Geen geplande diensten
					</p>
					<p class="text-sm text-muted">
						Er staan momenteel geen toekomstige diensten voor je ingepland.
					</p>
				</div>
			</div>

			<div class="flex items-center justify-end gap-3">
				<UIcon
					name="i-lucide-calendar-clock"
					class="size-5 text-secondary"
				/>
				<UButton
					to="/planning"
					variant="outline"
					icon="i-lucide-calendar-range"
					label="Bekijk planning"
				/>
			</div>
		</div>
	</UCard>
</template>
