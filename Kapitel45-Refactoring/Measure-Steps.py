"""Runs the same test after each refactoring step.
Usage: python Measure-Steps.py <work folder>"""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
work = pathlib.Path(sys.argv[1])
if work.exists():
    shutil.rmtree(work)
work.mkdir(parents=True)
text = (HERE / "Kapitel45.csproj").read_text(encoding="utf-8")
(work / "p.csproj").write_text(text, encoding="utf-8")
shutil.copy(HERE / "LateNoticeTests.cs", work)
for step in sorted((HERE / "Schritte").iterdir(), key=lambda p: int(p.name)):
    shutil.copy(step / "LateNotice.cs", work)
    out = subprocess.run(["dotnet", "test", str(work), "--nologo"], capture_output=True,
                         text=True, encoding="utf-8", errors="replace").stdout
    found = re.search(r"Fehler:\s+(\d+), erfolgreich:\s+(\d+)", out)
    print(step.name, "failed", found.group(1), "passed", found.group(2))
