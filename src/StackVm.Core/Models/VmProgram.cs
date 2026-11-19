namespace StackVm.Core;

/// <summary>
/// Contains immutable bytecode and the number of locals in each call frame.
/// </summary>
public sealed class VmProgram
{
    /// <summary>
    /// Gets the instructions.
    /// </summary>
    public IReadOnlyList<Instruction> Instructions { get; }

    /// <summary>
    /// Gets the number of local variables per frame.
    /// </summary>
    public int LocalCount { get; }

    /// <summary>
    /// Creates a bytecode program.
    /// </summary>
    /// <param name="instructions">Instructions in execution order.</param>
    /// <param name="localCount">Number of local slots in each frame.</param>
    public VmProgram(IEnumerable<Instruction> instructions, int localCount = 4)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        var copy = instructions.ToArray();
        if (copy.Length == 0) throw new ArgumentException("Program must not be empty.", nameof(instructions));
        if (localCount < 0) throw new ArgumentOutOfRangeException(nameof(localCount));

        Instructions = Array.AsReadOnly(copy);
        LocalCount = localCount;
    }
}