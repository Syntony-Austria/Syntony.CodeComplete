"""Changes one thing in the rule and asks which test class notices.
Usage: python Measure-Mutants.py <work folder>"""
import pathlib, re, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
MUTANTS = [
    ("grace 3 -> 4", "GraceDays = 3", "GraceDays = 4"),
    ("<= -> <", "daysLate <= GraceDays", "daysLate < GraceDays"),
    ("rate 25 -> 30", "CentsPerDay = 25", "CentsPerDay = 30"),
    ("maximum 2000 -> 3000", "MaxCents = 2000", "MaxCents = 3000"),
    ("double -> triple", "CentsPerDay * 2", "CentsPerDay * 3"),
    ("child pays all", "full / 2", "full"),
    ("staff pay", "member == MemberKind.Staff || ", ""),
    ("no maximum", "Math.Min(daysLate * perDay, MaxCents)", "daysLate * perDay"),
]
CLASSES = ["WeakTests", "MirrorTests", "IntentTests"]


def run(work, name):
    out = subprocess.run(["dotnet", "test", str(work), "--no-build", "--nologo",
                          "--filter", f"FullyQualifiedName~{name}"],
                         capture_output=True, text=True, encoding="utf-8", errors="replace").stdout
    return "Bestanden!" not in out and "Passed!" not in out


work = pathlib.Path(sys.argv[1])
if work.exists():
    shutil.rmtree(work)
shutil.copytree(HERE, work, ignore=shutil.ignore_patterns("bin", "obj"))
source = (work / "OverdueFee.cs").read_text(encoding="utf-8")
print("mutant;" + ";".join(CLASSES))
for label, old, new in MUTANTS:
    assert old in source, old
    (work / "OverdueFee.cs").write_text(source.replace(old, new), encoding="utf-8")
    subprocess.run(["dotnet", "build", str(work), "--nologo", "-v", "q"], capture_output=True, check=True)
    print(label + ";" + ";".join("caught" if run(work, c) else "missed" for c in CLASSES))
