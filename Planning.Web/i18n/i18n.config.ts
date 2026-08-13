export default defineI18nConfig(() => ({
  legacy: false,
  fallbackLocale: 'nl',
  datetimeFormats: {
    nl: {
      short: { day: 'numeric', month: 'short' },
      shortWeekday: { weekday: 'short', day: 'numeric', month: 'short' },
      long: { weekday: 'long', day: 'numeric', month: 'long' },
      time: { hour: '2-digit', minute: '2-digit' },
    },
    en: {
      short: { day: 'numeric', month: 'short' },
      shortWeekday: { weekday: 'short', day: 'numeric', month: 'short' },
      long: { weekday: 'long', day: 'numeric', month: 'long' },
      time: { hour: '2-digit', minute: '2-digit' },
    },
  },
}));
