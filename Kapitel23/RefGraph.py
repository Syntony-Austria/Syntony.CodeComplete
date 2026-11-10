"""Counts project references: who points at whom.

Usage: python RefGraph.py <folder>
"""
import collections
import pathlib
import re
import sys

fan_in = collections.Counter()
fan_out = collections.Counter()
for project in pathlib.Path(sys.argv[1]).rglob("*.csproj"):
    skip = "tests" in str(project).lower() or {"bin", "obj"} & set(project.parts)
    if skip:
        continue
    text = project.read_text(encoding="utf-8", errors="replace")
    for target in re.findall(r'ProjectReference\s+Include="([^"]+)"', text):
        name = pathlib.PurePosixPath(target.replace("\\", "/")).stem
        fan_in[name] += 1
        fan_out[project.stem] += 1

print("most depended on:", fan_in.most_common(6))
print("depends on most:", fan_out.most_common(6))
