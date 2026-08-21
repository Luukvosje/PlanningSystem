<script setup lang="ts">
import type { CustomerResponse } from '~/generated/models';

defineProps<{
  customer: CustomerResponse
}>();

const auth = useAuthStore();
const canManage = computed(() => canManageCustomers(auth.currentUser?.role));
const customerEdit = useCustomerEdit();
const { t } = useI18n();
</script>

<template>
	<FormEditableSection
		:key="customer.id"
		:edit="customerEdit"
		:entity="customer"
		:title="t('customers.details')"
		:can-edit="canManage"
	/>
</template>
