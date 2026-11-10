"""Counter-proof for the sample configuration: each violation must break the build.
Usage: python Measure-AnhangB.py <work folder>"""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
WORK = pathlib.Path(sys.argv[1]) / "anhangb"
shutil.rmtree(WORK, ignore_errors=True)
shutil.copytree(HERE, WORK, ignore=shutil.ignore_patterns("bin", "obj", "Verstoesse"))
PROBE = WORK / "src" / "Library" / "Probe.cs"


def build(*extra):
    out = subprocess.run(["dotnet", "build", str(WORK / "src" / "Library"), "--nologo",
                          "--no-incremental", "-v:q", *extra], capture_output=True,
                         text=True, encoding="utf-8", errors="replace").stdout
    return sorted(set(re.findall(r"(?:error|Fehler)\s+(\w+\d+)", out)))


print("clean:", build() or "no errors")
test = subprocess.run(["dotnet", "test", str(WORK / "Library.slnx"), "--nologo"],
                      capture_output=True, text=True, encoding="utf-8", errors="replace")
print("tests:", test.stdout.strip().splitlines()[-1][:90])
for name in sorted((HERE / "Verstoesse").glob("*.cs")):
    shutil.copy(name, PROBE)
    print(name.stem, build())
PROBE.unlink()
print("TreatWarningsAsErrors off:", build("-p:TreatWarningsAsErrors=false"))
print("Nullable off:", build("-p:Nullable=disable"))
