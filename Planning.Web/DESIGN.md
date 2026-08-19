# Ontwerptaal — Planning.Web

Dit beschrijft wat er **daadwerkelijk gebouwd is**, niet wat we ooit van plan waren. De tijdlijn
is de maatstaf: dat is het scherm waar het meeste denkwerk in zit, en de rest is daarnaartoe
getrokken. Wijk je hiervan af, pas dan dit document aan — een spec die de code tegenspreekt is
erger dan geen spec.

## De negen regels

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
9. **Geen kaarten binnen een pagina.** De content-area van de app-shell is al een omrande,
   afgeronde panel. Een `LayoutCard` daarbinnen is een doos in een doos: twee randen, twee radii,
   dubbele padding, zonder dat er iets extra's gegroepeerd wordt. Blokken op een pagina scheid je
   met een `LayoutSection` — kop, omschrijving, `gap-6` ertussen. `LayoutCard` blijft alleen voor
   wat écht op zichzelf zweeft: de auth-schermen en de dashboard-tegels.

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

## Pagina-anatomie

De app-chrome (`layouts/default.vue`) is de enige plek met een paginakop. Een pagina rendert
daarin via slots en zet er geen tweede balk onder.

| Slot | Wat erin gaat |
|---|---|
| `#title` | alleen als de standaardtitel niet klopt — op detailpagina's een `UBreadcrumb` |
| `#actions` | de acties van de pagina: aanmaken, verwijderen, periodenavigatie |

Een pagina die een slot vult zet `definePageMeta({ layout: false })` en wikkelt zichzelf in
`<NuxtLayout name="default">`. Pagina's zonder acties laten dat achterwege en houden de
standaardkop.

### Lijstpagina

`UiDataTable` in een `LayoutPageContainer fill`, zoekveld erboven, de primaire actie
("Klant toevoegen") in `#actions` achter een rechtencheck. Een rij klikt door naar de detailpagina.

### Detailpagina

- **Titel is een kruimelpad**: `Klanten › Acme B.V.`, waarvan alleen de eerste kruimel klikbaar
  is. Tijdens het laden staat er `Laden...` in plaats van een lege kruimel. Geen losse
  terug-knop — dat is wat het kruimelpad al doet.
- **Je ziet altijd dezelfde sectie.** De detailpagina opent in leesweergave. `Bewerken` staat in
  `#actions` en wisselt de inhoud van diezelfde sectie om naar het formulier — de kop met de naam
  blijft staan, de pagina springt niet. Opslaan of annuleren brengt je terug naar de leesweergave.
  Geen bewerk-modal: die verbergt de context die je net aan het lezen was.
- Een detailcomponent (`components/customer/Details.vue`) geeft daarvoor een default slot met de
  leesweergave als fallback. De pagina vult dat slot alleen tijdens het bewerken.
- **Zonder beheerrechten** verschijnt de `Bewerken`-knop niet en blijft het bij lezen. Een
  invoerveld tonen dat bij opslaan een 403 oplevert is erger dan het veld niet tonen.
- **Verwijderen** staat in `#actions` en gaat via `UiConfirmModal`, niet in het formulier.
- Meerdere kaarten naast elkaar in een `grid grid-cols-1 gap-6 lg:grid-cols-2`.

Dit geldt voor elke entiteit met een detailpagina — klanten, gebruikers, en wat er nog bij komt.
Modals blijven over voor **aanmaken** (`useCreate`), waar nog geen record is om naartoe te
navigeren.

### Formulieren

**Een `useForm(...)`-definitie staat nooit in een `.vue`-bestand.** Elk formulier krijgt een
composable in `app/composables/forms/use<Iets>Form.ts` die het schema, de controls, `onSubmit`
en het spiegelen van server-data naar `form.state` bevat en de `Form` teruggeeft. De pagina
roept die composable aan en beslist alleen nog *wanneer* het formulier zichtbaar is.

```ts
const form = useCustomerForm(customer, { onSaved: () => { isEditing.value = false } })
```

Wat er in die composable hoort: het schema, de controls, `onSubmit` inclusief cache-invalidatie,
toasts en navigatie, en het spiegelen van server-data naar `form.state`. Wat de pagina houdt:
laadstatus, rechten, en de vraag wanneer het formulier zichtbaar is. Alle formulieren staan er —
zie `app/composables/forms/`.

Zo blijft een pagina leesbaar als een pagina: data laden, staat kiezen, renderen. `grid: true`
levert de label-links/veld-rechts opmaak en de route-leave-guard.

De knoppen horen in de **`#footer`-slot van `form.render`**, niet in een slot van de omliggende
container. Alleen daar staan ze binnen het `<form>`-element, en alleen daar doet `type="submit"`
wat het belooft. Buiten het formulier is de opslaan-knop een knop die nergens op aangesloten is.

## Gedeelde bouwstenen

Gebruik deze in plaats van het patroon opnieuw te schrijven:

| Component | Waarvoor |
|---|---|
| `LayoutPageContainer` | paginawrapper, `gap-6`-ritme |
| `LayoutSection` | blok binnen een pagina: kop, omschrijving, `#actions`, content. De standaard — zie regel 9 |
| `LayoutSectionHeader` | alleen de kop van zo'n blok, of een losse omschrijving boven een pagina. **Rendert nooit de paginatitel** — die staat al in de chrome |
| `LayoutCard` | alleen wat op zichzelf zweeft: auth-schermen, dashboard-tegels |
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
