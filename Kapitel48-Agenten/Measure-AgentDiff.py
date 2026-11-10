"""Builds a small repository with a harmless and a suspicious change and runs the check.
Usage: python Measure-AgentDiff.py <work folder>"""
import pathlib, shutil, subprocess, sys

HERE = pathlib.Path(__file__).parent
work = pathlib.Path(sys.argv[1])
if work.exists():
    shutil.rmtree(work, ignore_errors=True)
work.mkdir(parents=True)


def git(*args):
    return subprocess.run(["git", *args], cwd=work, capture_output=True, text=True).stdout.strip()


def save(name, text):
    p = work / name
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text)
    git("add", name)


git("init", "-q")
git("config", "user.email", "probe@example.org")
git("config", "user.name", "Probe")
save("Fee.cs", "class Fee { int Rate => 25; }\n")
save("FeeTests.cs", "class FeeTests { void T() { Assert.That(Rate, Is.EqualTo(25)); } }\n")
save("floor.json", '{"line": 80, "branch": 70}\n')
git("commit", "-q", "-m", "start")
base = git("rev-parse", "HEAD")
save("Fee.cs", "class Fee { int RatePerDay => 25; }\n")
save("FeeTests.cs", "class FeeTests { void T() { Assert.That(RatePerDay, Is.EqualTo(25)); } }\n")
git("commit", "-q", "-m", "rename")
rename = git("rev-parse", "HEAD")
save("Fee.cs", "class Fee { int RatePerDay => 30; }\n")
save("FeeTests.cs", "class FeeTests { [Ignore(\"flaky\")] void T() { Assert.Pass(); } }\n")
save("floor.json", '{"line": 60, "branch": 50}\n')
save("App.csproj", '<PackageReference Include="Super.Fee.Helper" Version="1.0.0" />\n')
git("commit", "-q", "-m", "fix: tests are green now")
print("== rename only")
print(subprocess.run([sys.executable, str(HERE / "Check-AgentDiff.py"), str(work), base, rename],
                     capture_output=True, text=True).stdout)
print("== all green now")
r = subprocess.run([sys.executable, str(HERE / "Check-AgentDiff.py"), str(work), rename],
                   capture_output=True, text=True)
print(r.stdout, "exit", r.returncode)
