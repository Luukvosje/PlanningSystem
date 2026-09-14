# Planning SaaS - MVP Specificatie

## Doel

Bouw een eenvoudige planning SaaS waarmee kleine bedrijven (zoals sportscholen) eenvoudig medewerkers kunnen inplannen.

De eerste doelgroep werkt momenteel met:
- Microsoft Word
- WhatsApp
- Excel (optioneel)

Het doel is **niet** om direct een volledige HR-oplossing te bouwen, maar om het huidige proces aanzienlijk eenvoudiger te maken.

---

# Doelgroep

## Eerste klant

Sportschool

Probleem:
- Planning wordt gemaakt in Word.
- Planning wordt gedeeld via WhatsApp.
- Wijzigingen kosten veel tijd.
- Er is geen centraal overzicht.
- Iedereen werkt met verschillende versies.

Doel:

> Binnen 5 minuten een complete weekplanning kunnen maken en direct kunnen delen.

---

# MVP Functionaliteiten

## 1. Medewerkers

Een medewerker bevat minimaal:

- id
- naam
- telefoonnummer
- kleur
- actief/inactief

Voorbeeld:

```text
Kevin
+31 6 12345678
Blauw
Actief
```

---

## 2. Diensten (Shifts)

Een dienst bestaat uit:

- datum
- begintijd
- eindtijd
- medewerker
- locatie (optioneel)
- notitie

Voorbeeld:

```text
Maandag

08:00 - 12:00 Kevin
12:00 - 17:00 Lisa
17:00 - 22:00 Tom
```

---

## 3. Weekplanning

Dit is het belangrijkste scherm van de applicatie.

Weergave:

```text
Ma | Di | Wo | Do | Vr | Za | Zo
```

Per dag worden alle diensten weergegeven.

Gewenste interacties:

- Nieuwe dienst toevoegen
- Dienst aanpassen
- Dienst verwijderen
- Dienst slepen naar andere dag
- Medewerker wijzigen
- Tijd wijzigen

Drag & Drop is de standaard interactie.

---

## 4. Beschikbaarheid

Een medewerker kan aangeven wanneer hij beschikbaar is.

Bijvoorbeeld:

```text
Maandag

✓ Ochtend
✓ Middag
✗ Avond
```

Of:

```text
Vrijdag

Niet beschikbaar
19:00 - 22:00
```

De planner ziet deze informatie tijdens het plannen.

---

## 5. Planning delen

Een planner kan de volledige planning delen.

Opties:

- Kopiëren als tekst
- Delen via WhatsApp
- PDF export (later)

Voorbeeld:

```text
Planning week 31

Maandag

Kevin
08:00 - 16:00

Lisa
16:00 - 22:00
```

---

## 6. Open diensten

Wanneer geen medewerker is ingepland:

```text
Vrijdag

18:00 - 22:00

Open dienst
```

Later kan een medewerker deze dienst accepteren.

---

## 7. Notities

Elke dienst kan een notitie bevatten.

Voorbeeld:

```text
Kevin

- Sleutel meenemen
- Nieuwe medewerker inwerken
```

---

# Wat bouwen we bewust NIET?

De eerste versie bevat geen:

- Salarisadministratie
- Contractbeheer
- Verlofadministratie
- Declaraties
- Facturatie
- HR-dossiers
- AI planning
- Urenregistratie
- Klok in/uit
- Certificaten

Focus ligt volledig op plannen.

---

# SaaS Architectuur

## Multi Tenant

Elke klant krijgt zijn eigen omgeving.

Structuur:

```text
Tenant
    Users
    Employees
    Customers
    Planning
    Settings
```

Data van klanten mag nooit zichtbaar zijn voor andere klanten.

---

# Rollen

## Eigenaar

Mag alles.

---

## Planner

Kan:

- medewerkers beheren
- planning maken
- planning wijzigen

---

## Medewerker

Kan alleen:

- eigen planning bekijken
- beschikbaarheid aanpassen
- open diensten accepteren

---

# Dashboard

Voorbeeld:

```text
Vandaag

3 medewerkers aanwezig

2 open diensten

1 ziekmelding

Planning volgende week
85% gevuld
```

Dashboard moet in één oogopslag duidelijk zijn.

---

# Mobiele App

Een medewerker ziet alleen zijn eigen gegevens.

Voorbeeld:

```text
Mijn diensten

Maandag
08:00 - 16:00

Woensdag
17:00 - 22:00

Vrijdag
12:00 - 18:00
```

Functies:

- planning bekijken
- beschikbaarheid wijzigen
- notities lezen
- open diensten bekijken

---

# Roadmap

## Fase 1 (MVP)

- Medewerkers
- Planning
- Weekoverzicht
- Drag & Drop
- Planning delen
- Mobiele planning

Doel:

**Eerste betalende klant.**

---

## Fase 2

- Herhalende diensten
- Weektemplates
- Planning kopiëren
- Open diensten
- Beschikbaarheid

---

## Fase 3

- Vakantie
- Meerdere locaties
- Certificaten
- Pauzes

---

## Fase 4

- Urenregistratie
- Klok in/uit
- Loonexport
- API koppelingen

---

# UX Principes

De applicatie moet:

- extreem snel zijn
- mobiel werken
- weinig klikken vereisen
- overzichtelijk blijven
- drag & drop als primaire interactie gebruiken

Een planner moet binnen **5 minuten** een volledige weekplanning kunnen maken.

---

# Niet het doel

We bouwen geen compleet HR-pakket.

We bouwen de snelste en eenvoudigste planner voor kleine bedrijven.

---

# Verdienmodel

> Dit zijn de pakkettarieven: de alles-in-één-helft van het model. De kernprijs zijn
> planner-seats — zie [besluit 0006](decisions/0006-pricing-model.md) voor het geheel.
> De bedragen hieronder liggen nog niet vast.

Starter

- ±15 medewerkers
- €19 - €29 / maand

Groei

- Meer medewerkers
- Meerdere planners
- €49 - €69 / maand

Premium

- Meerdere vestigingen
- Uitgebreide functies
- €99+ / maand

---

# Visie

De beste planningsoftware is niet degene met de meeste functies.

De beste planningsoftware is degene waarbij een planner zonder uitleg direct begrijpt hoe hij een volledige weekplanning maakt.

Elke nieuwe functie moet bijdragen aan:

- sneller plannen
- minder fouten
- beter overzicht
- minder communicatie via WhatsApp

Als een functie hier niet aan bijdraagt, hoort deze niet in de MVP.