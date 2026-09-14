# 0012 — A team member can exist before their account

- **Date:** 2026-09-14
- **Status:** final, built
- **Touches:** `Planning.Domain/Users/User.cs`, `Planning.Domain/Invites/OrganizationInvite.cs`,
  `InviteService`, `UserService`, `POST /api/users`, the team page in `Planning.Web`

## Decision

A `User` (the membership of an organization) no longer requires an `Account` (the login).
`User.AccountId` and `User.Email` are nullable. A planner adds a member by name through
`POST /api/users`; that member can be scheduled immediately and simply cannot sign in.

An invite may target such a member (`OrganizationInvite.UserId`). Accepting it links the
account to the existing record instead of creating a second member. The planner's name for the
member stays; the e-mail is only filled in from the account when the planner left it empty.
Open invites without a target keep working as before.

## Why

Every comparable product (Deputy, When I Work, Homebase, Shiftbase, Sling, 7shifts, Planday)
lets you add people first and invite them later, and the first customer will hit this on day
one: "I want to make my schedule, my people have not clicked the link yet." Before this, the
membership only came into being on invite acceptance, so the planning was blocked on every
employee signing up.

It also fits the MVP point "manage customers and employees" and keeps the product simple to
explain: a member is someone on the schedule; an account is how they log in.

## Alternative considered

**A placeholder `Account` without a password.** Keeps `AccountId` required, but creates login
identities for people who may never log in, needs a fake or missing e-mail on a table with a
unique e-mail index, and makes password reset and login reason about "accounts that are not
really accounts". Making the membership stand on its own is the smaller change and the truer
model.

**Requiring an e-mail on the member.** Rejected: the planner often does not know it when they
set up the schedule, and it is only needed once an invite is sent.
