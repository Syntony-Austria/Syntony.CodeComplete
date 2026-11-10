"""Runs the hidden tests against every generated solution in Proben.
Usage: python Measure-Proben.py <work folder>"""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
base = pathlib.Path(sys.argv[1])
for probe in sorted((HERE / "Proben").glob("*.cs")):
    work = base / probe.stem
    if work.exists():
        shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True)
    project = (HERE / "Kapitel49.csproj").read_text(encoding="utf-8")
    (work / "p.csproj").write_text(project, encoding="utf-8")
    shutil.copy(HERE / "ProbeTests.cs", work)
    shutil.copy(probe, work / "Generated.cs")
    out = subprocess.run(["dotnet", "test", str(work), "--nologo", "--logger",
                          "console;verbosity=normal"], capture_output=True, text=True,
                         encoding="utf-8", errors="replace").stdout
    failed = sorted(set(re.findall(r"^\s*Fehler (.+?) \[", out, re.M)))
    total = re.search(r"Fehler:\s+(\d+), erfolgreich:\s+(\d+)", out)
    print(probe.stem, total.groups() if total else out[-300:], failed)
