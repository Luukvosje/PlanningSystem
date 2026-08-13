import { isApiError } from '~/types/api-error';

export function useApiError(error: Ref<unknown> | unknown) {
  const { t } = useI18n();
  const errorRef = isRef(error) ? error : ref(error);

  const apiError = computed(() => {
    const value = errorRef.value;

    if (isApiError(value)) {
      return value;
    }

    return {
      status: 0,
      message: t('errors.unknown'),
      code: 'UNKNOWN' as const,
    };
  });

  return {
    apiError,
    message: computed(() => apiError.value.message),
    validationErrors: computed(() => apiError.value.validationErrors),
  };
}
