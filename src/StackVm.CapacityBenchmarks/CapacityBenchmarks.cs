using BenchmarkDotNet.Attributes;
using StackVm.Core;
using StackVm.DualStack;
using StackVm.SingleStack;

namespace StackVm.CapacityBenchmarks;

/// <summary>
/// Measures the impact of initial stack capacity
/// on recursive execution and memory allocation.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[CsvExporter]
public class CapacityBenchmarks
{
    private VmProgram _program = null!;
    private SingleStackVirtualMachine _single = null!;
    private DualStackVirtualMachine _dual = null!;

    /// <summary>
    /// Gets or sets the recursion depth.
    /// </summary>
    [Params(100, 1000, 10000)]
    public int N { get; set; }

    /// <summary>
    /// Gets or sets the initial capacity of each physical stack.
    /// </summary>
    [Params(8, 32, 128, 512)]
    public int InitialCapacity { get; set; }

    /// <summary>
    /// Initializes the workloads and validates their results.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _program = ExperimentalWorkloads.Recursion(N);
        _single = new SingleStackVirtualMachine(InitialCapacity);
        _dual = new DualStackVirtualMachine(InitialCapacity);

        if (_single.Execute(_program).Value != N ||
            _dual.Execute(_program).Value != N)
        {
            throw new InvalidOperationException(
                "Incorrect recursive workload result.");
        }
    }

    /// <summary>
    /// Measures execution with a unified stack.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int SingleStack() => _single.Execute(_program).Value;

    /// <summary>
    /// Measures execution with separate stacks.
    /// </summary>
    [Benchmark]
    public int DualStack() => _dual.Execute(_program).Value;
}
