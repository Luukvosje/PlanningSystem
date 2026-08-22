import OrganizationsImportantWorkTimesInput from '~/components/organizations/ImportantWorkTimesInput.vue';
import OrganizationsOpeningHoursInput, { type OpeningHoursRow } from '~/components/organizations/OpeningHoursInput.vue';
import { createPlanningSettingsSchema } from '~/schemas/planningSettings.schema';
import { getWeekdayOptions } from '~/types/availability';
import { getDefaultImportantWorkTimes, normalizeTimeValue } from '~/utils/planning/planningSettings';

export function usePlanningSettingsForm() {
  const { data: organization, isLoading } = useCurrentOrganization();
  const { updatePlanningSettings } = useOrganizationSettingsApi();
  const { t } = useI18n();

  const weekdayOptions = computed(() => getWeekdayOptions(t));

  function defaultOpeningHours(): OpeningHoursRow[] {
    return weekdayOptions.value.map((option) => ({
      day: option.value,
      enabled: false,
      openTime: '06:00',
      closeTime: '22:00',
    }));
  }

  const form = useForm({
    schema: createPlanningSettingsSchema(t),
    initialState: {
      importantWorkTimes: getDefaultImportantWorkTimes(),
      openingHours: defaultOpeningHours(),
    },
    controls: computed(() => [
      {
        name: 'openingHours',
        label: t('organizations.planningSettings.openingHours'),
        description: t('organizations.planningSettings.openingHoursDescription'),
        component: OrganizationsOpeningHoursInput,
      },
      {
        name: 'importantWorkTimes',
        label: t('organizations.planningSettings.importantTimes'),
        description: t('organizations.planningSettings.importantTimesDescription'),
        component: OrganizationsImportantWorkTimesInput,
      },
    ]),
    grid: true,
    onSubmit: async (data) => {
      await updatePlanningSettings.mutateAsync({
        importantWorkTimes: data.importantWorkTimes.map((row) => ({
          label: row.label.trim() || null,
          startTime: normalizeTimeValue(row.startTime),
        })),
        openingHours: data.openingHours
          .filter((row) => row.enabled)
          .map((row) => ({
            day: row.day,
            openTime: normalizeTimeValue(row.openTime),
            closeTime: normalizeTimeValue(row.closeTime),
          })),
      });
    },
  });

  watch(
    organization,
    (org) => {
      if (!org) {
        return;
      }

      form.state.importantWorkTimes = org.importantWorkTimes?.length ?
        org.importantWorkTimes.map((entry) => ({
          label: entry.label ?? '',
          startTime: normalizeTimeValue(entry.startTime),
        })) :
        getDefaultImportantWorkTimes();

      form.state.openingHours = weekdayOptions.value.map((option) => {
        const existing = org.openingHours?.find((entry) => entry.day === option.value);
        return {
          day: option.value,
          enabled: !!existing,
          openTime: normalizeTimeValue(existing?.openTime ?? '06:00'),
          closeTime: normalizeTimeValue(existing?.closeTime ?? '22:00'),
        };
      });

      form.markClean();
    },
    { immediate: true },
  );

  return { form, isLoading };
}
