using BenchmarkDotNet.Running;

using StackVm.Benchmarks;

BenchmarkSwitcher.FromAssembly(
    typeof(CallAndMixedBenchmarks).Assembly
).Run(args);
