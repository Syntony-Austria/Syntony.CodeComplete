"""Measures which library changes break old compiled callers (binary) and which break a rebuild (source).

Usage: python Measure-Breaks.py <empty scratch folder>
Each case: build app against v1, swap in only the new library, run; then rebuild the app against v2.
"""
import os, shutil, subprocess, sys

ROOT = sys.argv[1]
CSPROJ = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable>%s</PropertyGroup>%s</Project>'
BASE = """[assembly: System.Reflection.AssemblyVersion("1.5.0.0")]
namespace Rules;
public sealed record Tariff(int Cents);
public sealed class Options { public int Cents { get; init; } = 25; }
public interface IRule { int Days(); }
public enum Kind { Book, Journal }
public class Fees
{
    public const int Grace = 3;
    public static int For(int days) => days * 25;
    public static int Named(int daysLate) => daysLate;
    public static int Wide() => 1;
    public static string Pick(string text) => text;
    public static string Describe(Kind kind) => kind.ToString();
    public static int Gone() => 1;
    public static string? Note() => null;
    public static string Name() => "x";
    public static int WithDefault(int rate = 25) => rate;
}
"""
APP = """using Rules;
public sealed class MyRule : IRule { public int Days() => 3; }
public sealed class MyFees : Fees { }
public static class Program
{
    public static void Main()
    {
        System.Console.Write(%s);
    }
}
"""
# The library sets its own AssemblyVersion, so the SDK must not add one.
LIB_PROPS = ("<GenerateAssemblyVersionAttribute>false"
             "</GenerateAssemblyVersionAttribute>")
OBSOLETE_OLD = "public static int Gone() => 1;"
OBSOLETE_WARN = '[System.Obsolete("Use For.")] '
OBSOLETE_ERROR = '[System.Obsolete("Use For.", true)] '
CASES = [
    ("neue Methode", "public static int Gone() => 1;", "public static int Gone() => 1; public static int More() => 2;", "Fees.For(1)"),
    ("Parameter umbenannt", "Named(int daysLate) => daysLate", "Named(int late) => late", "Fees.Named(daysLate: 1)"),
    ("optionaler Parameter ergänzt", "For(int days) => days * 25", "For(int days, int rate = 25) => days * rate", "Fees.For(1)"),
    ("Rückgabetyp erweitert", "public static int Wide() => 1;", "public static long Wide() => 1;", "(int)Fees.Wide() + Wider()"),
    ("Methode entfernt", "public static int Gone() => 1;", "", "Fees.Gone()"),
    ("Mitglied im Interface", "int Days(); }", "int Days(); int Weeks(); }", "new MyRule().Days()"),
    ("Interface mit Standardumsetzung", "int Days(); }", "int Days(); int Weeks() => 1; }", "new MyRule().Days()"),
    ("Klasse versiegelt", "public class Fees", "public sealed class Fees", "new MyFees().ToString() is null"),
    ("Überladung macht Aufruf mehrdeutig", "public static string Pick(string text) => text;", "public static string Pick(string text) => text; public static string Pick(System.Uri uri) => uri.ToString();", "Fees.Pick(null!) is null"),
    ("Konstante geändert", "Grace = 3", "Grace = 7", "Fees.Grace"),
    ("Record bekommt ein Feld", "record Tariff(int Cents);", "record Tariff(int Cents, int Vat = 0);", "new Tariff(1).Cents"),
    ("Klasse mit init bekommt eine Eigenschaft", "= 25; }", "= 25; public int Vat { get; init; } }", "new Options().Cents"),
    ("Standardwert geändert", "int rate = 25) => rate", "int rate = 30) => rate", "Fees.WithDefault()"),
    ("Enum-Wert ergänzt", "Book, Journal }", "Book, Journal, Map }", "Fees.Describe(Kind.Book)"),
    ("Assembly-Version höher", '"1.5.0.0"', '"2.0.0.0"', "Fees.For(1)"),
    ("Assembly-Version niedriger", '"1.5.0.0"', '"1.0.0.0"', "Fees.For(1)"),
    ("Rückgabe wird string?", "string Name()", "string? Name()",
     "Fees.Name().Length"),
    ("[Obsolete] ergänzt", OBSOLETE_OLD, OBSOLETE_WARN + OBSOLETE_OLD,
     "Fees.Gone()"),
    ("[Obsolete] als Fehler", OBSOLETE_OLD, OBSOLETE_ERROR + OBSOLETE_OLD,
     "Fees.Gone()"),
]


CASES_NULL = None


def sh(args, cwd):
    return subprocess.run(args, cwd=cwd, capture_output=True, text=True, encoding="utf-8", errors="replace")


def setup(case_dir, lib_source, expr):
    lib, app = os.path.join(case_dir, "Lib"), os.path.join(case_dir, "App")
    os.makedirs(lib), os.makedirs(app)
    open(os.path.join(lib, "Lib.csproj"), "w").write(CSPROJ % (LIB_PROPS, ""))
    open(os.path.join(lib, "F.cs"), "w", encoding="utf-8").write(lib_source)
    ref = '<ItemGroup><ProjectReference Include="../Lib/Lib.csproj"/></ItemGroup>'
    open(os.path.join(app, "App.csproj"), "w").write(CSPROJ % ("<OutputType>Exe</OutputType>", ref))
    code = APP % expr
    if "Wider()" in expr:
        code = code.replace("    public static void Main()", "    private static int Wider() => 0;\n    public static void Main()")
    open(os.path.join(app, "P.cs"), "w", encoding="utf-8").write(code)
    return lib, app


print("Fall | binär | Quelltext")
for i, (name, old, new, expr) in enumerate(CASES):
    d = os.path.join(ROOT, f"c{i}")
    lib, app = setup(d, BASE, expr)
    assert BASE.count(old) >= 1, name
    r = sh(["dotnet", "build", "--nologo", "-v", "q", "-o", os.path.join(d, "out")], app)
    if r.returncode != 0:
        print(name, "| APP1 BUILD FAILED", r.stdout[-300:])
        continue
    before = sh(["dotnet", os.path.join(d, "out", "App.dll")], d).stdout
    open(os.path.join(lib, "F.cs"), "w", encoding="utf-8").write(BASE.replace(old, new, 1))
    r = sh(["dotnet", "build", "--nologo", "-v", "q", "-o", os.path.join(d, "l2")], lib)
    shutil.copy(os.path.join(d, "l2", "Lib.dll"), os.path.join(d, "out", "Lib.dll"))
    run = sh(["dotnet", os.path.join(d, "out", "App.dll")], d)
    binary = run.stdout if run.returncode == 0 else next((w.strip(":,.") for w in (run.stderr + run.stdout).split() if w.endswith("Exception:") or w.endswith("Exception")), "Abbruch")
    if run.returncode == 0 and run.stdout != before:
        binary = f"läuft, Ergebnis {before} wird {run.stdout}"
    elif run.returncode == 0:
        binary = f"läuft ({run.stdout})"
    shutil.rmtree(os.path.join(app, "bin"), ignore_errors=True)
    shutil.rmtree(os.path.join(app, "obj"), ignore_errors=True)
    r = sh(["dotnet", "build", "--nologo", "-v", "q", "-o", os.path.join(d, "out2")], app)
    if r.returncode == 0:
        after = sh(["dotnet", os.path.join(d, "out2", "App.dll")], d).stdout
        warned = sorted({w.rstrip(":") for w in r.stdout.split()
                         if w.startswith("CS") and w.rstrip(":")[2:].isdigit()})
        source = f"übersetzt ({after})" + "".join(", Warnung " + w for w in warned)
    else:
        codes = sorted({w.rstrip(":") for w in r.stdout.split() if w.startswith("CS") and w.rstrip(":")[2:].isdigit()})
        source = ", ".join(codes)
    print(f"{name} | {binary} | {source}", flush=True)
