<script setup lang="ts">
withDefaults(defineProps<{
  /** Fill available height without page scroll (e.g. full-height tables). */
  fill?: boolean
}>(), {
  fill: false,
});
</script>

<template>
	<!--
		Alleen bij `fill` wordt de container aan de vensterhoogte geknoopt. Zonder `fill` zou
		`h-full min-h-0` hem laten krimpen tot de scrollport, waardoor een langere kaart eruit hangt en
		zijn onderrand exact op de scrollrand valt - de `ring` van UCard is een outset box-shadow en
		wordt daar weggeknipt. Op contenthoogte houdt de `p-0.5` die rand binnen beeld.
	-->
	<div
		class="mx-auto flex w-full flex-col gap-6 p-0.5"
		:class="fill ? 'h-full min-h-0 overflow-hidden' : undefined"
	>
		<slot />
	</div>
</template>
