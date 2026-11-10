# ADR-0002: Fees are whole cents in an int

- Owner: lending team
- Reversal cost: high (stored data, exports, two neighbour systems)
- Review by: 2027-05-01

## Status

Accepted

## Context

Fees were stored as double. Sums of many small fees drifted by a cent,
and the receipt did not match the account.

## Decision

Every fee is an int number of cents. Conversion to euros happens only
when a text for a person is written.

## Consequences

No rounding inside the system. A fee above about 21 million euros
does not fit, which the library accepts.
Changing the unit later needs a data migration.
