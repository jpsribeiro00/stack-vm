using StackVm.Core;
using StackVm.SingleStack;
using StackVm.DualStack;
using Xunit;

namespace StackVm.Tests;

/// <summary>
/// Checks functional correctness and architectural equivalence.
/// </summary>
public sealed class VirtualMachineTests
{
    /// <summary>
    /// Checks arithmetic and simple calls in both architectures.
    /// </summary>
    [Fact]
    public void BasicPrograms_ProduceEquivalentResults()
    {
        AssertEquivalent(SamplePrograms.Arithmetic(), 36);
        AssertEquivalent(SamplePrograms.FunctionCall(), 42);
    }

    /// <summary>
    /// Checks recursive calls and local-variable isolation.
    /// </summary>
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(8, 40320)]
    public void Factorial_ProducesEquivalentResults(int n, int expected) => AssertEquivalent(SamplePrograms.Factorial(n), expected);

    /// <summary>
    /// Checks that invalid operand access is rejected.
    /// </summary>
    [Fact]
    public void OperandUnderflow_ThrowsInBothMachines()
    {
        var program = new VmProgram(new[] { new Instruction(OpCode.Pop), new Instruction(OpCode.Halt) });
        foreach (var vm in Machines())
            Assert.Throws<InvalidOperationException>(() => vm.Execute(program));
    }

    /// <summary>
    /// Checks the execution guard for an infinite loop.
    /// </summary>
    [Fact]
    public void InstructionLimit_StopsInfiniteLoops()
    {
        var program = new VmProgram(new[] { new Instruction(OpCode.Jump, 0) });
        foreach (var vm in Machines())
            Assert.Throws<InvalidOperationException>(() => vm.Execute(program, 100));
    }

    /// <summary>
    /// Checks that machines can be reused without leaking execution state.
    /// </summary>
    [Fact]
    public void RepeatedExecutions_ResetState()
    {
        foreach (var vm in Machines())
        {
            Assert.Equal(120, vm.Execute(SamplePrograms.Factorial(5)).Value);
            Assert.Equal(36, vm.Execute(SamplePrograms.Arithmetic()).Value);
        }
    }

    private static IVirtualMachine[] Machines() => [new SingleStackVirtualMachine(), new DualStackVirtualMachine()];

    private static void AssertEquivalent(VmProgram program, int expected)
    {
        var single = new SingleStackVirtualMachine().Execute(program);
        var dual = new DualStackVirtualMachine().Execute(program);

        Assert.Equal(expected, single.Value);
        Assert.Equal(single.Value, dual.Value);
        Assert.Equal(single.ExecutedInstructions, dual.ExecutedInstructions);
    }
}