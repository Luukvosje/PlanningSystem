# Architecture

How the system fits together and why. Not how you write code in it — that is
[../guidelines/](../guidelines/).

| Document | Answers |
|---|---|
| [overview.md](overview.md) | Which projects exist, what may depend on what, which path a request takes, what production looks like |
| [multi-tenancy.md](multi-tenancy.md) | How does the system know *which* customer you are, and where is that boundary enforced? |
| [modules.md](modules.md) | How are Planning, Klant and Beheer switched on and off per organization and per user? |

Start with `multi-tenancy.md` if you are about to touch the API. That is where a mistake is
not a bug but a data leak.
