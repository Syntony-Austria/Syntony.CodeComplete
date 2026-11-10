# Kapitel48-Anforderungen

The lending rules as requirements with IDs (`Data/requirements.md`, LEND-001
to LEND-010); tests name them with `[Requirement("LEND-005")]`, and the
example rows in the file run as tests. `TraceTests` is the gate: it fails
when a requirement has no test or a test names an unknown ID.

Run `dotnet test` (36 tests). The red probes and the filter measurements:
`dotnet test -p:Probe=true` (three red: unknown ID, slow lookup, changed
example), e.g. `dotnet test -p:Probe=true --filter Priority=1`.
