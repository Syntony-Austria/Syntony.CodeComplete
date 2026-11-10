using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Library.Benchmarks.TextBenchmarks).Assembly)
    .Run(args);
