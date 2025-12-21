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
    private readonly PriorityQueue<IBlock, float> _queue = new();
    private readonly HashSet<IBlock> _enqueuedBlocks = [];
    private readonly Lock _lock = new();

    /// <inheritdoc />
    public bool HasPendingWork
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count > 0;
            }
        }
    }

    /// <inheritdoc />
    public void Initialize(ExecutionContext context)
    {
        // Find and enqueue all source blocks (IShipmentSource with in-degree 0)
        foreach (var block in context.Graph.Blocks)
        {
            if (block is IShipmentSource && context.InDegree[block] == 0)
            {
                context.SetBlockState(block, BlockExecutionState.Ready);
                Enqueue(block, context);
            }
        }
    }

    /// <inheritdoc />
    public IBlock? TryDequeue(ExecutionContext context)
    {
        lock (_lock)
        {
            while (_queue.Count > 0)
            {
                var block = _queue.Dequeue();
                _enqueuedBlocks.Remove(block);

                // Skip blocked blocks that may have been blocked after enqueueing
                if (context.IsBlocked(block))
                    continue;

                return block;
            }

            return null;
        }
    }

    /// <inheritdoc />
    public void NotifyCompleted(IBlock completedBlock, ExecutionContext context)
    {
        SignalDownstreamBarriers(completedBlock, context);
    }

    /// <inheritdoc />
    public void NotifyBlocked(IBlock blockedBlock, ExecutionContext context)
    {
        // Decrement warehouse counters for upstream blocks (cleanup)
        if (context.UpstreamBlocks.TryGetValue(blockedBlock, out var upstreamBlocks))
        {
            foreach (var upstreamBlock in upstreamBlocks)
            {
                if (context.TryGetWarehouse(upstreamBlock, out var warehouse) && warehouse != null)
                {
                    warehouse.DecrementConsumerCount();
                }
            }
        }

        // Signal downstream (so they can also be skipped when ready)
        SignalDownstreamBarriers(blockedBlock, context);

        // Count as completed (skipped)
        context.IncrementProcessedShipments();
    }

    /// <inheritdoc />
    public void BeginNextShipmentCycle(ExecutionContext context)
    {
        context.ForEachActiveSource(source =>
        {
            if (!context.IsBlocked(source))
            {
                Enqueue(source, context);
            }
        });
    }

    /// <summary>
    /// Enqueues a block for execution (internal operation).
    /// </summary>
    private void Enqueue(IBlock block, ExecutionContext context)
    {
        // Don't enqueue blocked blocks
        if (context.IsBlocked(block))
            return;

        if (context.IsRunning(block))
            return;

        lock (_lock)
        {
            // Prevent duplicate enqueueing
            if (!_enqueuedBlocks.Add(block))
                return;

            float priority = CalculatePriority(block, context);
            _queue.Enqueue(block, priority);
        }
    }

    /// <summary>
    /// Signals downstream dependency barriers and enqueues ready blocks.
    /// </summary>
    private void SignalDownstreamBarriers(IBlock completedBlock, ExecutionContext context)
    {
        if (!context.DownstreamBlocks.TryGetValue(completedBlock, out var downstreamBlocks))
            return;

        foreach (var downstreamBlock in downstreamBlocks)
        {
            int inDegree = context.GetActiveInDegree(downstreamBlock);
            // Get or create barrier
            var barrier = context.GetOrCreateBarrier(downstreamBlock, inDegree);

            // Signal and check if ready
            if (barrier.Signal())
            {
                context.SetBlockState(downstreamBlock, BlockExecutionState.Ready);
                Enqueue(downstreamBlock, context);
            }
        }
    }

    /// <summary>
    /// Calculates greedy priority based purely on completion pressure.
    /// </summary>
    private float CalculatePriority(IBlock block, ExecutionContext context)
        => -CalculateCompletionPressure(block, context);

    /// <summary>
    /// Calculates Completion Pressure priority for a block.
    /// </summary>
    private float CalculateCompletionPressure(IBlock block, ExecutionContext context)
    {
        float totalPressure = 0;

        if (!context.UpstreamBlocks.TryGetValue(block, out var predecessors))
            return totalPressure;

        foreach (var predecessor in predecessors)
        {
            if (context.TryGetWarehouse(predecessor, out var warehouse) && warehouse != null)
            {
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
