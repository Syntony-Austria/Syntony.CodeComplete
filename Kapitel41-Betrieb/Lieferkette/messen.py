"""Supply-chain measurements for chapter 41. Offline except the audit part.

Packs two harmless packages with the same id and version (Library.Lending
1.2.0) into two local feeds, "intern" and "fremd", then restores the
consumer project under different nuget.config files into fresh package
folders and reports which feed each restore used (from .nupkg.metadata).

Run from this folder: python messen.py
The audit part needs the network: python messen.py --audit
"""
import json
import os
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
WORK = tempfile.mkdtemp(prefix="k41-")
FEEDS = os.path.join(WORK, "feeds")
ENV = dict(os.environ, FEEDS=FEEDS.replace("\\", "/"),
           DOTNET_CLI_UI_LANGUAGE="en", DOTNET_NOLOGO="1")


def run(*args, cwd=HERE):
    p = subprocess.run(args, cwd=cwd, env=ENV, capture_output=True,
                       text=True, encoding="utf-8", errors="replace")
    return p.returncode, p.stdout + p.stderr


def pack(who):
    out = os.path.join(FEEDS, who.lower())
    code, log = run("dotnet", "pack", os.path.join("Pakete", who),
                    "-c", "Release", "-o", out)
    if code != 0:
        sys.exit(log)


def config_copy(name, mode=None, swap=False, drop=None):
    """Copy a config; optionally change the mode, swap the two sources
    or drop one of them (an unreachable feed)."""
    text = open(os.path.join(HERE, "Konfiguration", name),
                encoding="utf-8").read()
    if mode:
        text = text.replace('value="accept"', f'value="{mode}"')
    a = '    <add key="intern" value="%FEEDS%/intern" />\n'
    b = '    <add key="fremd" value="%FEEDS%/fremd" />\n'
    if swap:
        text = text.replace(a + b, b + a)
    if drop:
        text = text.replace(f"%FEEDS%/{drop}", "%FEEDS%/offline")
    tag = f"{mode}-{swap}-{drop}"
    path = os.path.join(WORK, f"{name}.{tag}.config")
    open(path, "w", encoding="utf-8").write(text)
    return path


def restore(config, packages):
    code, log = run("dotnet", "restore", os.path.join("Verbraucher"),
                    "--force", "--no-http-cache", "--configfile", config,
                    "--packages", packages)
    meta = os.path.join(packages, "library.lending", "1.2.0",
                        ".nupkg.metadata")
    feed = "-"
    if os.path.exists(meta):
        source = json.load(open(meta, encoding="utf-8")).get("source", "")
        feed = os.path.basename(source.rstrip("/\\")) or source
    codes = sorted({w.rstrip(":") for w in log.split()
                    if w.startswith("NU") and w[2:6].isdigit()})
    return code, feed, ",".join(codes) or "-"


def fresh():
    return tempfile.mkdtemp(prefix="pkgs-", dir=WORK)


def main():
    pack("Intern")
    pack("Fremd")
    plain = config_copy("ohne-zuordnung.config")
    swapped = config_copy("ohne-zuordnung.config", swap=True)
    plain_off = config_copy("ohne-zuordnung.config", drop="intern")
    mapped_off = config_copy("nuget.config", drop="intern")
    os.makedirs(os.path.join(FEEDS, "offline"))
    mapped = config_copy("nuget.config")
    require = config_copy("nuget.config", "require")

    rows = []
    for i in range(10):
        rows.append(("ohne Zuordnung, frischer Ordner", i + 1,
                     *restore(plain, fresh())))
    for i in range(20):
        rows.append(("ohne Zuordnung, fremd zuerst", i + 1,
                     *restore(swapped, fresh())))
    rows.append(("ohne Zuordnung, intern leer", 1,
                 *restore(plain_off, fresh())))
    rows.append(("mit Zuordnung, intern leer", 1,
                 *restore(mapped_off, fresh())))
    for i in range(10):
        rows.append(("mit Zuordnung, frischer Ordner", i + 1,
                     *restore(mapped, fresh())))
    # The limit: the package is already in the global packages folder.
    shared = fresh()
    rows.append(("ohne Zuordnung, fremd zuerst, Ordner A", 1,
                 *restore(swapped, shared)))
    rows.append(("mit Zuordnung, derselbe Ordner A", 1,
                 *restore(mapped, shared)))
    rows.append(("signatureValidationMode=require", 1,
                 *restore(require, fresh())))
    rows.append(("signatureValidationMode=accept", 1,
                 *restore(mapped, fresh())))

    print("case | run | exit | feed | codes")
    for r in rows:
        print(" | ".join(str(c) for c in r))
    shutil.rmtree(WORK, ignore_errors=True)


def audit():
    """NuGetAudit on Audit/Audit.csproj under five settings."""
    nu = "NU1901%3BNU1902%3BNU1903%3BNU1904"
    cases = [("default (net10.0: all)", []),
             ("NuGetAuditMode=direct", ["-p:NuGetAuditMode=direct"]),
             ("TreatWarningsAsErrors", ["-p:TreatWarningsAsErrors=true"]),
             ("... plus WarningsNotAsErrors",
              ["-p:TreatWarningsAsErrors=true", f"-p:WarningsNotAsErrors={nu}"]),
             ("NuGetAudit=false", ["-p:NuGetAudit=false"])]
    print("case | exit | codes")
    for name, extra in cases:
        code, log = run("dotnet", "build", "Audit", "--no-incremental",
                        "-p:RestoreForce=true", *extra)
        found = sorted({w.rstrip(":") for w in log.split()
                        if w.startswith("NU19")})
        print(f"{name} | {code} | {','.join(found) or '-'}")
    for extra in ([], ["--include-transitive"]):
        code, log = run("dotnet", "list", "Audit", "package", "--vulnerable",
                        *extra)
        print("list --vulnerable", *extra, "|", code, "|",
              "High" if " High " in log else "none")


if __name__ == "__main__":
    if "--audit" in sys.argv:
        audit()
    else:
        main()
