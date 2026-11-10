"""Which files change together? Usage: python CoChange.py <repo> [commits]"""
import collections
import itertools
import subprocess
import sys

repo = sys.argv[1]
commits = sys.argv[2] if len(sys.argv) > 2 else "1000"
log = subprocess.run(
    ["git", "-C", repo, "log", "-n", commits, "--name-only", "--format=@"],
    capture_output=True, text=True, encoding="utf-8", errors="replace").stdout

alone = collections.Counter()
pairs = collections.Counter()
for commit in log.split("@"):
    files = sorted({f for f in commit.split("\n") if f.endswith(".cs")})
    if len(files) > 12:  # A sweep across the repo says nothing about one secret.
        continue
    alone.update(files)
    pairs.update(itertools.combinations(files, 2))

for (a, b), n in pairs.most_common(200):
    share = n / min(alone[a], alone[b])
    if n >= 5 and share >= 0.6 and "Test" not in a + b:
        print(f"{n:3} {share:4.0%}  {a}  {b}")
