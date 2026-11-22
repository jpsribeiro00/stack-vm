using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Order;

using StackVm.Core;
using StackVm.DualStack;
using StackVm.SingleStack;

namespace StackVm.Benchmarks;

/// <summary>
/// Compares sequential function calls with mixed arithmetic
/// and function-call workloads across two stack architectures.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.Method)]
[CsvExporter(CsvSeparator.Comma)]
public class CallAndMixedBenchmarks
{
    private VmProgram _program = null!;
    private readonly SingleStackVirtualMachine _single = new();
    private readonly DualStackVirtualMachine _dual = new();

    /// <summary>
    /// Gets or sets the workload used by the experiment.
    /// </summary>
    [Params("Calls", "Mixed")]
    public string Workload { get; set; } = "Calls";

    /// <summary>
    /// Gets or sets the number of workload iterations.
    /// </summary>
    [Params(100, 1000)]
    public int N { get; set; }

    /// <summary>
    /// Prepares the bytecode and verifies that both architectures
    /// produce equivalent results and instruction counts.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _program = ExperimentalWorkloads.Create(Workload, N);

        int expected = Workload == "Mixed"
            ? checked(2 * N)
            : N;

        var single = _single.Execute(_program);
        var dual = _dual.Execute(_program);

        if (single.Value != expected ||
            dual.Value != expected ||
            single.ExecutedInstructions != dual.ExecutedInstructions)
        {
            throw new InvalidOperationException(
                $"Workload validation failed: {Workload}, N={N}.");
        }
    }

    /// <summary>
    /// Measures the complete execution using a unified stack.
    /// </summary>
    /// <returns>The resulting accumulator value.</returns>
    [Benchmark(Baseline = true)]
    public int SingleStack()
    {
        return _single.Execute(_program).Value;
    }

    /// <summary>
    /// Measures the complete execution using separate
    /// data and return stacks.
    /// </summary>
    /// <returns>The resulting accumulator value.</returns>
    [Benchmark]
    public int DualStack()
    {
        return _dual.Execute(_program).Value;
    }
}
