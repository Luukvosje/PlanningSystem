import type { Ref } from 'vue';
import type {
  CalendarDate } from '@internationalized/date';
import {
  Time,
  fromDate,
  getLocalTimeZone,
  parseDate,
  toCalendarDate,
  toCalendarDateTime,
} from '@internationalized/date';
import { getIntlLocale } from '~/utils/planning/dateUtils';

export interface UseDateTimeRangeOptions {
  /** Move end date to the next day when end time is before start time on the same day. */
  autoAdvanceEndDate?: boolean
}

function calendarDateFromDate(date: Date): CalendarDate {
  return toCalendarDate(fromDate(date, getLocalTimeZone()));
}

function timeFromDate(date: Date): Time {
  return new Time(date.getHours(), date.getMinutes(), 0);
}

function combineDateTime(calendarDate: CalendarDate, time: Time): Date {
  return toCalendarDateTime(calendarDate, time).toDate(getLocalTimeZone());
}

function timeToMinutes(time: Time): number {
  return time.hour * 60 + time.minute;
}

function timeToText(time: Time): string {
  return `${String(time.hour).padStart(2, '0')}:${String(time.minute).padStart(2, '0')}`;
}

/** `parseDate` throws on anything but a full ISO date, and manual typing hits that state. */
function calendarDateFromText(value: string): CalendarDate | null {
  try {
    return parseDate(value);
  } catch {
    return null;
  }
}

function timeFromText(value: string): Time {
  const [hours, minutes] = value.split(':');
  return new Time(Number(hours) || 0, Number(minutes) || 0);
}

export function formatDateTimeRangeDuration(start: Date, end: Date, t: ReturnType<typeof useI18n>['t']): string {
  const ms = end.getTime() - start.getTime();
  if (ms <= 0) {
    return '—';
  }

  const totalMinutes = Math.floor(ms / 60_000);
  const days = Math.floor(totalMinutes / (24 * 60));
  const hours = Math.floor((totalMinutes % (24 * 60)) / 60);
  const minutes = totalMinutes % 60;

  const parts: string[] = [];
  if (days > 0) {
    parts.push(days === 1 ?
      t('dateTimeRange.day', { count: days }) :
      t('dateTimeRange.daysPlural', { count: days }));
  }
  if (hours > 0) {
    parts.push(t('dateTimeRange.hours', { count: hours }));
  }
  if (minutes > 0 && days === 0) {
    parts.push(t('dateTimeRange.minutes', { count: minutes }));
  }
  if (parts.length === 0) {
    return t('dateTimeRange.lessThanOneMinute');
  }
  return parts.join(' ');
}

export function useDateTimeRange(
  externalStart: Ref<Date>,
  externalEnd: Ref<Date>,
  options: UseDateTimeRangeOptions = {},
) {
  const { autoAdvanceEndDate = true } = options;
  const { t, locale } = useI18n();

  const startDate = shallowRef(calendarDateFromDate(externalStart.value));
  const startTime = shallowRef(timeFromDate(externalStart.value));
  const endDate = shallowRef(calendarDateFromDate(externalEnd.value));
  const endTime = shallowRef(timeFromDate(externalEnd.value));
  const endDateFollowsStart = ref(startDate.value.compare(endDate.value) === 0);

  function syncFromExternal() {
    const newStart = externalStart.value;
    const newEnd = externalEnd.value;
    if (Number.isNaN(newStart.getTime()) || Number.isNaN(newEnd.getTime())) {
      return;
    }

    const currentStartMs = combineDateTime(startDate.value, startTime.value).getTime();
    const currentEndMs = combineDateTime(endDate.value, endTime.value).getTime();
    if (currentStartMs === newStart.getTime() && currentEndMs === newEnd.getTime()) {
      return;
    }

    startDate.value = calendarDateFromDate(newStart);
    startTime.value = timeFromDate(newStart);
    endDate.value = calendarDateFromDate(newEnd);
    endTime.value = timeFromDate(newEnd);
    endDateFollowsStart.value = startDate.value.compare(endDate.value) === 0;
  }

  watch([externalStart, externalEnd], syncFromExternal);

  function commit() {
    externalStart.value = combineDateTime(startDate.value, startTime.value);
    externalEnd.value = combineDateTime(endDate.value, endTime.value);
  }

  function onStartDateChange(value: CalendarDate) {
    startDate.value = value;
    if (endDateFollowsStart.value) {
      endDate.value = value;
    }
    commit();
  }

  function onStartTimeChange(value: Time) {
    startTime.value = value;
    commit();
  }

  function onEndDateChange(value: CalendarDate) {
    endDate.value = value;
    endDateFollowsStart.value = value.compare(startDate.value) === 0;
    commit();
  }

  function onEndTimeChange(value: Time) {
    endTime.value = value;
    if (
      autoAdvanceEndDate &&
      endDate.value.compare(startDate.value) === 0 &&
      timeToMinutes(value) <= timeToMinutes(startTime.value)
    ) {
      endDate.value = startDate.value.add({ days: 1 });
      endDateFollowsStart.value = false;
    }
    commit();
  }

  function applyNextDayEnd() {
    endDate.value = startDate.value.add({ days: 1 });
    endDateFollowsStart.value = false;
    commit();
  }

  const start = computed(() => combineDateTime(startDate.value, startTime.value));
  const end = computed(() => combineDateTime(endDate.value, endTime.value));

  const startTimeModel = computed({
    get: () => startTime.value,
    set: onStartTimeChange,
  });

  /** ISO date (`YYYY-MM-DD`), the shape `ControlsDateSelectInput` binds to. */
  const startDateModel = computed({
    get: () => startDate.value.toString(),
    set: (value: string) => {
      const parsed = calendarDateFromText(value);
      if (parsed) {
        onStartDateChange(parsed);
      }
    },
  });

  const endTimeModel = computed({
    get: () => endTime.value,
    set: onEndTimeChange,
  });

  const endDateModel = computed({
    get: () => endDate.value.toString(),
    set: (value: string) => {
      const parsed = calendarDateFromText(value);
      if (parsed) {
        onEndDateChange(parsed);
      }
    },
  });

  /** `HH:mm` views on both ends, so quick picks can read and write a plain time. */
  const startTimeText = computed(() => timeToText(startTime.value));
  const endTimeText = computed(() => timeToText(endTime.value));

  function applyStartTime(value: string) {
    onStartTimeChange(timeFromText(value));
  }

  function applyEndTime(value: string) {
    onEndTimeChange(timeFromText(value));
  }

  const isMultiDay = computed(() => startDate.value.compare(endDate.value) !== 0);

  const multiDayLabel = computed(() => {
    if (!isMultiDay.value) {
      return null;
    }
    const formatter = new Intl.DateTimeFormat(getIntlLocale(locale.value), {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
    });
    return `${formatter.format(start.value)} → ${formatter.format(end.value)}`;
  });

  const durationLabel = computed(() => formatDateTimeRangeDuration(start.value, end.value, t));

  const validationError = computed(() => {
    if (end.value.getTime() <= start.value.getTime()) {
      return t('dateTimeRange.endAfterStart');
    }
    return null;
  });

  const isValid = computed(() => validationError.value === null);

  const suggestsNextDay = computed(() => {
    if (autoAdvanceEndDate) {
      return false;
    }
    if (endDate.value.compare(startDate.value) !== 0) {
      return false;
    }
    return timeToMinutes(endTime.value) <= timeToMinutes(startTime.value);
  });

  return {
    start,
    end,
    startTime: startTimeModel,
    startDate: startDateModel,
    endTime: endTimeModel,
    endDate: endDateModel,
    startTimeText,
    endTimeText,
    applyStartTime,
    applyEndTime,
    isMultiDay,
    multiDayLabel,
    durationLabel,
    validationError,
    isValid,
    suggestsNextDay,
    applyNextDayEnd,
  };
}
