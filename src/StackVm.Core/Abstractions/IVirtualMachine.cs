namespace StackVm.Core;

/// <summary>
/// Defines the common contract for stack virtual machines.
/// </summary>
public interface IVirtualMachine
{
    /// <summary>
    /// Executes a program from instruction zero with fresh machine state.
    /// </summary>
    /// <param name="program">The program to execute.</param>
    /// <param name="maxInstructions">Safety limit preventing unbounded execution.</param>
    /// <param name="collectMetrics">Whether to collect optional execution metrics.</param>
    /// <returns>The execution result.</returns>
    ExecutionResult Execute(VmProgram program, long maxInstructions = 10_000_000, bool collectMetrics = false);
}