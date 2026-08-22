<script setup lang="ts">
const props = withDefaults(defineProps<{
  title?: string
  description?: string
  /** Fill remaining height; body becomes a flex column so nested content (e.g. a form) can pin a footer to the bottom. */
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

function mergeSlotClasses(...classes: Array<string | undefined>): string[] | undefined {
  const filtered = classes.filter((value): value is string => Boolean(value));
  return filtered.length ? filtered : undefined;
}

/**
 * UCard klipt zijn root standaard (`overflow-hidden`). Dat maakt de kaart zelf een scrollport, met
 * twee gevolgen: als flex-item krijgt hij een automatische minimumhoogte van 0 en krimpt hij in de
 * `h-full` kolom van LayoutPageContainer tot vensterhoogte - inhoud afgesneden, niets om te
 * scrollen - en een `sticky bottom-0` footer plakt aan de kaart in plaats van aan de pagina.
 */
const cardUi = computed(() => {
  const root = mergeSlotClasses('overflow-visible', props.ui?.root);
  const header = mergeSlotClasses(props.fill ? 'shrink-0' : undefined, props.ui?.header);
  const body = mergeSlotClasses(props.fill ? 'flex min-h-0 flex-1 flex-col overflow-visible' : undefined, props.ui?.body);
  const footer = mergeSlotClasses(props.fill ? 'shrink-0' : undefined, props.ui?.footer);

  return {
    ...props.ui,
    root,
    ...(header ? { header } : {}),
    ...(body ? { body } : {}),
    ...(footer ? { footer } : {}),
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
