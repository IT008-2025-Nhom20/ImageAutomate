using System.Collections.Immutable;
using ImageAutomate.Core;

namespace ImageAutomate.Execution;

/// <summary>
/// Producer-centric buffer that stores block execution results and implements
/// Just-In-Time (JIT) cloning for memory efficient and safe branching.
/// </summary>
/// <remarks>
/// This class is thread-safe. It uses atomic counter operations for lock-free coordination.
/// Initialized lazily when a block first produces output.
/// </remarks>
internal sealed class Warehouse
{
    private readonly Lock _lock = new(); // mutex lock to protect buffer access
    private ImmutableDictionary<Socket, ImmutableList<IBasicWorkItem>>? _inventory; // immutable data buffer for thread-safe reads
    private int _consumerCount; // number of remaining consumers (dependencies) of this warehouse
    private bool _isImported; // whether outputs has been imported into the warehouse

    /// <summary>
    /// Initializes a new warehouse with the specified consumer count (out-degree).
    /// </summary>
    /// <param name="consumerCount">Number of consumers that will read from (depends on) this warehouse.</param>
    public Warehouse(int consumerCount)
    {
        if (consumerCount < 0)
            throw new ArgumentOutOfRangeException(nameof(consumerCount), "Consumer count cannot be negative.");
        
        _consumerCount = consumerCount;
        _isImported = false;
    }

    /// <summary>
    /// Commits the block's output to the warehouse.
    /// </summary>
    /// <param name="outputs">Block execution outputs mapped by socket.</param>
    public void Import(IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> outputs)
    {
        lock (_lock)
        {
            if (_isImported)
                throw new InvalidOperationException("Warehouse is already sealed. Cannot commit twice.");

            var builder = ImmutableDictionary.CreateBuilder<Socket, ImmutableList<IBasicWorkItem>>();
            foreach (var kvp in outputs)
            {
                builder[kvp.Key] = [.. kvp.Value];
            }

            _inventory = builder.ToImmutable();
            _isImported = true;
        }
    }

    /// <summary>
    /// Retrieves inputs for a consumer.
    /// </summary>
    /// <returns>
    /// Dictionary of socket to work items. Items are cloned if other consumers remain,
    /// or transferred if caller is the last consumer.
    /// </returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> GetInventory()
    {
        if (_inventory == null)
            throw new InvalidOperationException("Warehouse has not been committed yet.");

        // Atomically decrement consumer count
        int remainingConsumers = Interlocked.Decrement(ref _consumerCount);

        if (remainingConsumers < 0)
            throw new InvalidOperationException("Consumer count underflow. More consumers than expected.");

        if (remainingConsumers == 0)
        {
            // Last consumer: Transfer ownership
            var result = _inventory.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<IBasicWorkItem>)kvp.Value
            );

            // Clear internal storage (allow GC)
            lock (_lock)
            {
                _inventory = null;
            }

            return result;
        }
        else
        {
            var result = _inventory.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<IBasicWorkItem>)kvp.Value
                    .Select(item => (IBasicWorkItem)item.Clone())
                    .ToList()
            );

            return result;
        }
    }

    /// <summary>
    /// Gets the current consumer count.
    /// </summary>
    public int RemainingConsumers => Math.Max(0, _consumerCount);

    /// <summary>
    /// Gets the total size of stored data in megapixels.
    /// </summary>
    public float TotalSizeMp
    {
        get
        {
            var data = _inventory;
            if (data == null) return 0;

            return data.Values
                .SelectMany(list => list)
                .OfType<IWorkItem>()
                .Sum(item => item.SizeMP);
        }
    }

    /// <summary>
    /// Decrements the consumer count without retrieving data (used for poisoned blocks cleanup).
    /// </summary>
    public void DecrementConsumerCount()
    {
        Interlocked.Decrement(ref _consumerCount);
    }
}
