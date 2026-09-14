# Runbooks

What to do when something has to happen on the production server. Work top to bottom; every
step is meant to be copied as-is.

| Document | When |
|---|---|
| [first-deploy.md](first-deploy.md) | from nothing to production — server, DNS, Docker, TLS, email, backups |
| [deploy-and-rollback.md](deploy-and-rollback.md) | the day-to-day, and what to do when a deploy fails |

Two things recur in both documents, because they are the most expensive to forget:

- **Rolling back does not roll back the database.** Images go back, the schema does not.
- **A backup that has never been restored is a guess.** Do the restore drill once before the
  first customer, not after the first incident.
