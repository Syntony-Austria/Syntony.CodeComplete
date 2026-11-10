"""Runs the steps from red to green and prints what the compiler and the tests said.
Usage: python Measure-Tdd.py <work folder>"""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
work = pathlib.Path(sys.argv[1])
if work.exists():
    shutil.rmtree(work)
work.mkdir(parents=True)
project = (HERE / "Kapitel31.csproj").read_text(encoding="utf-8")
(work / "Kapitel31.csproj").write_text(project, encoding="utf-8")
for step in sorted((HERE / "Schritte").iterdir(), key=lambda p: int(p.name)):
    for name in ("Renewals.cs", "RenewalTests.cs"):
        shutil.copy(step / name, work / name)
    out = subprocess.run(["dotnet", "test", str(work), "--nologo"], capture_output=True,
                         text=True, encoding="utf-8", errors="replace").stdout
    code = re.search(r"error (CS\d+)", out)
    summary = re.search(r"Fehler:\s+(\d+), erfolgreich:\s+(\d+)", out)
    if code:
        print(step.name, "compile", code.group(1))
    elif summary:
        print(step.name, "failed", summary.group(1), "passed", summary.group(2))
    else:
        print(step.name, out[-200:])
