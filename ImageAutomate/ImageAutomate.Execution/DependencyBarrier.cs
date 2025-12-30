using ImageAutomate.Core;

namespace ImageAutomate.Execution;

/// <summary>
/// Consumer-centric control gate that tracks dependency satisfaction.
/// </summary>
/// <remarks>
/// This class is thread-safe. It uses atomic operations for lock-free coordination.
/// Ensures exactly-once enqueueing via CAS (Compare-And-Swap).
/// Initialized lazily when first predecessor completes.
/// </remarks>
public sealed class DependencyBarrier
{
    private int _dependencyCount; // number of unsatisfied dependencies
    private int _enqueued; // 0 = false, 1 = true
    private readonly IBlock _block; // the consumer block that this barrier protects

    /// <summary>
    /// Initializes a new dependency barrier for the specified block.
    /// </summary>
    /// <param name="block">The consumer block this barrier protects.</param>
    /// <param name="inDegree">Number of incoming dependencies (in-degree).</param>
    public DependencyBarrier(IBlock block, int inDegree)
    {
        if (inDegree < 0)
            throw new ArgumentOutOfRangeException(nameof(inDegree), "In-degree cannot be negative.");

        _block = block ?? throw new ArgumentNullException(nameof(block));
        _dependencyCount = inDegree;
        _enqueued = 0;
    }

    /// <summary>
    /// Signals that one dependency has been satisfied.
    /// </summary>
    /// <returns>True if this was the last dependency and the block should be enqueued; otherwise false.</returns>
    public bool Signal()
    {
        // Atomically decrement the dependency count
        int remaining = Interlocked.Decrement(ref _dependencyCount);

        if (remaining < 0)
            throw new InvalidOperationException(
                $"Dependency count underflow for block '{_block.Name}'. More signals than expected.");

        if (remaining == 0)
        {
            // All dependencies satisfied. Try to set enqueue flag.
            int previousFlag = Interlocked.CompareExchange(ref _enqueued, 1, 0);

            // Return true only if this was the first barrier to set the flag
            return previousFlag == 0;
        }

        return false;
    }

    /// <summary>
    /// Gets the block this barrier protects.
    /// </summary>
    public IBlock Block => _block;

    /// <summary>
    /// Gets the current dependency count.
    /// </summary>
    public int RemainingDependencies => Math.Max(0, _dependencyCount);

    /// <summary>
    /// Gets whether this barrier has triggered enqueue.
    /// </summary>
    public bool HasEnqueued => _enqueued == 1;
}
