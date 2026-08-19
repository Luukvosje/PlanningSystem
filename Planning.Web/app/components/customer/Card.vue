<script setup lang="ts">
import type { CustomerResponse } from '~/generated/models';

defineProps<{
  customer: CustomerResponse
}>();

const { t } = useI18n();
</script>

<template>
	<div class="flex flex-col gap-2">

		<LayoutPageHeader
			:title="customer.name"
			:subtitle="customer.email"
		/>
	
		<slot>
			<dl class="space-y-3 text-sm">
				<div>
					<dt class="text-muted">
						{{ t('customers.fields.email') }}
					</dt>
					<dd class="font-medium">
						{{ customer.email }}
					</dd>
				</div>
				<div v-if="customer.address">
					<dt class="text-muted">
						{{ t('customers.fields.address') }}
					</dt>
					<dd class="font-medium whitespace-pre-line">
						{{ customer.address }}
					</dd>
				</div>
			</dl>
		</slot>
	</div>
</template>
