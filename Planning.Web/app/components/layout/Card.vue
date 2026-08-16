<script setup lang="ts">
const props = withDefaults(defineProps<{
  title?: string
  description?: string
  /** Fill remaining height; the body scrolls internally. */
  fill?: boolean
  variant?: 'solid' | 'outline' | 'soft' | 'subtle'
  ui?: {
    root?: string
    header?: string
    title?: string
    description?: string
    body?: string
    footer?: string
  }
}>(), {
  title: undefined,
  description: undefined,
  fill: false,
  variant: undefined,
  ui: undefined,
});

const cardUi = computed(() => {
  const header = [
    props.fill ? 'shrink-0' : undefined,
    props.ui?.header,
  ].filter((value): value is string => Boolean(value));

  const body = [
    props.fill ? 'min-h-0 flex-1 overflow-auto' : undefined,
    props.ui?.body,
  ].filter((value): value is string => Boolean(value));

  const footer = [
    props.fill ? 'shrink-0' : undefined,
    props.ui?.footer,
  ].filter((value): value is string => Boolean(value));

  if (!props.ui && header.length === 0 && body.length === 0 && footer.length === 0) {
    return undefined;
  }

  return {
    ...props.ui,
    ...(header.length ? { header } : {}),
    ...(body.length ? { body } : {}),
    ...(footer.length ? { footer } : {}),
  };
});
</script>

<template>
	<UCard
		:variant="variant"
		:class="fill ? 'flex h-full min-h-0 flex-1 flex-col' : undefined"
		:ui="cardUi"
	>
		<template
			v-if="title || description || $slots.header || $slots.actions"
			#header
		>
			<slot name="header">
				<div class="flex items-center justify-between gap-4">
					<div>
						<h3
							v-if="title"
							class="font-semibold"
						>
							{{ title }}
						</h3>
						<p
							v-if="description"
							class="text-muted text-sm"
						>
							{{ description }}
						</p>
					</div>
					<div
						v-if="$slots.actions"
						class="shrink-0"
					>
						<slot name="actions" />
					</div>
				</div>
			</slot>
		</template>

		<slot />

		<template
			v-if="$slots.footer"
			#footer
		>
			<slot name="footer" />
		</template>
	</UCard>
</template>
