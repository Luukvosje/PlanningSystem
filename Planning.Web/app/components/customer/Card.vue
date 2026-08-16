<script setup lang="ts">
import type { CustomerResponse } from '~/generated/models';

defineProps<{
  customer: CustomerResponse
}>();

const { t } = useI18n();
</script>

<template>
	<LayoutCard>
		<template #header>
			<h2 class="text-xl font-semibold">
				{{ customer.name }}
			</h2>
			<p class="text-sm text-muted mt-1">
				{{ customer.email }}
			</p>
		</template>

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
	</LayoutCard>
</template>
