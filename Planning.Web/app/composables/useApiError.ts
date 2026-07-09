import { isApiError } from '~/types/api-error'

export function useApiError(error: Ref<unknown> | unknown) {
  const errorRef = isRef(error) ? error : ref(error)

  const apiError = computed(() => {
    const value = errorRef.value

    if (isApiError(value)) {
      return value
    }

    return {
      status: 0,
      message: 'Onbekende fout',
      code: 'UNKNOWN' as const,
    }
  })

  return {
    apiError,
    message: computed(() => apiError.value.message),
    validationErrors: computed(() => apiError.value.validationErrors),
  }
}
