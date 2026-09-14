# Planning SaaS - MVP specification

## Goal

Build a simple planning SaaS that lets small companies (gyms, for instance) schedule their
staff easily.

The first audience currently works with:
- Microsoft Word
- WhatsApp
- Excel (optional)

The goal is **not** to build a full HR solution straight away, but to make the current process
considerably simpler.

---

# Audience

## First customer

A gym.

The problem:
- The planning is made in Word.
- The planning is shared over WhatsApp.
- Changes cost a lot of time.
- There is no central overview.
- Everybody works from a different version.

The goal:

> Build a complete week's planning in under 5 minutes and share it immediately.

---

# MVP features

## 1. Employees

An employee holds at least:

- id
- name
- phone number
- colour
- active/inactive

Example:

```text
Kevin
+31 6 12345678
Blue
Active
```

---

## 2. Shifts

A shift consists of:

- date
- start time
- end time
- employee
- location (optional)
- note

Example:

```text
Monday

08:00 - 12:00 Kevin
12:00 - 17:00 Lisa
17:00 - 22:00 Tom
```

---

## 3. Week planning

This is the most important screen in the application.

Layout:

```text
Mon | Tue | Wed | Thu | Fri | Sat | Sun
```

Every shift is shown under its day.

Required interactions:

- add a new shift
- change a shift
- delete a shift
- drag a shift to another day
- change the employee
- change the time

Drag and drop is the default interaction.

---

## 4. Availability

An employee can say when they are available.

For example:

```text
Monday

✓ Morning
✓ Afternoon
✗ Evening
```

Or:

```text
Friday

Unavailable
19:00 - 22:00
```

The planner sees this while scheduling.

---

## 5. Sharing a planning

A planner can share the whole planning.

Options:

- copy as text
- share over WhatsApp
- PDF export (later)

Example:

```text
Planning week 31

Monday

Kevin
08:00 - 16:00

Lisa
16:00 - 22:00
```

---

## 6. Open shifts

When no employee is scheduled:

```text
Friday

18:00 - 22:00

Open shift
```

Later an employee can accept that shift.

---

## 7. Notes

Every shift can carry a note.

Example:

```text
Kevin

- bring the key
- show the new colleague around
```

---

# What we deliberately do NOT build

The first version contains no:

- payroll
- contract management
- leave administration
- expenses
- invoicing
- HR files
- AI planning
- time tracking
- clock in/out
- certificates

The focus is entirely on planning.

---

# SaaS architecture

## Multi-tenant

Every customer gets their own environment.

Structure:

```text
Tenant
    Users
    Employees
    Customers
    Planning
    Settings
```

One customer's data must never be visible to another.

---

# Roles

## Owner

May do everything.

---

## Planner

Can:

- manage employees
- create a planning
- change a planning

---

## Employee

Can only:

- view their own planning
- change their availability
- accept open shifts

---

# Dashboard

Example:

```text
Today

3 employees present

2 open shifts

1 sick note

Planning next week
85% filled
```

The dashboard has to be clear at a glance.

---

# Mobile app

An employee sees only their own data.

Example:

```text
My shifts

Monday
08:00 - 16:00

Wednesday
17:00 - 22:00

Friday
12:00 - 18:00
```

Features:

- view the planning
- change availability
- read notes
- view open shifts

---

# Roadmap

## Phase 1 (MVP)

- employees
- planning
- week overview
- drag and drop
- sharing a planning
- planning on mobile

Goal:

**The first paying customer.**

---

## Phase 2

- recurring shifts
- week templates
- copying a planning
- open shifts
- availability

---

## Phase 3

- holidays
- multiple locations
- certificates
- breaks

---

## Phase 4

- time tracking
- clock in/out
- payroll export
- API integrations

---

# UX principles

The application has to:

- be extremely fast
- work on mobile
- require few clicks
- stay readable
- use drag and drop as the primary interaction

A planner has to be able to build a complete week's planning within **5 minutes**.

---

# Not the goal

We are not building a complete HR package.

We are building the fastest and simplest planner for small companies.

---

# Revenue model

> These are the package rates: the all-in half of the model. The core price is planner seats —
> see [decision 0006](decisions/0006-pricing-model.md) for the whole picture. The amounts below
> are not settled.

Starter

- ±15 employees
- €19 - €29 per month

Groei

- more employees
- several planners
- €49 - €69 per month

Premium

- several locations
- extended features
- €99+ per month

---

# Vision

The best planning software is not the one with the most features.

The best planning software is the one where a planner immediately understands, without
explanation, how to build a complete week's planning.

Every new feature has to contribute to:

- planning faster
- fewer mistakes
- a better overview
- less communication over WhatsApp
