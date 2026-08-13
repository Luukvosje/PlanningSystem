<script setup lang="ts">
import { getImportantGridLines } from '~/utils/planning/timelineMath';
import { getIntlLocale } from '~/utils/planning/dateUtils';
import { uniqueImportantTimes } from '~/utils/planning/planningSettings';

const props = defineProps<{
  importantWorkTimes: string[]
}>();

const { t, locale } = useI18n();
const intlLocale = computed(() => getIntlLocale(locale.value));

const PREVIEW_DAY_WIDTH = 640;
const hourSlotWidth = PREVIEW_DAY_WIDTH / 24;

const importantLines = computed(() =>
  getImportantGridLines(PREVIEW_DAY_WIDTH, uniqueImportantTimes(props.importantWorkTimes), intlLocale.value),
);

const _hourLines = computed(() =>
  Array.from({ length: 24 }, (_, hour) => ({
    leftPx: hour * hourSlotWidth,
    showLabel: hour % 6 === 0,
    label: `${String(hour).padStart(2, '0')}:00`,
  })),
);
</script>

<template>
	<div class="space-y-2">
		<p class="text-xs font-medium text-muted">
			{{ t('organizations.planningSettings.preview') }}
		</p>

		<div class="overflow-x-auto rounded-lg border border-default bg-default">
			<div
				class="relative shrink-0"
				:style="{ width: `${PREVIEW_DAY_WIDTH}px` }"
			>
				<div class="relative h-8 border-b border-default/50 bg-default">
					<div
						v-for="line in importantLines"
						:key="`header-important-${line.leftPx}`"
						class="absolute top-0 bottom-0 border-l-2 border-default/70 pointer-events-none"
						:style="{ left: `${line.leftPx}px` }"
					>
						<span
							v-if="line.label"
							class="absolute top-2 left-1 text-[10px] font-semibold text-default leading-none whitespace-nowrap"
						>
							{{ line.label }}
						</span>
					</div>
				</div>

				<div class="relative h-12 bg-secondary/5">
					<div
						v-for="line in importantLines"
						:key="`row-important-${line.leftPx}`"
						class="absolute inset-y-0 border-l-2 border-default/60 pointer-events-none"
						:style="{ left: `${line.leftPx}px` }"
					/>
				</div>
			</div>
		</div>

		<p class="text-xs text-muted">
			{{ t('organizations.planningSettings.previewDescription') }}
		</p>
	</div>
</template>
