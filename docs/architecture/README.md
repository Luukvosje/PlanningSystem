# Architectuur

Hoe het systeem in elkaar zit en waarom. Niet hoe je er code in schrijft — dat staat in
[../guidelines/](../guidelines/).

| Document | Beantwoordt |
|---|---|
| [overview.md](overview.md) | Welke projecten zijn er, wat mag waarvan afhangen, welke weg legt een request af, hoe ziet productie eruit? |
| [multi-tenancy.md](multi-tenancy.md) | Hoe weet het systeem wélke klant je bent, en waar wordt die grens bewaakt? |
| [modules.md](modules.md) | Hoe worden Planning, Klant en Beheer per organisatie en per gebruiker aan- en uitgezet? |

Begin bij `multi-tenancy.md` als je iets aan de API gaat doen. Dat is de plek waar een fout
geen bug is maar een datalek.
