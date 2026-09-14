export type LanguageCode = 'nl' | 'en';

export interface LanguageOption {
  code: LanguageCode
  label: string
  icon: string
}

/** The two locales from nuxt.config.ts, with the flag each one is shown with. */
export const LANGUAGE_OPTIONS: readonly LanguageOption[] = [
  { code: 'nl', label: 'Nederlands', icon: 'i-circle-flags-nl' },
  { code: 'en', label: 'English', icon: 'i-circle-flags-gb' },
];
