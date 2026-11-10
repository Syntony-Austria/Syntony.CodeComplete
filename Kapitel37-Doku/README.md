# Library.Lending

## Purpose

Rules for lending at a city library: renewals, due dates, fees.
No storage, no user interface; other projects call these rules.

## Status

Used in production by the lending desk. Version 1.3.0.
Fees follow ADR-0002, renewals ADR-0003. Reminders are not here yet.

## First five minutes

    git clone <repository>
    cd Kapitel37-Doku
    dotnet test tests/Library.Lending.Tests

Green means: rules, documentation and changelog agree.

## Where next

- Decisions and their reasons: docs/adr
- What changed in which version: CHANGELOG.md
- Questions: the lending team channel
