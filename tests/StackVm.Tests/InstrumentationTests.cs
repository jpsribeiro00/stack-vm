using StackVm.Core;
using StackVm.SingleStack;
using StackVm.DualStack;
using Xunit;

namespace StackVm.Tests;

/// <summary>
/// Verifies optional instrumentation and per-run isolation.
/// </summary>
public sealed class InstrumentationTests
{
    /// <summary>
    /// Checks that instrumentation does not change functional results.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(8)]
    public void Metrics_DoNotChangeResults(int n)
    {
        foreach (var vm in Machines())
        {
            var program = SamplePrograms.Factorial(n);
            var plain = vm.Execute(program);
            var tracked = vm.Execute(program, collectMetrics: true);

            Assert.Equal(plain.Value, tracked.Value);
            Assert.Equal(plain.ExecutedInstructions, tracked.ExecutedInstructions);
            Assert.Null(plain.Metrics);
            Assert.NotNull(tracked.Metrics);
            Assert.Equal(n + 1, tracked.Metrics.Calls);
            Assert.Equal(n + 1, tracked.Metrics.Returns);
            Assert.Equal(n + 1, tracked.Metrics.MaxCallDepth);
            Assert.True(tracked.Metrics.MaxTotalStackSlots > 0);
            Assert.True(tracked.Metrics.MaxAllocatedSlots >= tracked.Metrics.MaxTotalStackSlots);
        }
    }

    /// <summary>
    /// Checks that both machines count the same logical operations.
    /// </summary>
    [Fact]
    public void LogicalCounters_AreEquivalent()
    {
        var program = SamplePrograms.Factorial(5);
        var a = new SingleStackVirtualMachine().Execute(program, collectMetrics: true).Metrics!;
        var b = new DualStackVirtualMachine().Execute(program, collectMetrics: true).Metrics!;

        Assert.Equal(a.DataPushes, b.DataPushes);
        Assert.Equal(a.DataPops, b.DataPops);
        Assert.Equal(a.LocalReads, b.LocalReads);
        Assert.Equal(a.LocalWrites, b.LocalWrites);
        Assert.Equal(a.Calls, b.Calls);
        Assert.Equal(a.Returns, b.Returns);
        Assert.Equal(a.MaxCallDepth, b.MaxCallDepth);
        Assert.True(b.MaxReturnStackSlots > 0);
        Assert.Equal(0, a.MaxReturnStackSlots);
    }

    /// <summary>
    /// Checks that a second execution does not inherit prior counters.
    /// </summary>
    [Fact]
    public void Metrics_ResetBetweenExecutions()
    {
        foreach (var vm in Machines())
        {
            vm.Execute(SamplePrograms.Factorial(8), collectMetrics: true);
            var fresh = vm.Execute(SamplePrograms.Arithmetic(), collectMetrics: true).Metrics!;

            Assert.Equal(0, fresh.Calls);
            Assert.Equal(0, fresh.Returns);
            Assert.Equal(0, fresh.MaxCallDepth);
        }
    }

    private static IVirtualMachine[] Machines() => [new SingleStackVirtualMachine(), new DualStackVirtualMachine()];
}