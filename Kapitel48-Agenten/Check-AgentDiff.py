"""Reads a diff and names what a person should look at first.
Usage: python Check-AgentDiff.py <repository> <base> [<head>]"""
import re, subprocess, sys

repo, base = sys.argv[1], sys.argv[2]
head = sys.argv[3] if len(sys.argv) > 3 else "HEAD"
diff = subprocess.run(["git", "-C", repo, "diff", base, head], capture_output=True,
                      text=True, encoding="utf-8", errors="replace").stdout
SUSPECTS = [
    (r"^\+.*\[(Ignore|Explicit)", "test switched off"),
    (r"^\+.*Assert\.Pass", "test passes without checking"),
    (r"^-.*Assert\.", "an assertion was removed"),
    (r"^\+.*ExcludeFromCodeCoverage", "code excluded from coverage"),
    (r"^\+.*#pragma warning disable", "warning switched off"),
    (r"^\+.*SuppressMessage", "warning switched off"),
    (r"^\+.*PackageReference", "new package: does it exist, who keeps it?"),
    (r"^\+.*(\.Result\b|\.Wait\(\))", "blocks on async code"),
    (r"^\+.*(password|secret|apikey)\s*=\s*\"", "secret in the source"),
]
FLOOR = re.compile(r'^-.*"(line|branch)": *(\d+)')
findings = []
current = ""
for line in diff.splitlines():
    if line.startswith("+++ b/"):
        current = line[6:]
    for pattern, why in SUSPECTS:
        if re.search(pattern, line, re.I) and not line.startswith(("+++", "---")):
            findings.append((current, why, line[:70]))
    if "floor" in current and FLOOR.match(line):
        findings.append((current, "coverage floor changed", line[:70]))
    if line.startswith("+++ b/") and "/Golden/" in line:
        findings.append((current, "golden file changed", ""))
for name, why, text in findings:
    print(f"{why}: {name}  {text}")
print(len(findings), "places to read first")
sys.exit(2 if findings else 0)
