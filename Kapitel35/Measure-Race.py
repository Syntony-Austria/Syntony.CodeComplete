"""How many threads and rounds does it take until a race shows itself?
Usage: python Measure-Race.py <work folder>"""
import pathlib, re, shutil, subprocess, sys

PROGRAM = """
int Run(int threads, int rounds)
{
    int counter = 0;
    Thread[] all = Enumerable.Range(0, threads).Select(_ => new Thread(() =>
    {
        for (int i = 0; i < rounds; i++) { counter++; }
    })).ToArray();
    foreach (Thread t in all) { t.Start(); }
    foreach (Thread t in all) { t.Join(); }
    return threads * rounds - counter;
}

foreach ((int threads, int rounds) in new[]
    { (1, 1_000_000), (2, 100), (2, 1000), (2, 100_000), (4, 100_000), (8, 1_000_000) })
{
    int[] lost = Enumerable.Range(0, 100).Select(_ => Run(threads, rounds)).ToArray();
    Console.WriteLine($"{threads};{rounds};{lost.Count(x => x > 0)};{lost.Max()}");
}
"""
PROJECT = """<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>"""
work = pathlib.Path(sys.argv[1])
if work.exists():
    shutil.rmtree(work)
work.mkdir(parents=True)
(work / "p.csproj").write_text(PROJECT)
(work / "Program.cs").write_text(PROGRAM)
print("threads;rounds;runs that lost updates of 100;most lost")
print(subprocess.run(["dotnet", "run", "-c", "Release", "--project", str(work)],
                     capture_output=True, text=True).stdout)
