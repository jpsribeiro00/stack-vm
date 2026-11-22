using System;

namespace StackVm.Core;

/// <summary>
/// Provides the shared bytecode semantics while delegating frame layout to each architecture.
/// </summary>
public abstract class VirtualMachineBase : IVirtualMachine
{
    private VmProgram _program = null!;
    private int _ip;
    private long _steps;
    private bool _collectMetrics;
    private long _dataPushes, _dataPops, _localReads, _localWrites, _calls, _returns;
    private int _depth, _maxDepth;

    /// <summary>
    /// Whether metrics are enabled for the current execution.
    /// </summary>
    protected bool CollectMetrics => _collectMetrics;

    /// <summary>
    /// Gets a snapshot of architecture-specific storage metrics.
    /// </summary>
    protected abstract (long MetadataPushes, long MetadataPops, int DataPeak, int ReturnPeak, int TotalPeak, int AllocatedPeak, long Resizes) GetStorageMetrics();

    /// <summary>
    /// Gets the current program's number of local slots.
    /// </summary>
    protected int LocalCount => _program.LocalCount;

    /// <summary>
    /// Gets the current instruction address.
    /// </summary>
    protected int InstructionPointer => _ip;

    /// <summary>
    /// Initializes machine-specific stacks for a new execution.
    /// </summary>
    protected abstract void Initialize();

    /// <summary>
    /// Pushes an operand.
    /// </summary>
    protected abstract void Push(int value);

    /// <summary>
    /// Pops an operand, rejecting access to frame metadata.
    /// </summary>
    protected abstract int Pop();

    /// <summary>
    /// Reads an operand without removing it.
    /// </summary>
    protected abstract int Peek();

    /// <summary>
    /// Reads a local variable.
    /// </summary>
    protected abstract int ReadLocal(int index);

    /// <summary>
    /// Writes a local variable.
    /// </summary>
    protected abstract void WriteLocal(int index, int value);

    /// <summary>
    /// Enters a new frame, saving the return address.
    /// </summary>
    protected abstract void EnterFrame(int returnAddress);

    /// <summary>
    /// Leaves the current frame and returns its saved return address.
    /// </summary>
    protected abstract int LeaveFrame(int result);

    /// <summary>
    /// Returns whether the current frame is the root frame.
    /// </summary>
    protected abstract bool IsRootFrame { get; }

    /// <inheritdoc />
    public ExecutionResult Execute(VmProgram program, long maxInstructions = 10_000_000, bool collectMetrics = false)
    {
        ArgumentNullException.ThrowIfNull(program);
        if (maxInstructions < 1) throw new ArgumentOutOfRangeException(nameof(maxInstructions));

        _program = program;
        _ip = 0;
        _steps = 0;
        _collectMetrics = collectMetrics;
        _dataPushes = _dataPops = _localReads = _localWrites = _calls = _returns = 0;
        _depth = _maxDepth = 0;

        Initialize();

        while (true)
        {
            if (_ip < 0 || _ip >= program.Instructions.Count) throw new InvalidOperationException("Instruction pointer out of range.");
            if (_steps >= maxInstructions) throw new InvalidOperationException("Instruction limit exceeded.");

            var instruction = program.Instructions[_ip++];
            _steps++;

            // These counters are logical bytecode operations, not physical array writes.
            switch (instruction.Code)
            {
                case OpCode.Push:
                    PushData(instruction.Operand);
                    break;
                case OpCode.Pop:
                    PopData();
                    break;
                case OpCode.Dup:
                    PushData(Peek());
                    break;
                case OpCode.Add:
                    {
                        int b = PopData(), a = PopData();
                        PushData(checked(a + b));
                        break;
                    }
                case OpCode.Sub:
                    {
                        int b = PopData(), a = PopData();
                        PushData(checked(a - b));
                        break;
                    }
                case OpCode.Mul:
                    {
                        int b = PopData(), a = PopData();
                        PushData(checked(a * b));
                        break;
                    }
                case OpCode.Div:
                    {
                        int b = PopData(), a = PopData();
                        PushData(a / b);
                        break;
                    }
                case OpCode.Load:
                    ValidateLocal(instruction.Operand);
                    if (_collectMetrics) _localReads++;
                    PushData(ReadLocal(instruction.Operand));
                    break;
                case OpCode.Store:
                    ValidateLocal(instruction.Operand);
                    WriteLocal(instruction.Operand, PopData());
                    if (_collectMetrics) _localWrites++;
                    break;
                case OpCode.Jump:
                    _ip = ValidateAddress(instruction.Operand);
                    break;
                case OpCode.JumpIfZero:
                    if (PopData() == 0) _ip = ValidateAddress(instruction.Operand);
                    break;
                case OpCode.Call:
                    {
                        int target = ValidateAddress(instruction.Operand);
                        int argument = PopData();
                        EnterFrame(_ip);
                        PushData(argument);
                        _ip = target;
                        if (_collectMetrics)
                        {
                            _calls++;
                            _depth++;
                            _maxDepth = Math.Max(_maxDepth, _depth);
                        }
                        break;
                    }
                case OpCode.Return:
                    {
                        int result = PopData();
                        if (IsRootFrame) return Complete(result);
                        _ip = LeaveFrame(result);
                        if (_collectMetrics)
                        {
                            _returns++;
                            _depth--;
                        }
                        break;
                    }
                case OpCode.Halt:
                    if (!IsRootFrame) throw new InvalidOperationException("HALT is only valid in the root frame.");
                    return Complete(PopData());
                default:
                    throw new InvalidOperationException($"Unsupported opcode: {instruction.Code}.");
            }
        }
    }

    /// <summary>
    /// Gets allocation and resize metrics for each physical stack.
    /// </summary>
    /// <returns>
    /// Per-buffer resize counts and maximum capacities.
    /// </returns>
    protected abstract (long DataResizes, long ReturnResizes, int DataCapacity, int ReturnCapacity) GetBufferMetrics();

    private void PushData(int value)
    {
        Push(value);
        if (_collectMetrics) _dataPushes++;
    }

    private int PopData()
    {
        int value = Pop();
        if (_collectMetrics) _dataPops++;
        return value;
    }

    private ExecutionResult Complete(int value)
    {
        if (!_collectMetrics) return new ExecutionResult(value, _steps);
        var storage = GetStorageMetrics();
        var buffers = GetBufferMetrics();

        return new ExecutionResult(value, _steps, 
            new ExecutionMetrics(
            _dataPushes,
            _dataPops,
            storage.MetadataPushes,
            storage.MetadataPops,
            _localReads,
            _localWrites,
            _calls,
            _returns,
            _maxDepth,
            storage.DataPeak,
            storage.ReturnPeak,
            storage.TotalPeak,
            storage.AllocatedPeak,
            storage.Resizes,
            buffers.DataResizes,
            buffers.ReturnResizes,
            buffers.DataCapacity,
            buffers.ReturnCapacity
        ));
    }

    private void ValidateLocal(int index)
    {
        if (index < 0 || index >= LocalCount) throw new InvalidOperationException("Invalid local variable index.");
    }

    private int ValidateAddress(int address)
    {
        if (address < 0 || address >= _program.Instructions.Count) throw new InvalidOperationException("Invalid jump target.");
        return address;
    }
}