# Runbooks

Wat je doet als er iets moet gebeuren op de productieserver. Van boven naar beneden af te
werken, elke stap te kopiëren.

| Document | Wanneer |
|---|---|
| [first-deploy.md](first-deploy.md) | van niets naar productie — server, DNS, Docker, TLS, e-mail, back-ups |
| [deploy-en-rollback.md](deploy-en-rollback.md) | de dagelijkse gang van zaken, en wat je doet als een deploy faalt |

Twee dingen die in beide documenten terugkomen, omdat ze het duurst zijn om te vergeten:

- **Terugrollen draait de database niet terug.** Images gaan terug, het schema niet.
- **Een back-up die nooit hersteld is, is een gok.** Doe de hersteloefening één keer vóór de
  eerste klant, niet na het eerste incident.
