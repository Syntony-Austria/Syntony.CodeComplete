# Changelog

All notable changes to Library.Lending are documented in this file.
The format is based on Keep a Changelog 1.1.0, and the project
follows Semantic Versioning.

## [Unreleased]

## [1.3.0] - 2026-11-08

### Added

- `IFeeSchedule` and `FeeSchedule`: the overdue fee behind an interface.

### Changed

- `RenewalPolicy.MayRenew` throws `ArgumentNullException` for null.

## [1.2.0] - 2026-11-02

### Added

- `RenewalPolicy.MayRenew` with the three renewal rules.
