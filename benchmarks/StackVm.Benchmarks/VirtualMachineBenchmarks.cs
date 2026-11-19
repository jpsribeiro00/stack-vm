using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Order;

using StackVm.Core;
using StackVm.DualStack;
using StackVm.SingleStack;

namespace StackVm.Benchmarks;

/// <summary>
/// Benchmarks equivalent bytecode on two independent VM architectures.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.Method)]
[CsvExporter(CsvSeparator.Comma)]
public class VirtualMachineBenchmarks
{
    private VmProgram _program = null!;
    private readonly SingleStackVirtualMachine _single = new();
    private readonly DualStackVirtualMachine _dual = new();
    private int _expected;

    /// <summary>
    /// Gets or sets the workload family.
    /// </summary>
    [Params("Arithmetic", "Calls", "Recursion", "Mixed")]

    public string Workload { get; set; } = "Arithmetic";

    /// <summary>
    /// Gets or sets the workload size.
    /// </summary>
    [Params(100, 1000, 10000)]
    public int N { get; set; }

    /// <summary>
    /// Prepares bytecode outside the measured invocation and validates both implementations.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _program = ExperimentalWorkloads.Create(Workload, N);
        _expected = Workload == "Mixed" ? checked(2 * N) : N;
        var a = _single.Execute(_program);
        var b = _dual.Execute(_program);
        if (a.Value != _expected || b.Value != _expected || a.ExecutedInstructions != b.ExecutedInstructions)
            throw new InvalidOperationException("Workloads are not semantically equivalent.");
    }

    /// <summary>
    /// Measures the uninstrumented single-stack interpreter.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int SingleStack() => _single.Execute(_program).Value;

    /// <summary>
    /// Measures the uninstrumented dual-stack interpreter.
    /// </summary>
    [Benchmark]
    public int DualStack() => _dual.Execute(_program).Value;
}
