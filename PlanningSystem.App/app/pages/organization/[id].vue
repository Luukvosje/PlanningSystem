<script setup lang="ts">
import type { Organization, User, OrganizationInviteUserRequest } from '~/typescript/types'

definePageMeta({
  middleware: 'auth-client',
})

const route = useRoute()
const { $serviceFactory: sf } = useNuxtApp()
const { t } = useI18n()

const orgId = computed(() => Number(route.params.id))

const organization = ref<Organization | null>(null)
const users = ref<User[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const inviteForm = ref<OrganizationInviteUserRequest>({
  email: '',
  username: '',
})
const inviteLoading = ref(false)
const inviteError = ref<string | null>(null)
const invitedPassword = ref<string | null>(null)
const showPasswordModal = ref(false)

async function loadOrganization() {
  if (!orgId.value || isNaN(orgId.value)) return
  loading.value = true
  error.value = null
  try {
    const [orgResult, usersResult] = await Promise.all([
      sf.organizationService.getOrganization(orgId.value),
      sf.organizationService.getOrganizationUsers(orgId.value),
    ])
    if (orgResult.success) {
      organization.value = orgResult.data
    } else {
      error.value = orgResult.message || t('organization.notFound')
    }
    if (usersResult.success) {
      users.value = usersResult.data ?? []
    }
  } catch (e) {
    error.value = t('organization.loadFailed')
  } finally {
    loading.value = false
  }
}

async function inviteUser() {
  if (!orgId.value || !inviteForm.value.email.trim() || !inviteForm.value.username.trim()) {
    inviteError.value = t('organization.inviteError')
    return
  }
  inviteLoading.value = true
  inviteError.value = null
  invitedPassword.value = null
  try {
    const result = await sf.organizationService.inviteUserToOrganization(orgId.value, {
      email: inviteForm.value.email.trim(),
      username: inviteForm.value.username.trim(),
    })
    if (result.success && result.data) {
      invitedPassword.value = result.data.generatedPassword
      showPasswordModal.value = true
      inviteForm.value = { email: '', username: '' }
      await loadOrganization()
    } else {
      inviteError.value = result.message || t('organization.inviteFailed')
    }
  } catch (e) {
    inviteError.value = t('organization.inviteFailed')
  } finally {
    inviteLoading.value = false
  }
}

function copyPassword() {
  if (invitedPassword.value) {
    navigator.clipboard.writeText(invitedPassword.value)
  }
}

function closePasswordModal() {
  showPasswordModal.value = false
  invitedPassword.value = null
}

onMounted(() => loadOrganization())
watch(orgId, () => loadOrganization())
</script>

<template>
  <div class="p-6 space-y-6">
    <div class="flex items-center gap-4">
      <NuxtLink
        to="/"
        class="text-muted hover:text-foreground"
      >
        ← {{ $t('organization.back') }}
      </NuxtLink>
    </div>

    <div v-if="loading" class="text-sm text-muted">
      {{ $t('common.loading') }}
    </div>

    <div v-else-if="error" class="text-sm text-red-600">
      {{ error }}
    </div>

    <template v-else-if="organization">
      <div>
        <h1 class="text-2xl font-semibold">
          {{ organization.name }}
        </h1>
        <p class="text-sm text-muted">
          {{ $t('organization.organizationId', { id: organization.id }) }}
        </p>
      </div>

      <UCard>
        <template #header>
          <span class="font-medium">{{ $t('organization.inviteUsers') }}</span>
        </template>
        <div class="space-y-4">
          <UFormField :label="$t('auth.email')">
            <UInput
              v-model="inviteForm.email"
              type="email"
              :placeholder="$t('organization.emailPlaceholder')"
              :disabled="inviteLoading"
            />
          </UFormField>
          <UFormField :label="$t('organization.username')">
            <UInput
              v-model="inviteForm.username"
              :placeholder="$t('organization.username')"
              :disabled="inviteLoading"
            />
          </UFormField>
          <p v-if="inviteError" class="text-sm text-red-600">
            {{ inviteError }}
          </p>
          <UButton
            :loading="inviteLoading"
            @click="inviteUser"
          >
            {{ $t('organization.invite') }}
          </UButton>
        </div>
        <template #footer>
          <p class="text-xs text-muted">
            {{ $t('organization.inviteFooter') }}
          </p>
        </template>
      </UCard>

      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <span class="font-medium">{{ $t('organization.users') }}</span>
            <span class="text-sm text-muted">{{ users.length }}</span>
          </div>
        </template>
        <div v-if="users.length === 0" class="text-sm text-muted">
          {{ $t('organization.noUsers') }}
        </div>
        <ul v-else class="divide-y divide-default">
          <li
            v-for="u in users"
            :key="u.id"
            class="py-3 flex items-center justify-between"
          >
            <div>
              <span class="font-medium">{{ u.name }}</span>
              <span class="text-sm text-muted ml-2">{{ u.email }}</span>
            </div>
            <span class="text-sm text-muted">#{{ u.id }}</span>
          </li>
        </ul>
      </UCard>

      <UModal v-model:open="showPasswordModal" :title="$t('organization.passwordGenerated')">
        <template #default>
          <span class="hidden" />
        </template>
        <template #body>
          <p class="text-sm text-muted mb-4">
            {{ $t('organization.passwordGeneratedHint') }}
          </p>
          <div class="flex items-center gap-2">
            <UInput
              :model-value="invitedPassword"
              readonly
              class="font-mono"
            />
            <UButton
              variant="soft"
              @click="copyPassword"
            >
              {{ $t('organization.copy') }}
            </UButton>
          </div>
        </template>
        <template #footer>
          <UButton @click="closePasswordModal">
            {{ $t('organization.close') }}
          </UButton>
        </template>
      </UModal>
    </template>
  </div>
</template>
