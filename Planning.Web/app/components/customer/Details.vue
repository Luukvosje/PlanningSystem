<script setup lang="ts">
import type { CustomerResponse } from '~/generated/models';

const props = defineProps<{
  customer: CustomerResponse
}>();

const auth = useAuthStore();
const canManage = computed(() => canManageCustomers(auth.currentUser?.role));
const customerEdit = useCustomerEdit();
const { t } = useI18n();

// Same controls the edit modal renders, so a new field shows up in both without a second declaration.
const controls = customerEdit.form.controls;
const values = computed(() => customerEdit.toState(props.customer));
</script>

<template>
	<LayoutSection :title="t('customers.details')">
		<template
			v-if="canManage"
			#actions
		>
			<UButton
				variant="outline"
				color="neutral"
				icon="i-lucide-pencil"
				size="sm"
				@click="customerEdit.open(customer)"
			>
				{{ t('common.actions.edit') }}
			</UButton>
		</template>

		<FormDisplay
			:controls="controls"
			:values="values"
		/>
	</LayoutSection>
</template>
