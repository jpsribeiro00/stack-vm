using StackVm.Core;

namespace StackVm.SingleStack;

/// <summary>
/// Executes bytecode using one physical stack for operands, locals and call metadata.
/// </summary>
public sealed class SingleStackVirtualMachine : VirtualMachineBase
{
    private IntStack _stack = null!;
    private int _frame;
    private long _metadataPushes, _metadataPops;

    private int OperandFloor => _frame + 2 + LocalCount;

    /// <inheritdoc />
    protected override bool IsRootFrame => _frame == 0;

    /// <inheritdoc />
    protected override void Initialize()
    {
        _stack = new IntStack(trackMetrics: CollectMetrics);
        _frame = 0;
        _metadataPushes = _metadataPops = 0;

        _stack.Push(-1); // return address
        _stack.Push(-1); // previous frame

        for (int i = 0; i < LocalCount; i++)
            _stack.Push(0);
    }

    /// <inheritdoc />
    protected override void Push(int value) => _stack.Push(value);

    /// <inheritdoc />
    protected override int Pop() => _stack.Count > OperandFloor ? _stack.Pop() : throw new InvalidOperationException("Operand stack underflow.");

    /// <inheritdoc />
    protected override int Peek() => _stack.Count > OperandFloor ? _stack.Peek() : throw new InvalidOperationException("Operand stack underflow.");

    /// <inheritdoc />
    protected override int ReadLocal(int index) => _stack[_frame + 2 + index];

    /// <inheritdoc />
    protected override void WriteLocal(int index, int value) => _stack[_frame + 2 + index] = value;

    /// <inheritdoc />
    protected override void EnterFrame(int returnAddress)
    {
        int nextFrame = _stack.Count;
        _stack.Push(returnAddress);
        _stack.Push(_frame);

        for (int i = 0; i < LocalCount; i++)
            _stack.Push(0);

        _frame = nextFrame;

        if (CollectMetrics)
            _metadataPushes += 2 + LocalCount;
    }

    /// <inheritdoc />
    protected override int LeaveFrame(int result)
    {
        int returnAddress = _stack[_frame];
        int previousFrame = _stack[_frame + 1];

        if (CollectMetrics)
            _metadataPops += 2 + LocalCount;

        _stack.Truncate(_frame);
        _frame = previousFrame;
        _stack.Push(result);

        return returnAddress;
    }

    /// <inheritdoc />
    protected override (long MetadataPushes, long MetadataPops, int DataPeak, int ReturnPeak, int TotalPeak, int AllocatedPeak, long Resizes) GetStorageMetrics()
        => (_metadataPushes, _metadataPops, _stack.PeakCount, 0, _stack.PeakCount, _stack.PeakCapacity, _stack.ResizeCount);
}