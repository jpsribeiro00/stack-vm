namespace StackVm.Core;

/// <summary>
/// Provides deterministic bytecode programs for demonstrations and tests.
/// </summary>
public static class SamplePrograms
{
    /// <summary>
    /// Returns a program that computes (7 + 5) * 3.
    /// </summary>
    public static VmProgram Arithmetic() => new(new[]
    {
        new Instruction(OpCode.Push, 7),
        new Instruction(OpCode.Push, 5),
        new Instruction(OpCode.Add),
        new Instruction(OpCode.Push, 3),
        new Instruction(OpCode.Mul),
        new Instruction(OpCode.Halt)
    });

    /// <summary>
    /// Returns a program that invokes a function returning 42.
    /// </summary>
    public static VmProgram FunctionCall() => new(new[]
    {
        new Instruction(OpCode.Push, 0),
        new Instruction(OpCode.Call, 3),
        new Instruction(OpCode.Halt),
        new Instruction(OpCode.Push, 42),
        new Instruction(OpCode.Return)
    });

    /// <summary>
    /// Returns a program that computes factorial recursively using local slot zero.
    /// </summary>
    /// <param name="n">Nonnegative argument, at most 12 to avoid 32-bit overflow.</param>
    public static VmProgram Factorial(int n)
    {
        if (n < 0 || n > 12) throw new ArgumentOutOfRangeException(nameof(n));
        return new(new[]
        {
            new Instruction(OpCode.Push, n),        // 0
            new Instruction(OpCode.Call, 3),        // 1
            new Instruction(OpCode.Halt),           // 2
            new Instruction(OpCode.Store, 0),       // 3: factorial(n) receives n on stack
            new Instruction(OpCode.Load, 0),        // 4
            new Instruction(OpCode.JumpIfZero, 13), // 5
            new Instruction(OpCode.Load, 0),        // 6
            new Instruction(OpCode.Load, 0),        // 7
            new Instruction(OpCode.Push, 1),        // 8
            new Instruction(OpCode.Sub),            // 9
            new Instruction(OpCode.Call, 3),        // 10
            new Instruction(OpCode.Mul),            // 11
            new Instruction(OpCode.Return),         // 12
            new Instruction(OpCode.Push, 1),        // 13
            new Instruction(OpCode.Return)          // 14
        });
    }
}