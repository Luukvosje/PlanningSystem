/**
 * Frontend-only translation for backend Application/Domain error messages.
 *
 * The .NET backend has no i18n system of its own: every `Result.Failure(...)` message
 * and every domain `ArgumentException` message is a fixed English string (see
 * CLAUDE.md's backend conventions). Rather than introduce resource files and a
 * localization pipeline in the backend for a small, enumerable set of strings, this
 * maps the exact English text to a translation, keyed by the literal message.
 *
 * This is deliberately NOT part of the vue-i18n message tree: the keys are full
 * sentences (with periods and spaces), not stable i18n key paths, and matching is a
 * plain object lookup. Unknown/new backend messages simply fall through untranslated
 * (English) — a new backend message never breaks this, it just isn't translated yet
 * until an entry is added here.
 *
 * FluentValidation's own *default* messages (e.g. "'{Field}' must not be empty.") are
 * NOT handled here — those are localized automatically by the backend via
 * `RequestLocalizationOptions` in Program.cs, driven by the `Accept-Language` header
 * this app sends on every request (see utils/apiClient.ts). Only messages the backend
 * writes explicitly (Result.Failure text, thrown ArgumentException text, custom
 * FluentValidation .WithMessage(...) overrides) need an entry here.
 *
 * One known gap: a handful of backend messages interpolate dynamic data (e.g.
 * `Module '{module}' is disabled at organization level.`) and can't be exact-matched.
 * Those fall back to English too — translating them would require the backend to
 * expose a stable message key instead of pre-formatted text.
 */

const NL_TRANSLATIONS: Record<string, string> = {
  // ASP.NET's default ValidationProblemDetails title — kept in sync with
  // errors.validationFailed in i18n/locales/*.json so Form.ts's "is this just the
  // generic placeholder" check matches regardless of which code path produced it.
  'One or more validation errors occurred.': 'Er zijn een of meer validatiefouten opgetreden.',

  // Team members without an account
  'Only owners and admins can add team members.': 'Alleen eigenaren en beheerders kunnen teamleden toevoegen.',
  'A team member with this email already exists.': 'Er bestaat al een teamlid met dit e-mailadres.',
  'This team member already has an account.': 'Dit teamlid heeft al een account.',
  'Member is already linked to an account.': 'Dit teamlid is al aan een account gekoppeld.',

  // Cross-cutting / auth
  'Organization context is required.': 'Organisatiecontext is vereist.',
  'No organization context. Set the X-Organization-Id header or create an organization first.':
    'Geen organisatiecontext. Stel de X-Organization-Id header in of maak eerst een organisatie aan.',
  'Not authenticated.': 'Niet ingelogd.',
  'Invalid email or password.': 'Ongeldig e-mailadres of wachtwoord.',
  'Refresh token is required.': 'Refresh-token is vereist.',
  'Invalid or expired refresh token.': 'Ongeldige of verlopen refresh-token.',
  'Email is already registered.': 'Dit e-mailadres is al geregistreerd.',
  'Account not found.': 'Account niet gevonden.',

  // Not found
  'User not found.': 'Gebruiker niet gevonden.',
  'Organization not found.': 'Organisatie niet gevonden.',
  'Customer not found.': 'Klant niet gevonden.',
  'Customer not found for organization.': 'Klant niet gevonden voor deze organisatie.',
  'Assigned user not found for organization.': 'Toegewezen medewerker niet gevonden voor deze organisatie.',
  'Planning record not found.': 'Planningsregel niet gevonden.',
  'Employee not found.': 'Medewerker niet gevonden.',
  'Availability rule not found.': 'Beschikbaarheidsregel niet gevonden.',
  'Invite not found.': 'Uitnodiging niet gevonden.',
  'Logo not found.': 'Logo niet gevonden.',

  // Forbidden
  'You are not allowed to view these rules.': 'Je hebt geen toegang om deze regels te bekijken.',
  'You are not allowed to view this availability.': 'Je hebt geen toegang om deze beschikbaarheid te bekijken.',
  'You are not allowed to create this rule.': 'Je hebt geen toegang om deze regel aan te maken.',
  'You are not allowed to update this rule.': 'Je hebt geen toegang om deze regel te wijzigen.',
  'You are not allowed to delete this rule.': 'Je hebt geen toegang om deze regel te verwijderen.',
  'Only owners and admins can change roles.': 'Alleen eigenaren en beheerders kunnen rollen wijzigen.',
  'You are not allowed to decide on requests.': 'Je hebt geen toegang om aanvragen te beoordelen.',
  'Only owners and admins can change who has to request their availability.':
    'Alleen eigenaren en beheerders kunnen instellen wie beschikbaarheid moet aanvragen.',
  'Only owners and admins can create invites.': 'Alleen eigenaren en beheerders kunnen uitnodigingen aanmaken.',
  'Only owners and admins can update organization modules.':
    'Alleen eigenaren en beheerders kunnen organisatiemodules wijzigen.',
  'Only owners and admins can update user modules.': 'Alleen eigenaren en beheerders kunnen gebruikersmodules wijzigen.',

  // Conflict / expired
  'Invite has already been used.': 'Deze uitnodiging is al gebruikt.',
  'Invite has expired.': 'Deze uitnodiging is verlopen.',
  'You are already a member of this organization.': 'Je bent al lid van deze organisatie.',

  // Validation (Application-level)
  'No file uploaded.': 'Geen bestand geüpload.',
  'Logo must be 2 MB or smaller.': 'Logo moet 2 MB of kleiner zijn.',
  'Weekday is required for weekly rules.': 'Weekdag is verplicht voor wekelijkse regels.',
  'Date is required for one-time rules.': 'Datum is verplicht voor eenmalige regels.',
  'Cannot assign the owner role.': 'De eigenaarsrol kan niet worden toegewezen.',
  'Cannot change the owner role.': 'De eigenaarsrol kan niet worden gewijzigd.',
  'You cannot remove your own admin role.': 'Je kunt je eigen beheerdersrol niet verwijderen.',

  // Validation (Domain invariants / ArgumentException)
  'Code is required.': 'Code is verplicht.',
  'Email is required.': 'E-mail is verplicht.',
  'Password hash is required.': 'Wachtwoord-hash is verplicht.',
  'Name is required.': 'Naam is verplicht.',
  'Open time must be before close time.': 'Openingstijd moet vóór sluitingstijd liggen.',
  'A completed or cancelled booking can no longer change status.':
    'Een afgeronde of geannuleerde boeking kan niet meer van status wijzigen.',
  'This status change is not allowed.': 'Deze statuswijziging is niet toegestaan.',
  'Title is required.': 'Titel is verplicht.',
  'End date must be after start date.': 'Einddatum moet na de startdatum liggen.',
  'Duration must be at least 15 minutes.': 'Duur moet minimaal 15 minuten zijn.',
  'Color must be a valid hex color (e.g. #6366F1).': 'Kleur moet een geldige hexkleur zijn (bijv. #6366F1).',
  'End time must be after start time.': 'Eindtijd moet na de starttijd liggen.',
  'Only unavailable rules are supported at this time.': 'Alleen \'niet beschikbaar\'-regels worden nu ondersteund.',
  'This request has already been decided.': 'Deze aanvraag is al afgehandeld.',
  'Only an employee can be required to request availability.':
    'Alleen een medewerker kan beschikbaarheid hoeven aanvragen.',
  'Cannot decide on more than 200 requests at once.':
    'Je kunt niet meer dan 200 aanvragen in één keer beoordelen.',
  'Customer name is required.': 'Klantnaam is verplicht.',
  'Customer email is required.': 'E-mailadres van de klant is verplicht.',
  'Organization id is required.': 'Organisatie-ID is verplicht.',
  'Account id is required.': 'Account-ID is verplicht.',
  'Token hash is required.': 'Token-hash is verplicht.',
  'User id is required.': 'Gebruikers-ID is verplicht.',
  'Organization name is required.': 'Organisatienaam is verplicht.',
  'Organization email is required.': 'E-mailadres van de organisatie is verplicht.',

  // Validation (custom FluentValidation messages)
  'Important work times must use HH:mm format.': 'Belangrijke werktijden moeten het formaat UU:mm hebben.',
  'Important work times must be unique.': 'Belangrijke werktijden mogen niet dubbel voorkomen.',
  'At most 24 important work times are allowed.': 'Maximaal 24 belangrijke werktijden toegestaan.',
  'Open time must use HH:mm format.': 'Openingstijd moet het formaat UU:mm hebben.',
  'Close time must use HH:mm format.': 'Sluitingstijd moet het formaat UU:mm hebben.',
  'Each weekday can only appear once in opening hours.': 'Elke weekdag mag maar één keer voorkomen in de openingstijden.',
};

/**
 * Translate a backend error message for the given app locale. Falls back to the
 * original (English) message when there's no translation or the locale is 'en'.
 */
export function translateBackendMessage(message: string | undefined | null, locale: string): string | undefined | null {
  if (!message) {
    return message;
  }
  if (locale === 'nl') {
    return NL_TRANSLATIONS[message] ?? message;
  }
  return message;
}
