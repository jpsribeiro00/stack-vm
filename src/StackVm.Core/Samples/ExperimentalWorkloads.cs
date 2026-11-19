namespace StackVm.Core;

/// <summary>
/// Creates deterministic workloads with known results for controlled comparisons.
/// </summary>
public static class ExperimentalWorkloads
{
    /// <summary>
    /// Creates n additions of one to an accumulator.
    /// </summary>
    public static VmProgram Arithmetic(int n)
    {
        Validate(n);
        var instructions = new List<Instruction>(checked(2 * n + 2)) { new(OpCode.Push, 0) };
        for (int i = 0; i < n; i++)
        {
            instructions.Add(new Instruction(OpCode.Push, 1));
            instructions.Add(new Instruction(OpCode.Add));
        }
        instructions.Add(new Instruction(OpCode.Halt));
        return new VmProgram(instructions, localCount: 0);
    }

    /// <summary>
    /// Creates n sequential calls to a function that returns its argument plus one.
    /// </summary>
    public static VmProgram FunctionCalls(int n)
    {
        Validate(n);
        var instructions = new List<Instruction>(checked(2 * n + 6)) { new(OpCode.Push, 0) };
        for (int i = 0; i < n; i++) instructions.Add(new Instruction(OpCode.Call, 0));
        instructions.Add(new Instruction(OpCode.Halt));
        int functionAddress = instructions.Count;
        for (int i = 1; i <= n; i++) instructions[i] = new Instruction(OpCode.Call, functionAddress);
        instructions.AddRange(new[] { new Instruction(OpCode.Push, 1), new Instruction(OpCode.Add), new Instruction(OpCode.Return) });
        return new VmProgram(instructions, localCount: 0);
    }

    /// <summary>
    /// Creates a recursive countdown that returns n, with call depth n + 1.
    /// </summary>
    public static VmProgram Recursion(int n)
    {
        Validate(n);
        return new VmProgram(new[]
        {
            new Instruction(OpCode.Push, n),        // 0
            new Instruction(OpCode.Call, 3),        // 1
            new Instruction(OpCode.Halt),           // 2
            new Instruction(OpCode.Store, 0),       // 3
            new Instruction(OpCode.Load, 0),        // 4
            new Instruction(OpCode.JumpIfZero, 14), // 5
            new Instruction(OpCode.Load, 0),        // 6
            new Instruction(OpCode.Push, 1),        // 7
            new Instruction(OpCode.Sub),            // 8
            new Instruction(OpCode.Call, 3),        // 9
            new Instruction(OpCode.Push, 1),        // 10
            new Instruction(OpCode.Add),            // 11
            new Instruction(OpCode.Return),         // 12
            new Instruction(OpCode.Jump, 14),       // 13 (unreachable)
            new Instruction(OpCode.Push, 0),        // 14
            new Instruction(OpCode.Return)          // 15
        }, localCount: 1);
    }

    /// <summary>
    /// Creates n iterations combining arithmetic with a function call.
    /// </summary>
    public static VmProgram Mixed(int n)
    {
        Validate(n);
        var instructions = new List<Instruction>(checked(4 * n + 5)) { new(OpCode.Push, 0) };
        for (int i = 0; i < n; i++)
        {
            instructions.Add(new Instruction(OpCode.Push, 1));
            instructions.Add(new Instruction(OpCode.Add));
            instructions.Add(new Instruction(OpCode.Call, 0));
        }
        instructions.Add(new Instruction(OpCode.Halt));
        int functionAddress = instructions.Count;
        for (int i = 3; i < functionAddress - 1; i += 3) instructions[i] = new Instruction(OpCode.Call, functionAddress);
        instructions.AddRange(new[] { new Instruction(OpCode.Push, 1), new Instruction(OpCode.Add), new Instruction(OpCode.Return) });
        return new VmProgram(instructions, localCount: 0);
    }

    /// <summary>
    /// Returns a workload selected by name.
    /// </summary>
    public static VmProgram Create(string name, int n) => name switch
    {
        "Arithmetic" => Arithmetic(n),
        "Calls" => FunctionCalls(n),
        "Recursion" => Recursion(n),
        "Mixed" => Mixed(n),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown workload.")
    };

    private static void Validate(int n)
    {
        if (n < 1 || n > 100_000) throw new ArgumentOutOfRangeException(nameof(n), "Expected 1..100000.");
    }
}