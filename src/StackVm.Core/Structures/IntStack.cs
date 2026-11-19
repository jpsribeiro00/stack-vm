namespace StackVm.Core;

/// <summary>
/// Provides an array-backed integer stack with indexed frame access.
/// </summary>
public sealed class IntStack
{
    private int[] _items;
    private readonly bool _track;

    /// <summary>
    /// Gets the number of used slots.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the current allocated capacity in integer slots.
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// Gets the highest occupied slot count when tracking is enabled.
    /// </summary>
    public int PeakCount { get; private set; }

    /// <summary>
    /// Gets the highest allocated capacity when tracking is enabled.
    /// </summary>
    public int PeakCapacity { get; private set; }

    /// <summary>
    /// Gets the number of buffer expansions when tracking is enabled.
    /// </summary>
    public long ResizeCount { get; private set; }

    /// <summary>
    /// Creates a stack with an initial capacity.
    /// </summary>
    /// <param name="capacity">Initial capacity, at least one.</param>
    /// <param name="trackMetrics">Enables storage metrics.</param>
    public IntStack(int capacity = 32, bool trackMetrics = false)
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        _items = new int[capacity];
        _track = trackMetrics;
        if (_track) PeakCapacity = capacity;
    }

    /// <summary>
    /// Appends a value.
    /// </summary>
    public void Push(int value)
    {
        if (Count == _items.Length)
        {
            Array.Resize(ref _items, checked(_items.Length * 2));
            if (_track)
            {
                ResizeCount++;
                PeakCapacity = _items.Length;
            }
        }
        _items[Count++] = value;
        if (_track && Count > PeakCount) PeakCount = Count;
    }

    /// <summary>
    /// Removes and returns the top value.
    /// </summary>
    public int Pop()
    {
        if (Count == 0) throw new InvalidOperationException("Stack underflow.");
        return _items[--Count];
    }

    /// <summary>
    /// Returns the top value without removing it.
    /// </summary>
    public int Peek()
    {
        if (Count == 0) throw new InvalidOperationException("Stack underflow.");
        return _items[Count - 1];
    }

    /// <summary>
    /// Gets or sets a slot within the active stack.
    /// </summary>
    public int this[int index]
    {
        get => index >= 0 && index < Count ? _items[index] : throw new ArgumentOutOfRangeException(nameof(index));
        set
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            _items[index] = value;
        }
    }

    /// <summary>
    /// Discards slots above the specified size.
    /// </summary>
    public void Truncate(int count)
    {
        if (count < 0 || count > Count) throw new ArgumentOutOfRangeException(nameof(count));
        Count = count;
    }
}