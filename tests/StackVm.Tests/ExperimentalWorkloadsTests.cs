using StackVm.Core;
using StackVm.SingleStack;
using StackVm.DualStack;
using Xunit;

namespace StackVm.Tests;

/// <summary>
/// Checks correctness and equivalence of the benchmark workloads.
/// </summary>
public sealed class ExperimentalWorkloadsTests
{
    /// <summary>
    /// Checks all workload families at representative sizes.
    /// </summary>
    [Theory]
    [InlineData("Arithmetic", 1)]
    [InlineData("Arithmetic", 100)]
    [InlineData("Calls", 1)]
    [InlineData("Calls", 100)]
    [InlineData("Recursion", 1)]
    [InlineData("Recursion", 100)]
    [InlineData("Mixed", 1)]
    [InlineData("Mixed", 100)]
    public void Workload_ReturnsSameExpectedValueAndInstructionCount(string name, int n)
    {
        var program = ExperimentalWorkloads.Create(name, n);
        var a = new SingleStackVirtualMachine().Execute(program, collectMetrics: true);
        var b = new DualStackVirtualMachine().Execute(program, collectMetrics: true);

        int expected = name == "Mixed" ? 2 * n : n;

        Assert.Equal(expected, a.Value);
        Assert.Equal(expected, b.Value);
        Assert.Equal(a.ExecutedInstructions, b.ExecutedInstructions);
        Assert.Equal(a.Metrics!.Calls, b.Metrics!.Calls);
    }
}