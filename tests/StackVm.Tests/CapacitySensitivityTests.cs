using StackVm.Core;
using StackVm.DualStack;
using StackVm.SingleStack;
using Xunit;

namespace StackVm.Tests;

/// <summary>
/// Verifies equivalent results and consistent buffer metrics
/// across different initial capacities.
/// </summary>
public sealed class CapacitySensitivityTests
{
    /// <summary>
    /// Verifies recursive execution and allocation invariants.
    /// </summary>
    [Theory]
    [InlineData(8, 100)]
    [InlineData(32, 1000)]
    [InlineData(128, 1000)]
    [InlineData(512, 10000)]
    public void Recursion_ProducesEquivalentResultsAndValidBufferMetrics(
        int capacity, int n)
    {
        var program = ExperimentalWorkloads.Recursion(n);

        var single = new SingleStackVirtualMachine(capacity)
            .Execute(program, collectMetrics: true);

        var dual = new DualStackVirtualMachine(capacity)
            .Execute(program, collectMetrics: true);

        Assert.Equal(n, single.Value);
        Assert.Equal(single.Value, dual.Value);
        Assert.Equal(single.ExecutedInstructions, dual.ExecutedInstructions);

        foreach (var metrics in new[] { single.Metrics!, dual.Metrics! })
        {
            Assert.True(metrics.MaxAllocatedSlots >= metrics.MaxTotalStackSlots);

            Assert.Equal(
                metrics.DataStackResizes + metrics.ReturnStackResizes,
                metrics.StackResizes);

            Assert.Equal(
                metrics.MaxDataAllocatedSlots + metrics.MaxReturnAllocatedSlots,
                metrics.MaxAllocatedSlots);

            Assert.Equal(n + 1, metrics.Calls);
        }

        Assert.Equal(0, single.Metrics!.ReturnStackResizes);
        Assert.True(dual.Metrics!.MaxReturnAllocatedSlots >= capacity);
    }

    /// <summary>
    /// Verifies that invalid initial capacities are rejected.
    /// </summary>
    [Fact]
    public void Constructors_RejectZeroCapacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SingleStackVirtualMachine(0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DualStackVirtualMachine(0));
    }
}
