# Requirements: Library.Lending

One requirement per line: ID, colon, one sentence. A test names
the ID with [Requirement("LEND-001")]. Example rows are tests too.

LEND-001: A loan is due 28 days after the day it is lent.
LEND-002: Returning up to three days after the due date costs nothing.
LEND-003: From the fourth day late, every day late costs 25 cents,
  the first three included.
LEND-004: A reference book costs twice as much per day.
LEND-005: A child pays half, rounded down to whole cents.
LEND-006: Child or adult is decided on the day the book is lent.
LEND-007: Staff pay nothing.
LEND-008: The fee is capped at 2000 cents before the child's half.
LEND-009: The card on the loan pays, whoever returns the book.
LEND-010: Fees for 10,000 loans take at most 20 ms at the 95th
  percentile of 100 runs in the test run.

## Examples

| ID       | days late | member | reference | cents |
|----------|-----------|--------|-----------|-------|
| LEND-002 | 3         | Adult  | no        | 0     |
| LEND-003 | 4         | Adult  | no        | 100   |
| LEND-003 | 10        | Adult  | no        | 250   |
| LEND-004 | 4         | Adult  | yes       | 200   |
| LEND-005 | 4         | Child  | no        | 50    |
| LEND-005 | 5         | Child  | no        | 62    |
| LEND-007 | 400       | Staff  | yes       | 0     |
| LEND-008 | 100       | Adult  | no        | 2000  |
| LEND-008 | 400       | Child  | yes       | 1000  |
