<script setup lang="ts">
import type { OrganizationResponse } from '~/generated/models';

const props = defineProps<{
  organization: OrganizationResponse
}>();

const { t } = useI18n();
const config = useRuntimeConfig();
const { uploadLogo } = useOrganizationSettingsApi();

const fileInput = ref<HTMLInputElement | null>(null);
const logoVersion = ref(0);

const apiBaseUrl = computed(
  () => config.public.apiBaseUrl || config.apiBaseUrl,
);

const logoSrc = computed(() => {
  if (!props.organization.logoUrl) {
    return null;
  }

  const cacheKey = props.organization.updatedAtUtc ?? logoVersion.value;
  return `${apiBaseUrl.value}${props.organization.logoUrl}?v=${cacheKey}`;
});

function openFilePicker() {
  fileInput.value?.click();
}

async function onFileSelected(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];

  if (!file) {
    return;
  }

  await uploadLogo.mutateAsync(file);
  logoVersion.value++;
  input.value = '';
}
</script>

<template>
	<LayoutSection :title="t('organizations.logo.title')">
		<div class="flex items-center gap-4">
			<div
				class="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-lg border border-default bg-elevated"
			>
				<img
					v-if="logoSrc"
					:src="logoSrc"
					:alt="t('organizations.logo.alt')"
					class="size-full object-contain"
				>
				<UIcon
					v-else
					name="i-lucide-building-2"
					class="size-8 text-muted"
				/>
			</div>

			<div class="space-y-2">
				<p class="text-sm text-muted">
					{{ t('organizations.logo.constraints') }}
				</p>

				<input
					ref="fileInput"
					type="file"
					accept="image/png,image/jpeg,image/webp,image/gif"
					class="hidden"
					@change="onFileSelected"
				>

				<UButton
					:label="t('organizations.logo.upload')"
					icon="i-lucide-upload"
					variant="outline"
					:loading="uploadLogo.isPending.value"
					@click="openFilePicker"
				/>
			</div>
		</div>
	</LayoutSection>
</template>
