"""Fails when line or branch coverage falls below the floor.
Usage: python Ratchet.py <coverage.cobertura.xml> <floor.json> [--raise]"""
import json, math, pathlib, sys
import xml.etree.ElementTree as ET

report, floor_file = sys.argv[1], pathlib.Path(sys.argv[2])
root = ET.parse(report).getroot()
line = float(root.get("line-rate")) * 100
branch = float(root.get("branch-rate")) * 100
floor = (json.loads(floor_file.read_text()) if floor_file.exists()
         else {"line": 0, "branch": 0})
print(f"line {line:.1f} (floor {floor['line']}), "
      f"branch {branch:.1f} (floor {floor['branch']})")
if "--raise" in sys.argv:
    new = {"line": max(floor["line"], math.floor(line) - 2),
           "branch": max(floor["branch"], math.floor(branch) - 2)}
    floor_file.write_text(json.dumps(new))
    print("floor now", new)
sys.exit(2 if line < floor["line"] or branch < floor["branch"] else 0)
