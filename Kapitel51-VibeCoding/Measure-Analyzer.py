"""Builds every generated solution in Proben with all .NET analyzer rules
and counts the warnings per rule in the generated file.
Usage: python Measure-Analyzer.py <work folder>
Measured on 2026-11-10: CA1515 three times in every solution (a test
project reports each public type); besides that knapp-1..3 none,
genau-1 CA1307 twice (one line), genau-2 CA1307 once, genau-3 CA1031 once."""
import collections, os, pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
base = pathlib.Path(sys.argv[1])
ENV = dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en", DOTNET_NOLOGO="1")
WARNING = re.compile(r"Generated\.cs\((\d+),(\d+)\): warning (CA\d+)")
for probe in sorted((HERE / "Proben").glob("*.cs")):
    work = base / probe.stem
    if work.exists():
        shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True)
    project = (HERE / "Kapitel51.csproj").read_text(encoding="utf-8")
    (work / "p.csproj").write_text(project, encoding="utf-8")
    shutil.copy(HERE / "ProbeTests.cs", work)
    shutil.copy(probe, work / "Generated.cs")
    out = subprocess.run(["dotnet", "build", str(work), "--no-incremental",
                          "-p:AnalysisLevel=latest-all"],
                         capture_output=True, text=True, env=ENV,
                         encoding="utf-8", errors="replace").stdout
    # The build prints each warning twice; count each place once.
    places = set(WARNING.findall(out))
    rules = collections.Counter(rule for _, _, rule in places)
    print(f"{probe.stem}: {dict(sorted(rules.items())) or 'none'}")
