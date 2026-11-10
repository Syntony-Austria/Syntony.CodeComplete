"""What do commits look like? Size, share with tests, size of the largest.
Usage: python Measure-History.py <repository> [<repository> ...]"""
import statistics, subprocess, sys

for repo in sys.argv[1:]:
    log = subprocess.run(["git", "-C", repo, "log", "--numstat", "--no-merges",
                          "--format=@%H"], capture_output=True, text=True,
                         encoding="utf-8", errors="replace").stdout
    commits = []
    for block in log.split("@")[1:]:
        lines, tests, code = 0, False, False
        for row in block.splitlines()[1:]:
            parts = row.split("\t")
            if len(parts) != 3 or parts[0] == "-":
                continue
            lines += int(parts[0]) + int(parts[1])
            is_cs = parts[2].endswith(".cs")
            is_test = "test" in parts[2].lower()
            tests |= is_cs and is_test
            code |= is_cs and not is_test
        commits.append((lines, code, tests))
    with_code = [c for c in commits if c[1]]
    sizes = sorted(c[0] for c in commits)
    print(repo, "commits", len(commits), "median", statistics.median(sizes),
          "p90", sizes[int(len(sizes) * 0.9)], "max", sizes[-1],
          "under 100", sum(s < 100 for s in sizes),
          "code", len(with_code), "code+tests", sum(c[2] for c in with_code))
