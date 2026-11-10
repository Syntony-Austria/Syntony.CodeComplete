# Kapitel49-Projekt

An invented year of finished work pieces (`Data/done.csv`, made by
`DemoHistory` with seed 49). The code turns it into weekly throughput,
forecasts 30 pieces with Monte Carlo, backtests the forecast against what
came later, checks Little's Law and simulates a WIP limit.

Run `dotnet test` (21 tests, about ten seconds; one builds a temporary git
repository). The forecast output: `dotnet test --filter Name=PrintForecast
--logger "console;verbosity=detailed"`. The red probes:
`dotnet test -p:Probe=true --filter Category=Probe`.
