<script setup lang="ts">
definePageMeta({ layout: 'auth' });

const FormView = resolveComponent('FormView');

const auth = useAuthStore();
const { t } = useI18n();

const joinForm = useJoinForm();

const canPreview = computed(() => auth.isAuthenticated && joinForm.state.code.length >= 4);
const previewCode = computed(() => canPreview.value ? joinForm.state.code : '');
const { data: preview, isLoading: previewLoading } = useInvitePreview(previewCode);
</script>

<template>
	<LayoutCard class="w-full lg:max-w-2xl mx-auto">
		<template #header>
			<h1 class="text-xl font-semibold">
				{{ t('invites.acceptTitle') }}
			</h1>
			<p class="text-sm text-muted mt-1">
				{{ t('invites.acceptDescription') }}
			</p>
		</template>

		<component
			:is="FormView"
			:form="joinForm"
		>
			<template #after>
				<div v-if="auth.isAuthenticated && joinForm.state.code.length >= 4">
					<div
						v-if="previewLoading"
						class="py-2"
					>
						<UiLoadingIndicator
							:label="t('invites.checkingCode')"
							size="sm"
						/>
					</div>
					<UAlert
						v-else-if="preview"
						:color="preview.isValid ? 'info' : 'warning'"
						variant="subtle"
						:title="preview.organizationName ?? t('nav.organization')"
						:description="preview.isValid
							? t('invites.previewValid')
							: t('invites.previewInvalid')"
					/>
				</div>
			</template>
		</component>

		<template #footer>
			<AuthFooterLink
				:question="t('auth.noAccountYet')"
				:link-label="t('auth.register')"
				:to="{ path: '/register', query: { redirect: '/join', code: joinForm.state.code } }"
			/>
		</template>
	</LayoutCard>
</template>
