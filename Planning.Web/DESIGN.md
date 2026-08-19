# Ontwerptaal — Planning.Web

Dit beschrijft wat er **daadwerkelijk gebouwd is**, niet wat we ooit van plan waren. De tijdlijn
is de maatstaf: dat is het scherm waar het meeste denkwerk in zit, en de rest is daarnaartoe
getrokken. Wijk je hiervan af, pas dan dit document aan — een spec die de code tegenspreekt is
erger dan geen spec.

## De acht regels

1. **Semantische tokens, bijna dogmatisch.** `bg-default` / `bg-muted` / `bg-elevated`,
   `text-default` / `text-muted` / `text-dimmed` / `text-highlighted`, `border-default`.
   Geen `gray-*`, `slate-*` of `zinc-*` in componenten. De enige uitzonderingen zijn
   statussignalen (rood, amber) en overlays bovenop een door de gebruiker gekozen blokkleur,
   waar een token per definitie niet kan contrasteren.
2. **1px hairlines, hiërarchie via opacity.** `/15`–`/20` voor het fijnste raster, `/40`–`/60`
   voor rij- en dagscheiding, `border-r-2 border-default/70` voor een primaire grens.
   Nooit dikker dan 2px.
3. **Eén radius-ladder.** Nuxt UI koppelt de Tailwind-schaal aan `--ui-radius` (0.375rem):

   | Klasse | Waarde | Waarvoor |
   |---|---|---|
   | `rounded-md` | 9px | bedieningselementen en tijdlijnobjecten: knop, input, blok, badge |
   | `rounded-lg` | 12px | containers: kaart, tabel, paneel |
   | `rounded-xl` | 18px | app-shells en modals |

   Zet geen radius-klasse op een component-slot in `app.config.ts` — dat omzeilt `--ui-radius`
   en is precies hoe knoppen ooit scherper werden dan de blokken ernaast.
4. **`truncate` + `min-w-0` op elke tekstnode.** Bij krappe ruimte verbergen we regels
   progressief in plaats van ze te laten teruglopen.
5. **Geen schaduw op chrome.** Schaduw betekent "zweeft": `shadow-sm` op een blok →
   `hover:shadow-md` → `shadow-lg` tijdens slepen → tooltip en contextmenu. Sticky lagen scheiden
   met `bg-default/95 backdrop-blur`, niet met een schaduw.
6. **Selectie is een ring, geen kleurwissel.** `ring-2 ring-brand ring-offset-1` plus een lichte
   wash. Het object zelf verandert niet van kleur.
7. **Knop-idioom.** Inactief `variant="outline" color="neutral"`; actief `variant="solid"` in de
   accentkleur; in menu's `ghost` → `soft`. De primaire actie laat props wég en erft de default
   uit `nuxt.config.ts`. Icon-only knoppen krijgen altijd een `aria-label`.
8. **Nauwelijks animatie.** Een handvol `transition-colors`/`transition-shadow`, geen
   `duration-*` of `ease-*`. Tijdens slepen wordt de transitie bewust uitgezet.

## Kleur

`nuxt.config.ts` bepaalt welke aliassen bestaan (`ui.theme.colors`), `app.config.ts` koppelt ze
aan een palet:

- **`brand`** = teal — de accentkleur. Exacte tinten in `app/assets/css/main.css`
  (`--color-teal-500`, `--color-teal-600: #006a61`).
- **`neutral`** = slate — draagt alle semantische grijstinten.

> Ze heetten ooit `secondary` en `primary`, met de merkkleur onder "secondary" en grijs onder
> "primary". Daardoor renderde alles wat als `text-primary` geschreven was grijs, inclusief de
> links op de inlogpagina. Nuxt UI kent nog steeds een ingebouwde `primary`-alias waar niets aan
> gekoppeld is — gebruik die niet; hij geeft geen fout maar een ongedefinieerde kleur.

Dark mode wordt volledig door de tokens gedragen en moet werkend blijven.

## Dichtheid

Twee schalen, bewust verschillend:

| | Tijdlijn | Beheer en formulieren |
|---|---|---|
| Rijhoogte | 44px (compact) – 60px | ~45px tabelrij |
| Cel-/celpadding | `px-2 py-1.5` | `px-3 py-2.5` |
| Tekst | 10px micro-labels, `text-xs` koppen | `text-sm` cellen, `text-xs` koppen |
| Chrome eromheen | `gap-4 p-4` | `gap-6` paginaritme |

Een tijdlijn scan je, een formulier vul je in. Beheer is daarom iets ruimer, maar gebruikt
dezelfde tokens en hetzelfde kop-recept (`text-xs font-semibold uppercase tracking-wide text-muted`).

## Gedeelde bouwstenen

Gebruik deze in plaats van het patroon opnieuw te schrijven:

| Component | Waarvoor |
|---|---|
| `LayoutPageContainer` | paginawrapper, `gap-6`-ritme |
| `LayoutPageHeader` | balk boven de content: context links, acties rechts. **Rendert geen titel** — die staat al in de chrome |
| `LayoutCard` | kaart met kop, body en voettekst |
| `UiDataTable` | lijstweergave met zoekfilter, lege staat en klikbare rijen |
| `UiEmptyState` | "hier staat nog niets" |
| `UiQueryState` | fout → laden → content, in die volgorde |
| `UiLoadingIndicator` | losse laadindicator |
| `UiConfirmModal` | bevestiging voor onomkeerbare acties |
| `AvailabilityRuleRow` | één regel in een lijst met bewerk- en verwijderacties |
| `AuthFooterLink` | "nog geen account? Registreren" onder een auth-kaart |

Knopgroottes: de default (`md`) voor pagina-acties in de header, `sm` binnen kaarten en rijen.

## Wat hier bewust níet staat

Een kleurenpalet met vijftig tinten, een typografische schaal met acht niveaus, een
grid-specificatie. Dat hadden we, en de code deed er niets mee. Wat er staat, staat er omdat het
in de app terug te vinden is.
