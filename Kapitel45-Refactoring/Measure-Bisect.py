"""A history of 64 commits with one bug. How many tests does git bisect need?
Usage: python Measure-Bisect.py <work folder> [bad commit, default 41]"""
import pathlib, shutil, subprocess, sys

work = pathlib.Path(sys.argv[1])
bad = int(sys.argv[2]) if len(sys.argv) > 2 else 41
if work.exists():
    shutil.rmtree(work, ignore_errors=True)
work.mkdir(parents=True)


def git(*args):
    return subprocess.run(["git", *args], cwd=work, capture_output=True, text=True).stdout


git("init", "-q")
git("config", "user.email", "probe@example.org")
git("config", "user.name", "Probe")
for i in range(1, 65):
    rate = 30 if i >= bad else 25
    (work / "fee.txt").write_text(f"rate={rate}\nchange={i}\n")
    git("add", "fee.txt")
    git("commit", "-q", "-m", f"change {i}")
(work.parent / "check.py").write_text(
    "import sys\nsys.exit(0 if 'rate=25' in open('fee.txt').read() else 1)\n")
first = git("rev-list", "--max-parents=0", "HEAD").strip()
git("bisect", "start", "HEAD", first)
git("bisect", "run", sys.executable, str(work.parent / "check.py"))
log = git("bisect", "log")
steps = sum(l.startswith(("git bisect good", "git bisect bad")) for l in log.splitlines())
found = git("log", "-1", "--format=%s", "refs/bisect/bad").strip()
print("commits 64, marked by the test", steps, "first bad:", found)
