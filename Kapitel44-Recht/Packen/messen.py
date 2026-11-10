"""License metadata measurements for chapter 44. Offline.

Packs the same small library with six license settings and records exit
code, NuGet codes and the <license>/<licenseUrl> element of the nuspec.
Then runs the license gate's rule on each nuspec (exact match on a list).

Run from this folder: python messen.py
"""
import os
import re
import subprocess
import tempfile
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
ENV = dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en", DOTNET_NOLOGO="1")
CASES = ["expression", "file", "both", "url", "unknown", "lost", "none"]
ALLOWED = {"MIT", "Apache-2.0"}

lines = ["Fall\tCode\tMeldungen\tnuspec\tGate"]
for case in CASES:
    out = tempfile.mkdtemp(prefix="k44-pack-")
    p = subprocess.run(["dotnet", "pack", "Packen.csproj", "-c", "Release",
                        f"-p:Fall={case}", "-o", out, "--nologo"],
                       cwd=HERE, env=ENV, capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    text = p.stdout + p.stderr
    codes = sorted(set(re.findall(r"\b(NU\d{4})\b", text)))
    element, gate = "-", "-"
    for name in os.listdir(out):
        if name.endswith(".nupkg"):
            with zipfile.ZipFile(os.path.join(out, name)) as z:
                spec = next(n for n in z.namelist() if n.endswith(".nuspec"))
                xml = z.read(spec).decode("utf-8")
            m = re.search(r"<license[^>]*>.*?</license>|<licenseUrl>.*?"
                          r"</licenseUrl>", xml)
            element = m.group(0) if m else "(none)"
            e = re.search(r'<license type="expression">(.*?)</license>', xml)
            gate = "pass" if e and e.group(1) in ALLOWED else "finding"
    lines.append(f"{case}\t{p.returncode}\t{','.join(codes) or '-'}\t"
                 f"{element}\t{gate}")
    for msg in sorted(set(re.findall(r"(?:error|warning) (NU\d{4}): ([^\[\r\n]*)",
                                      text))):
        lines.append(f"    {msg[0]}: {msg[1].strip()}")

result = "\n".join(lines)
print(result)
with open(os.path.join(HERE, "ergebnis_2026-11-08.txt"), "w",
          encoding="utf-8", newline="\n") as f:
    f.write(result + "\n")
