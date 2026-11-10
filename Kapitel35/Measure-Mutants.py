"""Breaks WatchedLock on purpose and asks which deadlock test notices.
Usage: python Measure-Mutants.py <work folder> [runs]
Expected: "no ring search" -> OppositeOrder... fails after the 5 s exit;
"no first victim" -> OppositeOrder... mostly passes, EveryRing... fails;
"graph for every Enter" -> all pass (slower, not wrong)."""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
ONE = "OppositeOrderIsReportedOnceAndBothFinish"
RING = "EveryRingHasExactlyOneVictim"
MUTANTS = [
    ("no ring search", "ThrowIfRing(me);", "", [ONE]),
    ("no first victim", "WaitsFor.Remove(me); // one victim is enough", "",
     [ONE, RING]),
    ("graph for every Enter", "if (!_gate.TryEnter())", "if (true)",
     [ONE, RING, "PlainContentionReportsNothing", "AFixedOrderIsNeverReported"]),
]


def failed(work, test):
    out = subprocess.run(["dotnet", "test", str(work), "--no-build", "--nologo",
                          "--filter", f"Name={test}"], capture_output=True,
                         text=True, encoding="utf-8", errors="replace").stdout
    return "Bestanden!" not in out and "Passed!" not in out


work = pathlib.Path(sys.argv[1])
runs = int(sys.argv[2]) if len(sys.argv) > 2 else 3
if work.exists():
    shutil.rmtree(work)
shutil.copytree(HERE, work, ignore=shutil.ignore_patterns("bin", "obj", "Probe"))
source = (work / "WatchedLock.cs").read_text(encoding="utf-8")
print("mutant;test;red in runs")
for label, old, new, tests in MUTANTS:
    assert old in source, old
    (work / "WatchedLock.cs").write_text(source.replace(old, new), encoding="utf-8")
    subprocess.run(["dotnet", "build", str(work), "--nologo", "-v", "q"],
                   capture_output=True, check=True)
    for test in tests:
        red = sum(failed(work, test) for _ in range(runs))
        print(f"{label};{test};{red} of {runs}")
