using StackVm.Core;

namespace StackVm.DualStack;

/// <summary>
/// Executes bytecode with a data stack and a separate return-frame stack.
/// </summary>
public sealed class DualStackVirtualMachine : VirtualMachineBase
{
    private IntStack _data = null!;
    private IntStack _returns = null!;
    private int _frame;
    private long _metadataPushes, _metadataPops;
    private int _maxCombinedSlots;

    private int OperandFloor => _frame + LocalCount;

    /// <inheritdoc />
    protected override bool IsRootFrame => _returns.Count == 0;

    /// <inheritdoc />
    protected override void Initialize()
    {
        _data = new IntStack(trackMetrics: CollectMetrics);
        _returns = new IntStack(trackMetrics: CollectMetrics);
        _frame = 0;
        _metadataPushes = _metadataPops = 0;
        _maxCombinedSlots = 0;

        for (int i = 0; i < LocalCount; i++)
            _data.Push(0);
    }

    /// <inheritdoc />
    protected override void Push(int value)
    {
        _data.Push(value);
        ObserveCombinedSlots();
    }

    /// <inheritdoc />
    protected override int Pop() => _data.Count > OperandFloor ? _data.Pop() : throw new InvalidOperationException("Operand stack underflow.");

    /// <inheritdoc />
    protected override int Peek() => _data.Count > OperandFloor ? _data.Peek() : throw new InvalidOperationException("Operand stack underflow.");

    /// <inheritdoc />
    protected override int ReadLocal(int index) => _data[_frame + index];

    /// <inheritdoc />
    protected override void WriteLocal(int index, int value) => _data[_frame + index] = value;

    /// <inheritdoc />
    protected override void EnterFrame(int returnAddress)
    {
        _returns.Push(returnAddress);
        _returns.Push(_frame);
        int nextFrame = _data.Count;

        for (int i = 0; i < LocalCount; i++)
            _data.Push(0);

        _frame = nextFrame;

        if (CollectMetrics)
            _metadataPushes += 2 + LocalCount;

        ObserveCombinedSlots();
    }

    /// <inheritdoc />
    protected override int LeaveFrame(int result)
    {
        if (CollectMetrics)
            _metadataPops += 2 + LocalCount;

        _data.Truncate(_frame);
        _frame = _returns.Pop();
        int returnAddress = _returns.Pop();
        _data.Push(result);

        return returnAddress;
    }

    private void ObserveCombinedSlots()
    {
        if (CollectMetrics)
            _maxCombinedSlots = Math.Max(_maxCombinedSlots, _data.Count + _returns.Count);
    }

    /// <inheritdoc />
    protected override (long MetadataPushes, long MetadataPops, int DataPeak, int ReturnPeak, int TotalPeak, int AllocatedPeak, long Resizes) GetStorageMetrics()
        => (_metadataPushes, _metadataPops, _data.PeakCount, _returns.PeakCount, _maxCombinedSlots,
            _data.PeakCapacity + _returns.PeakCapacity, _data.ResizeCount + _returns.ResizeCount);
}