"""Measures Table 13 and Table 14 of chapter 8 on QualityProbe.cs.
Usage: python Measure-Kapitel08.py <work folder>
Prints, per line, the diagnostic IDs of one build with -p:QualityProbe=true:
first the five AnalysisMode values, then the default build with one more
analyzer package in its newest version. IDs from other files of App follow
after "other:". Warnings are not errors here."""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
WORK = pathlib.Path(sys.argv[1]) / "kapitel08"
PACKAGES = ["Roslynator.Analyzers", "Meziantou.Analyzer", "SonarAnalyzer.CSharp"]


def run(*args):
    return subprocess.run(args, capture_output=True, text=True,
                          encoding="utf-8", errors="replace").stdout


def build(app, *extra):
    out = run("dotnet", "build", str(app), "--nologo", "--no-incremental",
              "-p:QualityProbe=true", "-p:TreatWarningsAsErrors=false", *extra)
    found = set(re.findall(r"(\w+)\.cs\(\d+,\d+\): warning (\w+\d+)", out))
    probe = sorted({rule for file, rule in found if file == "QualityProbe"})
    other = sorted({file + ":" + rule for file, rule in found
                    if file != "QualityProbe"})
    return " ".join(probe) + ("  other: " + " ".join(other) if other else "")


shutil.rmtree(WORK, ignore_errors=True)
for part in ("App", "Rules"):
    shutil.copytree(HERE / part, WORK / part,
                    ignore=shutil.ignore_patterns("bin", "obj"))
APP = WORK / "App" / "App.csproj"

for mode in ("None", "Default", "Minimum", "Recommended", "All"):
    print(mode + ":", build(APP, "-p:AnalysisMode=" + mode))
for package in PACKAGES:
    shutil.copy(HERE / "App" / "App.csproj", APP)
    run("dotnet", "add", str(APP), "package", package)
    version = re.search(package + r'" Version="([^"]+)"', APP.read_text())
    print(package, version.group(1) if version else "?", build(APP))
