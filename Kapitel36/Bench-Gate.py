"""Compares a BenchmarkDotNet report with a baseline. Exit 2 on regression.
Usage: python Bench-Gate.py <report-full.json> <baseline.json> [--accept]"""
import json, pathlib, sys

THRESHOLD_PERCENT = 5.0

report = json.loads(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8"))
base_file = pathlib.Path(sys.argv[2])
now = {b["FullName"]: (b["Statistics"]["Mean"],
                       b["Memory"]["BytesAllocatedPerOperation"])
       for b in report["Benchmarks"]}
if "--accept" in sys.argv:
    base_file.write_text(json.dumps(now, indent=1))
    print("baseline written; a person decided that")
    sys.exit(0)
base = json.loads(base_file.read_text())
bad = 0
for name, (mean, bytes_) in now.items():
    if name not in base:
        print("new      ", name)
        continue
    old_mean, old_bytes = base[name]
    change = (mean - old_mean) / old_mean * 100
    slower = change > THRESHOLD_PERCENT
    more = bytes_ > old_bytes
    bad += slower or more
    verdict = "REGRESSED" if slower or more else "ok"
    print(f"{verdict:9} {change:+6.1f} %  {old_bytes} -> {bytes_} B  {name}")
sys.exit(2 if bad else 0)
