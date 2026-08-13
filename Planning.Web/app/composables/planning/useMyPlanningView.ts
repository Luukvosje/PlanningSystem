import { useQuery } from '@tanstack/vue-query';
import type { PlanningRecord } from '~/types/planning';
import { getPlanningList } from '~/utils/planningClient';
import { queryKeys } from '~/utils/queryKeys';
import {
  addDays,
  getIntlLocale,
  getMonday,
  isToday,
  toDateKey,
  toUtcDateTimeIso,
} from '~/utils/planning/dateUtils';
import { formatWeekDateRange, getISOWeekNumber } from '~/utils/planning/planningSettings';

export interface MyPlanningDay {
  date: Date
  dateKey: string
  shifts: PlanningRecord[]
  hasShifts: boolean
  isToday: boolean
  isWeekend: boolean
}

function pickDefaultDayKey(days: MyPlanningDay[]): string {
  const today = days.find((day) => day.isToday);
  if (today) {
    return today.dateKey;
  }

  const firstWorkDay = days.find((day) => day.hasShifts);
  return firstWorkDay?.dateKey ?? days[0]?.dateKey ?? '';
}

export function useMyPlanningView() {
  const auth = useAuthStore();
  const { locale } = useI18n();
  const intlLocale = computed(() => getIntlLocale(locale.value));

  const weekStart = ref(getMonday(new Date()));
  const selectedDateKey = ref('');

  const weekEnd = computed(() => addDays(weekStart.value, 7));

  const weekDays = computed(() =>
    Array.from({ length: 7 }, (_, index) => addDays(weekStart.value, index)),
  );

  const userId = computed(() => auth.currentUser?.userId);

  const startUtc = computed(() => toUtcDateTimeIso(weekStart.value));
  const endUtc = computed(() => toUtcDateTimeIso(weekEnd.value));

  const query = useQuery({
    queryKey: computed(() =>
      queryKeys.planning.range(startUtc.value, endUtc.value, `my:${userId.value}`),
    ),
    queryFn: () =>
      getPlanningList({
        startUtc: startUtc.value,
        endUtc: endUtc.value,
        userIds: userId.value,
        pageSize: 500,
      }),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization && !!userId.value),
  });

  const records = computed(() =>
    (query.data.value?.items ?? [])
      .filter((record) => record.status !== 'Cancelled')
      .sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime()),
  );

  const days = computed<MyPlanningDay[]>(() =>
    weekDays.value.map((date) => {
      const dateKey = toDateKey(date);
      const shifts = records.value.filter(
        (record) => toDateKey(new Date(record.startUtc)) === dateKey,
      );

      return {
        date,
        dateKey,
        shifts,
        hasShifts: shifts.length > 0,
        isToday: isToday(date),
        isWeekend: date.getDay() === 0 || date.getDay() === 6,
      };
    }),
  );

  const selectedDay = computed(
    () => days.value.find((day) => day.dateKey === selectedDateKey.value) ?? null,
  );

  watch(
    days,
    (nextDays) => {
      if (!nextDays.length) {
        selectedDateKey.value = '';
        return;
      }

      const stillInWeek = nextDays.some((day) => day.dateKey === selectedDateKey.value);
      if (!stillInWeek) {
        selectedDateKey.value = pickDefaultDayKey(nextDays);
      }
    },
    { immediate: true },
  );

  const weekNumber = computed(() => getISOWeekNumber(weekStart.value));

  const weekLabel = computed(() => `Week ${weekNumber.value}`);

  const weekRangeLabel = computed(() => {
    const lastDay = addDays(weekStart.value, 6);
    return formatWeekDateRange(weekStart.value, lastDay, intlLocale.value);
  });

  const isCurrentWeek = computed(
    () => toDateKey(weekStart.value) === toDateKey(getMonday(new Date())),
  );

  function selectDay(dateKey: string) {
    selectedDateKey.value = dateKey;
  }

  function navigatePrevious() {
    weekStart.value = addDays(weekStart.value, -7);
  }

  function navigateNext() {
    weekStart.value = addDays(weekStart.value, 7);
  }

  function goToToday() {
    weekStart.value = getMonday(new Date());
    selectedDateKey.value = toDateKey(new Date());
  }

  function selectWeekContaining(date: Date) {
    weekStart.value = getMonday(date);
    selectedDateKey.value = toDateKey(date);
  }

  return {
    weekStart,
    weekEnd,
    weekDays,
    weekNumber,
    weekLabel,
    weekRangeLabel,
    isCurrentWeek,
    days,
    selectedDateKey,
    selectedDay,
    records,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    error: query.error,
    refetch: query.refetch,
    selectDay,
    navigatePrevious,
    navigateNext,
    goToToday,
    selectWeekContaining,
  };
}
