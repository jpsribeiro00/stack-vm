namespace StackVm.Core;

/// <summary>
/// Captures operation counts and peak storage usage for one execution.
/// These measurements describe logical operations and allocated array slots,
/// not managed heap bytes or wall-clock time.
/// </summary>
public sealed record ExecutionMetrics(
    long DataPushes,
    long DataPops,
    long FrameMetadataPushes,
    long FrameMetadataPops,
    long LocalReads,
    long LocalWrites,
    long Calls,
    long Returns,
    int MaxCallDepth,
    int MaxDataStackSlots,
    int MaxReturnStackSlots,
    int MaxTotalStackSlots,
    int MaxAllocatedSlots,
    long StackResizes,
    long DataStackResizes,
    long ReturnStackResizes,
    int MaxDataAllocatedSlots,
    int MaxReturnAllocatedSlots);