<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query';

const auth = useAuthStore();
const toast = useToast();
const queryClient = useQueryClient();
const { t } = useI18n();
const { data: organizations } = useMyOrganizations();

const options = computed(() =>
  (organizations.value ?? []).map((org) => ({
    label: org.organizationName ?? t('common.unknown'),
    value: org.organizationId ?? '',
  })));

const selectedOrganizationId = computed({
  get: () => auth.organizationId ?? '',
  set: (orgId: string) => {
    void switchOrganization(orgId);
  },
});

async function switchOrganization(orgId: string) {
  if (!orgId || orgId === auth.organizationId) {
    return;
  }

  try {
    auth.selectOrganization(orgId);
    await invalidateOrgScopedQueries(queryClient);
    toast.add({ title: t('layout.organizationSwitched'), color: 'success' });
  } catch (error) {
    const { message } = useApiError(error);
    toast.add({ title: message.value, color: 'error' });
  }
}
</script>

<template>
	<USelect
		v-if="options.length > 0"
		v-model="selectedOrganizationId"
		:items="options"
		icon="i-lucide-building-2"
		:placeholder="t('nav.organization')"
		class="w-full"
		size="sm"
	/>
</template>
