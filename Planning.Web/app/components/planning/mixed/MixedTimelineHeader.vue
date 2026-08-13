<script setup lang="ts">
import { getHourMarkers, getMixedTimelineWidth, MIXED_HOUR_WIDTH } from '~/utils/planning/mixedTimelineMath';
import { getIntlLocale } from '~/utils/planning/dateUtils';

const { locale } = useI18n();
const hourMarkers = computed(() => getHourMarkers(getIntlLocale(locale.value)));
const timelineWidth = getMixedTimelineWidth();
</script>

<template>
	<div
		class="sticky top-0 z-20 relative border-b border-default bg-default/95 backdrop-blur"
		:style="{ minWidth: `${timelineWidth}px`, height: '40px' }"
	>
		<div
			v-for="marker in hourMarkers"
			:key="marker.hour"
			class="absolute top-0 bottom-0 border-r border-default/40"
			:style="{ left: `${marker.leftPx}px`, width: `${MIXED_HOUR_WIDTH}px` }"
		>
			<span class="absolute left-1 top-2 text-xs font-medium text-muted whitespace-nowrap">
				{{ marker.label }}
			</span>
		</div>
	</div>
</template>
