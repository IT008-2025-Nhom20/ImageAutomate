using ImageAutomate.Core;

namespace ImageAutomate.Execution.Scheduling;

/// <summary>
/// Greedy completion pressure scheduler - executes the "hungriest" block first.
/// Simple implementation - production-ready, minimal overhead.
/// </summary>
/// <remarks>
/// Strategy: "Always pick the block that will free the most memory right now"
/// 
/// Priority = -CompletionPressure
/// where CompletionPressure = Σ(WarehouseSize / RemainingConsumers) for all predecessors
/// 
/// No depth calculation needed - greedy strategy naturally follows deep paths.
/// Simple, fast (O(In-Degree) per enqueue), and optimal for memory pressure.
/// </remarks>
internal sealed class SimpleDfsScheduler : IScheduler
{
    // Scheduler queue; Critical section
    private readonly PriorityQueue<IBlock, float> _queue = new();
    private readonly Lock _lock = new();

    public bool IsEmpty
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count == 0;
            }
        }
    }

    public void Enqueue(IBlock block, ExecutionContext context)
    {
        lock (_lock)
        {
            // Calculate priority at enqueue time for accurate scheduling
            float priority = CalculatePriority(block, context);
            _queue.Enqueue(block, priority);
        }
    }

    public IBlock? TryDequeue(ExecutionContext context)
    {
        lock (_lock)
        {
            if (_queue.Count == 0)
                return null;

            // Simply dequeue - priority was calculated correctly at enqueue
            return _queue.Dequeue();
        }
    }

    /// <summary>
    /// Calculates greedy priority based purely on completion pressure.
    /// </summary>
    /// <remarks>
    /// Priority = -CompletionPressure (negative for min-heap)
    /// </remarks>
    private float CalculatePriority(IBlock block, ExecutionContext context)
        => -CalculateCompletionPressure(block, context);

    /// <summary>
    /// Calculates Completion Pressure priority for a block.
    /// </summary>
    /// <remarks>
    /// Priority = Σ(WarehouseSize / RemainingConsumers) for all predecessors.
    /// Higher values indicate more memory pressure (should execute sooner).
    /// </remarks>
    private float CalculateCompletionPressure(IBlock block, ExecutionContext context)
    {
        float totalPressure = 0;

        // Find all predecessor blocks (sources of incoming connections)
        var predecessors = context.Graph.Connections
            .Where(c => c.Target == block)
            .Select(c => c.Source)
            .Distinct();

        foreach (var predecessor in predecessors)
        {
            if (context.Warehouses.TryGetValue(predecessor, out var lazyWarehouse))
            {
                var warehouse = lazyWarehouse.Value;
                float warehouseSize = warehouse.TotalSizeMp;
                int remainingConsumers = warehouse.RemainingConsumers;

                if (remainingConsumers > 0)
                {
                    totalPressure += warehouseSize / remainingConsumers;
                }
            }
        }

        return totalPressure;
    }
}
