namespace StackVm.Core;

/// <summary>
/// Defines the bytecode operations supported by both virtual machines.
/// </summary>
public enum OpCode
{
    Push,
    Pop,
    Dup,
    Add,
    Sub,
    Mul,
    Div,
    Load,
    Store,
    Jump,
    JumpIfZero,
    Call,
    Return,
    Halt
}