# ADR-0001: We record decisions as short text files

- Owner: lending team
- Reversal cost: low
- Review by: 2027-11-01

## Status

Accepted

## Context

Rules of the library change, and the people who set them move on.
A comment can say why a line is there, but not what we weighed.

## Decision

Every decision that a reader of the code would question gets one file
in docs/adr, numbered, never renumbered, never deleted.

## Consequences

A changed decision gets a new file; the old one is marked Superseded.
A test checks the form of every file and every ADR number in the code.
