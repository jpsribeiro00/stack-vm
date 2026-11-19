namespace StackVm.Core;

/// <summary>
/// Represents a bytecode instruction and its optional integer operand.
/// </summary>
/// <param name="Code">The operation.</param>
/// <param name="Operand">An immediate value, local index, or absolute instruction address.</param>
public readonly record struct Instruction(OpCode Code, int Operand = 0);