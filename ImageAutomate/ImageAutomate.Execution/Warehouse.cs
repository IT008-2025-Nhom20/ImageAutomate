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
    /// Import the block's output into the warehouse.
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
    /// Retrieves inputs for a consumer filtered by the specified sockets.
    /// </summary>
    /// <param name="sockets">A set of sockets to filter</param>
    /// <returns>
    /// Dictionary of socket to WorkItems. Items are cloned if other consumers remain,
    /// or transferred if caller is the last consumer
    /// </returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> GetInventory(IEnumerable<Socket> sockets)
        => GetInventoryCore(sockets);

    /// <summary>
    /// Retrieves inputs for a consumer.
    /// </summary>
    /// <returns>
    /// Dictionary of socket to WorkItems. Items are cloned if other consumers remain,
    /// or transferred if caller is the last consumer
    /// </returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> GetInventory()
        => GetInventoryCore(null);

    /// <summary>
    /// Retrieves inputs for a consumer, optionally filtered by sockets.
    /// </summary>
    /// <param name="sockets">Optional set of sockets to filter. If null, all sockets are returned.</param>
    /// <returns>
    /// Dictionary of socket to work items. Items are cloned if other consumers remain,
    /// or transferred if caller is the last consumer.
    /// </returns>
    private Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> GetInventoryCore(IEnumerable<Socket>? sockets = null)
    {
        var inventory = _inventory;
        if (inventory == null)
            throw new InvalidOperationException("Warehouse has not been committed yet.");

        // Atomically decrement consumer count
        int remainingConsumers = Interlocked.Decrement(ref _consumerCount);

        if (remainingConsumers < 0)
            throw new InvalidOperationException("Consumer count underflow. More consumers than expected.");

        var filtered = sockets != null
            ? inventory.Where(kvp => sockets.Contains(kvp.Key))
            : inventory;

        if (remainingConsumers == 0)
        {
            // Last consumer: Transfer ownership
            var result = filtered.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<IBasicWorkItem>)kvp.Value
            );

            // Clear internal storage (allow GC)
            //!!!!! VERY IMPORTANT: NEVER FORGET TO LOCK WHEN TOUCHING _inventory !!!!!
            lock (_lock)
            {
                _inventory = null;
            }

            return result;
        }
        else
        {
            var result = filtered.ToDictionary(
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
    /// Decrements the consumer count without retrieving data (used for blocked blocks cleanup).
    /// </summary>
    public void DecrementConsumerCount()
    {
        Interlocked.Decrement(ref _consumerCount);
    }
}
