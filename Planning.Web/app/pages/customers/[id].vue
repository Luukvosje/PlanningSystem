<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query'
import { queryKeys } from '~/utils/queryKeys'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const id = computed(() => route.params.id as string)

const auth = useAuthStore()
const customersApi = useCustomersApi()
const queryClient = useQueryClient()
const toast = useToast()
const customerEdit = useCustomerEdit()
const canManage = computed(() => canManageCustomers(auth.currentUser?.role))

onMounted(async () => {
  await auth.fetchMe()

  if (!auth.hasOrganization) {
    await navigateTo('/dashboard')
  }
})

const { data: customer, isLoading, error } = useCustomer(id)
const { message } = useApiError(error)

const showDeleteModal = ref(false)
const isDeleting = ref(false)

async function onDelete() {
  isDeleting.value = true

  try {
    await customersApi.delete(id.value)
    await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all })

    toast.add({ title: 'Klant verwijderd', color: 'success' })
    showDeleteModal.value = false
    await router.push('/customers')
  }
  catch (err) {
    const { message: errMsg } = useApiError(err)
    toast.add({ title: errMsg.value, color: 'error' })
  }
  finally {
    isDeleting.value = false
  }
}
</script>

<template>
  <LayoutPageContainer>
    <UButton to="/customers" variant="ghost" icon="i-lucide-arrow-left" size="sm">
      Terug naar klanten
    </UButton>

    <UiLoadingIndicator v-if="isLoading" label="Klant laden..." />

    <UAlert v-else-if="error" color="error" :title="message" />

    <template v-else-if="customer">
      <div v-if="canManage" class="flex justify-end gap-2">
        <UButton variant="outline" size="sm" icon="i-lucide-pencil" @click="customerEdit.open(customer)">
          Bewerken
        </UButton>
        <UButton
          variant="outline"
          color="error"
          size="sm"
          icon="i-lucide-trash-2"
          @click="() => { showDeleteModal = true }"
        >
          Verwijderen
        </UButton>
      </div>

      <CustomerCard :customer="customer" />

      <UiConfirmModal
        v-model:open="showDeleteModal"
        title="Klant verwijderen"
        description="Weet je zeker dat je deze klant wilt verwijderen? Deze actie kan niet ongedaan worden gemaakt."
        confirm-label="Verwijderen"
        :loading="isDeleting"
        @confirm="onDelete"
      />
    </template>
  </LayoutPageContainer>
</template>
