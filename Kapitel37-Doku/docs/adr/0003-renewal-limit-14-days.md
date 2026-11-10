# ADR-0003: Renewals end 14 days after the due date

- Owner: head of lending desk
- Reversal cost: medium (rule, tests, reminder letters)
- Review by: 2027-05-01

## Status

Accepted

## Context

The desk wanted one clear point from which a loan belongs to the
reminder process. Before that, staff renewed by hand weeks after the
due date while reminder letters were already on their way.
Options were 7, 14 and 21 days.

## Decision

A loan may be renewed up to 14 days after its due date, not later.
The constant MaxDaysOverdueForRenewal in RenewalPolicy holds the value.

## Consequences

From day 15 a member must return the book or talk to the desk.
21 days would overlap with the second reminder at day 15.
Changing the value means changing the reminder plan as well.
