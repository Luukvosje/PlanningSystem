<script setup lang="ts">
import type { FormControl } from '~/lib/form/control-types';
import { formatControlValue } from '~/lib/form/display';

/**
 * The read-only twin of `FormView`: same controls, same label-left/value-right rhythm, no inputs.
 *
 * Rendering from the form's own `controls` array is the point — a detail page that spells its
 * fields out again drifts from the form the moment a field is added.
 */
const props = defineProps<{
  controls: FormControl[]
  values: Record<string, unknown>
}>();

defineSlots<{
  [key: string]: (props: { control: FormControl, value: unknown, text: string | null }) => unknown
}>();

const { t } = useI18n();

const rows = computed(() =>
  props.controls
    .filter((control) => !control.hidden)
    .map((control) => {
      const value = props.values[control.name];

      return {
        control,
        value,
        text: formatControlValue(control, value),
      };
    }),
);
</script>

<template>
	<dl class="divide-y divide-default text-sm">
		<div
			v-for="row in rows"
			:key="row.control.name"
			class="grid grid-cols-1 items-start gap-x-4 gap-y-1 py-3 sm:grid-cols-[200px_1fr]"
		>
			<dt class="text-muted">
				{{ row.control.label }}
			</dt>
			<dd class="min-w-0">
				<slot
					:name="`control-${row.control.name}`"
					:control="row.control"
					:value="row.value"
					:text="row.text"
				>
					<span
						v-if="row.text"
						class="font-medium whitespace-pre-line"
					>{{ row.text }}</span>
					<span
						v-else
						class="text-dimmed"
					>{{ t('common.empty') }}</span>
				</slot>
			</dd>
		</div>
	</dl>
</template>
