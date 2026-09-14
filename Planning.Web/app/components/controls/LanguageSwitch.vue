<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui';

/**
 * A globe that opens the language list. Used on the auth screens, where there is no user menu
 * to hang the language under; the app shell keeps it in the user menu.
 */
const { t, locale, setLocale } = useI18n();

const items = computed<DropdownMenuItem[]>(() =>
  LANGUAGE_OPTIONS.map((option) => ({
    label: option.label,
    icon: option.icon,
    type: 'checkbox' as const,
    checked: locale.value === option.code,
    onSelect: () => setLocale(option.code),
  })));
</script>

<template>
	<UDropdownMenu
		:items="items"
		:content="{ align: 'start', side: 'top', sideOffset: 8 }"
		:ui="{ content: 'min-w-44' }"
	>
		<UButton
			icon="i-lucide-globe"
			color="neutral"
			variant="ghost"
			square
			size="md"
			class="data-[state=open]:bg-elevated"
			:aria-label="t('layout.userMenu.language')"
		/>
	</UDropdownMenu>
</template>
