<script setup lang="ts">
import type { CustomerResponse } from '~/generated/models';

defineProps<{
  customer: CustomerResponse
}>();

const { t } = useI18n();
</script>

<template>
	<LayoutSection
		:title="customer.name"
		:description="customer.email"
	>
		<slot>
			<dl
				v-if="customer.address"
				class="space-y-3 text-sm"
			>
				<div>
					<dt class="text-muted">
						{{ t('customers.fields.address') }}
					</dt>
					<dd class="font-medium whitespace-pre-line">
						{{ customer.address }}
					</dd>
				</div>
			</dl>
		</slot>
	</LayoutSection>
</template>
