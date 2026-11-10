"""How often does a later "fix" touch the same files, by commit size?
Usage: python Measure-CommitFix.py <repository>"""
import re, subprocess, sys

log = subprocess.run(
    ["git", "-C", sys.argv[1], "log", "--no-merges", "--reverse",
     "--format=%x00%at|%s", "--numstat"],
    capture_output=True, text=True, encoding="utf-8", errors="replace").stdout
commits = []
for block in log.split("\x00")[1:]:
    lines = block.strip().split("\n")
    when, subject = lines[0].split("|", 1)
    files, size = set(), 0
    for line in lines[1:]:
        m = re.match(r"(\d+)\t(\d+)\t(.*\.cs)$", line)
        if m:
            files.add(m[3])
            size += int(m[1]) + int(m[2])
    if files:
        commits.append((int(when), subject, files, size))

FIX = re.compile(r"\s*(fix|bugfix|hotfix)\b", re.I)
WEEK = 7 * 86400
for low, high in [(0, 100), (100, 400), (400, 1600), (1600, 10**9)]:
    group = [i for i, c in enumerate(commits) if low <= c[3] < high]
    followed = 0
    for i in group:
        for later in commits[i + 1:]:
            if later[0] - commits[i][0] > WEEK:
                break
            if FIX.match(later[1]) and later[2] & commits[i][2]:
                followed += 1
                break
    share = 100 * followed / max(1, len(group))
    print(f"{low}-{high}: {len(group)} commits, "
          f"{followed} followed ({share:.0f} %)")
