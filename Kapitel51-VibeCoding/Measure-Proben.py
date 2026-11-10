"""Runs the hidden tests against every generated solution in Proben.
Usage: python Measure-Proben.py <work folder>"""
import os, pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
base = pathlib.Path(sys.argv[1])
# English output on every machine, so the patterns below match.
ENV = dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en", DOTNET_NOLOGO="1")
for probe in sorted((HERE / "Proben").glob("*.cs")):
    work = base / probe.stem
    if work.exists():
        shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True)
    project = (HERE / "Kapitel51.csproj").read_text(encoding="utf-8")
    (work / "p.csproj").write_text(project, encoding="utf-8")
    shutil.copy(HERE / "ProbeTests.cs", work)
    shutil.copy(probe, work / "Generated.cs")
    out = subprocess.run(["dotnet", "test", str(work), "--nologo",
                          "--logger", "console;verbosity=normal"],
                         capture_output=True, text=True, env=ENV,
                         encoding="utf-8", errors="replace").stdout
    failed = sorted(set(re.findall(r"^\s*Failed (.+?) \[", out, re.M)))
    passed = re.search(r"^\s*Passed:\s+(\d+)", out, re.M)
    total = re.search(r"^\s*Total tests:\s+(\d+)", out, re.M)
    if not (passed and total):
        sys.exit(f"{probe.stem}: no test summary\n{out[-300:]}")
    print(f"{probe.stem}: {passed[1]} of {total[1]} passed", failed)
