namespace StackVm.Core;

/// <summary>
/// Represents the final result of executing a program.
/// </summary>
/// <param name="Value">The final value.</param>
/// <param name="ExecutedInstructions">The number of executed instructions.</param>
/// <param name="Metrics">Optional instrumentation data for this execution.</param>
public readonly record struct ExecutionResult(int Value, long ExecutedInstructions, ExecutionMetrics? Metrics = null);