<script setup lang="ts">
import type { UserResponse } from '~/generated/models';

/**
 * Whether a member can sign in yet. A member added by name has no account until they accept an
 * invite, and the only useful thing to do about that is to send one - hence the button.
 */
withDefaults(defineProps<{
	user: UserResponse
	canInvite: boolean
	size?: 'xs' | 'sm'
}>(), {
	size: 'sm',
});

defineEmits<{
	invite: []
}>();

const { t } = useI18n();
</script>

<template>
	<span
		v-if="user.hasAccount"
		class="inline-flex items-center gap-1.5 text-sm text-muted"
	>
		<UIcon
			name="i-lucide-check"
			class="size-4"
		/>
		{{ t('users.account.linked') }}
	</span>
	<UButton
		v-else-if="canInvite"
		:size="size"
		variant="outline"
		color="neutral"
		icon="i-lucide-mail"
		:label="t('users.account.invite')"
		@click="$emit('invite')"
	/>
	<UBadge
		v-else
		color="neutral"
		variant="subtle"
	>
		{{ t('users.account.none') }}
	</UBadge>
</template>
