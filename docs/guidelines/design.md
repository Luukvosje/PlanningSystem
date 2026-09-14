# Ontwerptaal — Planning.Web

Dit beschrijft wat er **daadwerkelijk gebouwd is**, niet wat we ooit van plan waren. De tijdlijn
is de maatstaf: dat is het scherm waar het meeste denkwerk in zit, en de rest is daarnaartoe
getrokken. Wijk je hiervan af, pas dan dit document aan — een spec die de code tegenspreekt is
erger dan geen spec.

Code-conventies (data-flow, formulier-composables, query-keys) staan in
[frontend.md](frontend.md). Hier gaat het alleen over hoe het eruitziet.

## De negen regels

1. **Semantische tokens, bijna dogmatisch.** `bg-default` / `bg-muted` / `bg-elevated` /
   `bg-accented`, `text-default` / `text-muted` / `text-dimmed` / `text-toned` /
   `text-highlighted`, `border-default`. Geen `gray-*`, `slate-*` of `zinc-*` in componenten —
   die staan er op dit moment nul keer. De enige uitzonderingen zijn statussignalen (de rode
   punt van `CurrentTimeIndicator`) en overlays bovenop een door de gebruiker gekozen
   blokkleur, waar een token per definitie niet kan contrasteren.
2. **1px hairlines, hiërarchie via opacity.** `/8`–`/25` voor het fijnste raster, `/40`–`/70`
   voor rij- en dagscheiding. Een primaire grens is het enige dat 2px mag zijn:
   `border-r-2 border-default/70` tussen de resourcekolom en het raster. Nooit dikker.
3. **Eén radius-ladder.** `main.css` zet `--ui-radius: 0.375rem` (6px, waar Nuxt UI zelf
   0.25rem gebruikt) en Nuxt UI leidt de hele Tailwind-schaal daaruit af — `sm` = ×1,
   `md` = ×1.5, `lg` = ×2, `xl` = ×3:

   | Klasse | Waarde | Waarvoor |
   |---|---|---|
   | `rounded-sm` | 6px | het kleinste grut: sleeptooltip, beschikbaarheidsoverlay, badge in een label |
   | `rounded-md` | 9px | bedieningselementen en tijdlijnobjecten: knop, input, blok |
   | `rounded-lg` | 12px | containers: kaart, tabel, contextmenu |
   | `rounded-xl` | 18px | app-shells, modals, het nav-paneel |
   | `rounded-full` | — | alleen echte cirkels: kleurkeuze, statusstip, dag-pil |

   Zet geen radius-klasse op een component-slot in `app.config.ts` — dat omzeilt `--ui-radius`
   en is precies hoe knoppen ooit scherper werden dan de blokken ernaast.
4. **`truncate` + `min-w-0` op elke tekstnode.** Bij krappe ruimte verbergen we regels
   progressief in plaats van ze te laten teruglopen. `timeline/Block.vue` is het voorbeeld:
   vijf tekstnodes, alle vijf `min-w-0 … truncate shrink-0`.
5. **Wat zweeft is glas; schaduw zegt "je kunt me oppakken".** Zie [Glas](#glas) hieronder.
   Schaduw is daarnaast bewust schaars en betekent precies één ding — dit object ligt los op
   het raster: `shadow-sm` op een blok, `hover:shadow-md`, `shadow-lg` tijdens slepen. Verder
   alleen op chrome die echt over de pagina hangt (contextmenu, uitgeklapt nav-paneel) en als
   `shadow-sm ring ring-default` op de app-shell zelf.
6. **Selectie is een ring, geen kleurwissel.** `ring-2 ring-brand ring-offset-1` plus z-index.
   Het object zelf verandert niet van kleur. Een droptarget krijgt de zachte variant:
   `ring-1 ring-inset ring-brand/40` met een `bg-brand/10` wash.
7. **Knop-idioom.** Inactief `variant="outline"`; in menu's en op iconen `ghost`. `subtle` is
   de derde vaste variant en staat voor "informatief, niet aanklikbaar-belangrijk": badges,
   statuslabels, de alert-default. De primaire actie laat props wég en erft `solid` in de
   accentkleur uit `nuxt.config.ts` — daarom zie je nergens een expliciete
   `variant="solid"`. Icon-only knoppen krijgen altijd een `aria-label`.
8. **Animatie alleen waar iets van plaats verandert.** Een handvol `transition-colors` en
   `transition-shadow` zonder `duration-*`, plus `transition-transform` op de twee dingen die
   opschalen bij hover. De enige plek met expliciete timing is het uitklappende nav-paneel in
   `layouts/default.vue` (`duration-150 ease-out` in, `duration-100 ease-in` uit) — en die
   staat achter `motion-reduce:transition-none`. Tijdens slepen wordt de transitie bewust
   uitgezet.
9. **Kaarten zijn er om in te typen, niet om mee te groeperen.** De content-area van de
   app-shell is al een omrande, afgeronde panel; een `LayoutCard` om een lijst of een grafiek
   is een doos in een doos — twee randen, twee radii, dubbele padding. Dus:

   - **`LayoutSection`** voor alles wat je alleen leest: lijsten, tegels, lege staten. Kop,
     omschrijving, `gap-6` ertussen, geen rand.
   - **`LayoutCard`** voor alles waar je in typt — dat is wat `FormEditableSection` eromheen
     zet — en voor wat écht op zichzelf zweeft: de auth-schermen en de dashboard-tegels.

   Deze regel is ooit strenger geformuleerd ("geen kaarten binnen een pagina"). Dat klopte niet
   meer zodra formulieren de kaart kregen; de doc-comment in `layout/Section.vue` houdt de
   huidige formulering.

## Glas

Alles wat over de pagina hangt is uit hetzelfde materiaal gesneden, gedefinieerd in `main.css`:

| Token | Wat het is |
|---|---|
| `--glass-bg` | de vulling: `color-mix` van `--ui-bg` met transparant, 62% licht / 72% dark |
| `--glass-filter` | `blur(20px) saturate(180%) brightness(105%)`; dark: 24px / 150% / 115% |
| `--overlay-blur` | 6px — waas over de pagina die een scrim bedekt |

Drie utilities, en welke je pakt hangt af van waar het op ligt:

| Utility | Gebruik |
|---|---|
| `glass` | een zwevend paneel: eigen vulling **plus** materiaal. Het nav-paneel, de tijdlijn-ruler |
| `glass-fill` | alleen vulling, voor iets dat op een paneel staat dat al blurt — de headercellen van de resourcekolom, die daardoor op ~86% uitkomen en als één strook onder de ruler doorlopen |
| `glass-material` | alleen materiaal, voor een Nuxt UI-slot. De vulling moet daar als `bg-[var(--glass-bg)]` geschreven worden: tailwind-merge herkent dát als background en gooit de eigen `bg-default` van het component weg, terwijl het `glass` niet kent en beide vullingen laat staan |

Modal, slideover, popover, select, selectMenu, inputMenu, dropdownMenu en contextMenu zijn in
`app.config.ts` allemaal op `glass-material` gezet. Hun scrim is `bg-elevated/25
backdrop-blur-(--overlay-blur)` — bewust licht, want bij `/75` is er niets meer achter het
paneel te zien en is het glas zinloos.

Saturatie staat hoog omdat blur kleur uitwast, en de gekleurde blokken die onder de ruler
doorschuiven zijn precies wat je wilt blijven herkennen terwijl je er een versleept.

`@media (prefers-reduced-transparency: reduce)` zet alles terug naar volledig dekkend. Dat
mag niet sneuvelen.

## Kleur

`nuxt.config.ts` bepaalt welke aliassen bestaan (`ui.theme.colors`), `app.config.ts` koppelt ze
aan een palet:

- **`brand`** = teal — de accentkleur, en de default voor elk Nuxt UI-component. Exacte tinten
  worden in `main.css` overschreven: `--color-teal-500: #0d9488`, `--color-teal-600: #006a61`.
- **`neutral`** = gray — draagt alle semantische grijstinten.

> Ze heetten ooit `secondary` en `primary`, met de merkkleur onder "secondary" en grijs onder
> "primary". Daardoor renderde alles wat als `text-primary` geschreven was grijs, inclusief de
> links op de inlogpagina. Nuxt UI kent nog steeds een ingebouwde `primary`-alias waar niets aan
> gekoppeld is — gebruik die niet; hij geeft geen fout maar een ongedefinieerde kleur.

`--ui-border` wordt ongelayerd overschreven (neutral-300 licht, neutral-700 dark). Dat moet
buiten een `@layer` blijven staan: Nuxt UI declareert het in `@layer theme`, en een override
vanuit een layer wint alleen vanaf een latere layer.

Dark mode wordt volledig door de tokens gedragen en moet werkend blijven.

## Dichtheid

Twee schalen, bewust verschillend.

**Tijdlijn** — de rijhoogte volgt uit de laanhoogte (`utils/planning/timelineMath.ts`) plus
2 × `BLOCK_PADDING` (4px). Een rij met overlappende diensten stapelt lanen:

| Modus | Laanhoogte | Rij met één laan |
|---|---|---|
| `compact` | 36px | 44px |
| default | 52px | 60px |
| default op detail-zoom (`15m`–`day`) | 80px | 88px |
| `spacious` | 96px | 104px |

Blokpadding is `px-2 py-0.5`, in `spacious` `py-2`. Tekst in een blok: `text-xs font-semibold`
voor de titel, `text-[10px]` voor alles daaronder.

**Beheer en formulieren** — de tabel is in `app.config.ts` dichter gezet dan de Nuxt
UI-default (`th px-4 py-3.5` / `td p-4`): `th px-3 py-2`, `td px-3 py-4 text-sm text-muted`,
wat op ~44px rijhoogte uitkomt in plaats van ~56. Kaartslots zijn `p-3 sm:p-4`. Het
paginaritme is `gap-6`.

Een tijdlijn scan je, een formulier vul je in. Beheer is daarom iets ruimer, maar gebruikt
dezelfde tokens en hetzelfde kop-recept: `text-xs font-semibold uppercase tracking-wide
text-muted` — dat is letterlijk de `th` van de tabel, en het staat ook boven de secties in het
dashboard, de zijbalk en het persoonlijke weekoverzicht.

## Pagina-anatomie

De app-chrome (`layouts/default.vue`) is de enige plek met een paginakop. Een pagina rendert
daarin via drie slots en zet er geen tweede balk onder.

| Slot | Wat erin gaat |
|---|---|
| `#title` | alleen als de standaardtitel niet klopt — op detailpagina's een `UBreadcrumb` |
| `#actions` | de acties van de pagina: aanmaken, verwijderen, periodenavigatie |
| `#tabs` | een `LayoutPageTabs`, direct onder de kop |

Een pagina die een slot vult zet `definePageMeta({ layout: false })` en wikkelt zichzelf in
`<NuxtLayout name="default">`. Pagina's zonder acties laten dat achterwege en houden de
standaardkop.

### Lijstpagina

`UiDataTable` in een `LayoutPageContainer fill`, zoekveld erboven, de primaire actie
("Klant toevoegen") in `#actions` achter een rechtencheck. Een rij klikt door naar de
detailpagina. `LayoutPageContainer` knoopt zich alleen bij `fill` aan de vensterhoogte — zonder
dat zou een langere kaart eruit hangen en zijn `ring` (een outset box-shadow) op de scrollrand
weggeknipt worden.

### Detailpagina

- **Titel is een kruimelpad**: `Klanten › Acme B.V.`, waarvan alleen de eerste kruimel klikbaar
  is. Tijdens het laden staat er `Laden...` in plaats van een lege kruimel. Geen losse
  terug-knop — dat is wat het kruimelpad al doet.
- **Tabs verdelen de pagina**, niet kaarten naast elkaar: overzicht / gegevens / planning. Elke
  tab is een ander deel van de pagina, geen panel — vandaar `:content="false"` in
  `LayoutPageTabs`.
- **De gegevens-tab ís het formulier.** `FormEditableSection` toont het levende formulier aan
  wie mag bewerken en `FormDisplay` met dezelfde velden aan wie niet. Geen leesmodus met een
  Bewerken-knop ertussen, geen bewerk-modal: die verbergt de context die je net aan het lezen
  was, en een wisselstap kost een klik voor iets dat je toch al mag. De `Bewerken`-knop die op
  de klantpagina in `#actions` staat is dan ook geen modus-schakelaar maar een snelkoppeling
  naar die tab, en verdwijnt zodra je er bent.
- **Zonder rechten** blijft het bij lezen. Een invoerveld tonen dat bij opslaan een 403
  oplevert is erger dan het veld niet tonen; `can-edit` komt uit `utils/userRole.ts`.
- **Verwijderen** staat in `#actions` en gaat via `UiConfirmModal` — daar omdat de klantpagina
  voortgang wil tonen terwijl het verwijderen loopt. Elders is de standaard de
  `UiDeleteConfirm` die één keer in `app.vue` hangt. Er wordt niets verwijderd zonder te vragen.

Dit geldt voor elke entiteit met een detailpagina — klanten, gebruikers, de organisatie, en wat
er nog bij komt. Modals blijven over voor **aanmaken** (`useCreate`), waar nog geen record is om
naartoe te navigeren.

### Formulieren

**Een formulierdefinitie staat nooit in een `.vue`-bestand.** Waar hij wél staat hangt af van
hoe hij verschijnt:

| Composable | Map | Verschijnt als |
|---|---|---|
| `useForm` | `composables/forms/` | wat de pagina met `form.render` doet |
| `useEdit` | `composables/edit/` | een `FormEditableSection` |
| `useCreate` | `composables/create/` | een `FormCreateModal` via de Nuxt UI-overlay |

Wat er in zo'n definitie hoort: het zod-schema, de controls, `onSubmit` inclusief
cache-invalidatie, toasts en navigatie, en voor `useEdit` de `toState` die de entiteit op het
formulier afbeeldt. Wat de pagina houdt: laadstatus, rechten, en de vraag wanneer het formulier
zichtbaar is.

Zo blijft een pagina leesbaar als een pagina: data laden, staat kiezen, renderen. `grid: true`
levert de label-links/veld-rechts opmaak en de route-leave-guard.

De knoppen horen in de **`#footer`-slot van `form.render`**, niet in een slot van de omliggende
container. Alleen daar staan ze binnen het `<form>`-element, en alleen daar doet `type="submit"`
wat het belooft. Buiten het formulier is de opslaan-knop een knop die nergens op aangesloten is.

## Gedeelde bouwstenen

Gebruik deze in plaats van het patroon opnieuw te schrijven:

| Component | Waarvoor |
|---|---|
| `LayoutPageContainer` | paginawrapper, `gap-6`-ritme; `fill` voor volledige hoogte |
| `LayoutSection` | leesblok binnen een pagina: kop, omschrijving, `#actions`, content — zie regel 9 |
| `LayoutSectionHeader` | alleen die kop, of een losse omschrijving. **Rendert nooit de paginatitel** — die staat al in de chrome |
| `LayoutCard` | iets waar je in typt, of wat op zichzelf zweeft: auth-schermen, dashboard-tegels |
| `LayoutPageTabs` | de tabrij onder de paginakop |
| `LayoutHeaderActions` | een `#actions`-groep die uit een `HeaderAction[]` wordt opgebouwd en onder 1024px inklapt tot een dropdown. Nu alleen op de planningspagina, die er de meeste heeft |
| `FormEditableSection` | de standaard voor de velden van een entiteit: formulier of leesweergave, één kaart |
| `FormDisplay` | leesweergave van dezelfde controls, los te gebruiken voor velden die de API niet bijwerkt |
| `FormCreateModal` | aanmaken, geopend door `useCreate` |
| `UiDataTable` | lijstweergave met zoekfilter, lege staat en klikbare rijen |
| `UiQueryState` | fout → laden → content, in die volgorde |
| `UiEmptyState` | "hier staat nog niets" |
| `UiLoadingIndicator` | losse laadindicator |
| `UiStatTile` | één getal op een detailpagina — `rounded-lg p-4 ring ring-default`, geen kaart. Zonder `value` een streepje plus placeholder, zodat een onaffe KPI-rij bewust oogt |
| `UiEntitySelect` | de kiezer voor élke soort entiteit — medewerker, klant, wat er nog bij komt. Zoeken, actief/inactief en A-Z/Recent zitten hier één keer; wát een entiteit is staat in `useEntitySource` |
| `UiConfirmModal` | bevestiging met eigen voortgang, voor een component dat die zelf wil sturen |
| `UiDeleteConfirm` | de ene verwijderdialoog, hangt in `app.vue`, aangeroepen via `useDeleteConfirm` |
| `AvailabilityRuleRow` | één regel in een lijst met bewerk- en verwijderacties |
| `AuthFooterLink` | "nog geen account? Registreren" onder een auth-kaart |

Knopgroottes: de default (`md`) voor pagina-acties in de header, `sm` binnen kaarten en rijen.

## Wat hier bewust níet staat

Een kleurenpalet met vijftig tinten, een typografische schaal met acht niveaus, een
grid-specificatie. Dat hadden we, en de code deed er niets mee. Wat er staat, staat er omdat het
in de app terug te vinden is.
