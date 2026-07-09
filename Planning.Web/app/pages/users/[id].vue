<script setup lang="ts">
definePageMeta({ layout: 'default' })

const route = useRoute()
const id = computed(() => route.params.id as string)

const { data: user, isLoading, error } = useUser(id)
const { message } = useApiError(error)
</script>

<template>
  <LayoutPageContainer>
    <UButton to="/users" variant="ghost" icon="i-lucide-arrow-left" size="sm">
      Terug naar team
    </UButton>

    <USkeleton v-if="isLoading" class="h-32 w-full" />

    <UAlert v-else-if="error" color="error" :title="message" />

    <UsersCard v-else-if="user" :user="user" />
  </LayoutPageContainer>
</template>
