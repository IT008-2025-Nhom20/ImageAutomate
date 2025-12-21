using System.Collections.Concurrent;
using ImageAutomate.Core;
using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Execution;

/// <summary>
/// The runtime state of a pipeline execution.
/// </summary>
internal sealed class ExecutionContext
{
    /// <summary>
    /// The pipeline graph being executed.
    /// </summary>
    public PipelineGraph Graph { get; }

    /// <summary>
    /// The executor configuration.
    /// </summary>
    public ExecutorConfiguration Configuration { get; }

    /// <summary>
    /// The scheduler instance for this execution.
    /// </summary>
    public IScheduler Scheduler { get; }

    /// <summary>
    /// Cancellation token for cooperative cancellation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Warehouses indexed by producer block.
    /// </summary>
    /// <remarks>
    /// Warehouses are lazily initialized. Initialization defers to the first access.
    /// </remarks>
    public ConcurrentDictionary<IBlock, Lazy<Warehouse>> Warehouses { get; } = new();

    /// <summary>
    /// Dependency barriers indexed by consumer block.
    /// </summary>
    /// <remarks>
    /// Barriers are lazily initialized. Initialization defers to the first access.
    /// </remarks>
    public ConcurrentDictionary<IBlock, Lazy<DependencyBarrier>> Barriers { get; } = new();

    /// <summary>
    /// In-degree (number of incoming connections) for each block.
    /// </summary>
    public Dictionary<IBlock, int> InDegree { get; } = [];

    /// <summary>
    /// Out-degree (number of outgoing connections) for each block.
    /// </summary>
    public Dictionary<IBlock, int> OutDegree { get; } = [];

    /// <summary>
    /// Precomputed downstream blocks for each block (adjacency list).
    /// </summary>
    public Dictionary<IBlock, HashSet<IBlock>> DownstreamBlocks { get; } = [];

    /// <summary>
    /// Precomputed upstream blocks for each block (reverse adjacency list).
    /// </summary>
    public Dictionary<IBlock, HashSet<IBlock>> UpstreamBlocks { get; } = [];

    /// <summary>
    /// Precomputed connections grouped by target block and target socket.
    /// Maps: TargetBlock -> TargetSocket -> List of source blocks.
    /// </summary>
    public Dictionary<IBlock, Dictionary<Socket, List<IBlock>>> SocketSources { get; } = [];

    /// <summary>
    /// Current execution state of each block.
    /// </summary>
    public ConcurrentDictionary<IBlock, BlockExecutionState> BlockStates { get; } = new();

    /// <summary>
    /// Exceptions encountered during execution.
    /// </summary>
    public ConcurrentBag<Exception> Exceptions { get; } = [];

    /// <summary>
    /// Set of source blocks that can still emit more shipments.
    /// </summary>
    public HashSet<IBlock> ActiveSources { get; } = [];

    /// <summary>
    /// Lock for ActiveSources access.
    /// </summary>
    public Lock ActiveSourcesLock { get; } = new();

    /// <summary>
    /// Cached lookup table tracking which blocks have active upstream sources.
    /// Updated incrementally when ActiveSources changes.
    /// </summary>
    private readonly ConcurrentDictionary<IBlock, bool> _hasActiveUpstreamCache = new();

    /// <summary>
    /// Number of blocks currently executing.
    /// </summary>
    public int ActiveBlockCount => _activeBlockCount;

    /// <summary>
    /// Number of shipments processed (for progress tracking).
    /// A shipment is one execution of a block (may be multiple for IShipmentSource blocks).
    /// </summary>
    public int ProcessedShipmentCount => _processedShipmentCount;

    /// <summary>
    /// Timestamp of last progress.
    /// </summary>
    public DateTime LastProgress
    {
        get => new(Interlocked.Read(ref _lastProgressTicks));
        set => Interlocked.Exchange(ref _lastProgressTicks, value.Ticks);
    }

    private int _activeBlockCount;
    private int _processedShipmentCount;
    private long _lastProgressTicks;

    public ExecutionContext(
        PipelineGraph graph,
        ExecutorConfiguration configuration,
        IScheduler scheduler,
        CancellationToken cancellationToken)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        CancellationToken = cancellationToken;

        _activeBlockCount = 0;
        _processedShipmentCount = 0;
        _lastProgressTicks = DateTime.UtcNow.Ticks;

        InitializeDegrees();
    }

    /// <summary>
    /// Initializes in-degree, out-degree, and adjacency maps from the graph.
    /// </summary>
    private void InitializeDegrees()
    {
        // Initialize all blocks with degree 0 and Pending state
        foreach (var block in Graph.Blocks)
        {
            InDegree[block] = 0;
            OutDegree[block] = 0;
            BlockStates[block] = BlockExecutionState.Pending;
            DownstreamBlocks[block] = [];
            UpstreamBlocks[block] = [];
            SocketSources[block] = [];
        }

        // Build adjacency lists and count degrees from connections
        foreach (var connection in Graph.Connections)
        {
            InDegree[connection.Target]++;
            OutDegree[connection.Source]++;

            // Add to downstream adjacency
            DownstreamBlocks[connection.Source].Add(connection.Target);

            // Add to upstream adjacency
            UpstreamBlocks[connection.Target].Add(connection.Source);

            // Add to socket sources map
            if (!SocketSources[connection.Target].TryGetValue(connection.TargetSocket, out var sources))
            {
                sources = [];
                SocketSources[connection.Target][connection.TargetSocket] = sources;
            }
            sources.Add(connection.Source);
        }
    }

    /// <summary>
    /// Increments the active block count.
    /// </summary>
    public void IncrementActiveBlocks()
    {
        Interlocked.Increment(ref _activeBlockCount);
    }

    /// <summary>
    /// Decrements the active block count.
    /// </summary>
    public void DecrementActiveBlocks()
    {
        Interlocked.Decrement(ref _activeBlockCount);
    }

    /// <summary>
    /// Increments the processed shipment count and updates last progress timestamp.
    /// </summary>
    public void IncrementProcessedShipments()
    {
        Interlocked.Increment(ref _processedShipmentCount);
        LastProgress = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks a block as blocked and invalidates its cache entry.
    /// </summary>
    public void MarkBlocked(IBlock block)
    {
        BlockStates[block] = BlockExecutionState.Blocked;
        InvalidateCacheForBlockedBlock(block);
    }

    /// <summary>
    /// Marks a block as failed and invalidates its cache entry.
    /// (won't reset, can block downstream) but retain their Failed state.
    /// </summary>
    /// <remarks>
    /// Failed blocks are "poisonous" - they behave like blocked blocks
    /// </remarks>
    public void MarkFailed(IBlock block)
    {
        BlockStates[block] = BlockExecutionState.Failed;
        InvalidateCacheForBlockedBlock(block);
    }

    /// <summary>
    /// Checks if a block has failed.
    /// </summary>
    public bool IsFailed(IBlock block)
    {
        return BlockStates.TryGetValue(block, out var state)
            && state == BlockExecutionState.Failed;
    }

    /// <summary>
    /// Checks if a block is blocked or failed (poisonous)
    /// </summary>
    /// <remarks>
    /// Poisonous blocks are meant to not execute, not reset, not provide data to downstream blocks.
    /// </remarks>
    public bool IsBlocked(IBlock block)
    {
        return BlockStates.TryGetValue(block, out var state)
            && (state == BlockExecutionState.Blocked || state == BlockExecutionState.Failed);
    }

    public bool IsRunning(IBlock block)
    {
        return BlockStates.TryGetValue(block, out var state)
            && state == BlockExecutionState.Running;
    }

    /// <summary>
    /// Calculates the active in-degree for a block (number of active incoming connections).
    /// </summary>
    /// <remarks>
    /// This counts connections, not blocks, to properly handle multiple connections to the same socket.
    /// Only counts connections from blocks that have active upstream sources.
    /// </remarks>
    public int GetActiveInDegree(IBlock block)
    {
        var incomingConnections = Graph.Connections
            .Where(c => c.Target == block)
            .ToList();

        int activeCount = 0;
        foreach (var connection in incomingConnections)
        {
            var sourceBlock = connection.Source;

            // Check if this upstream block has any active source in its transitive dependencies
            if (HasActiveUpstreamSource(sourceBlock))
            {
                activeCount++;
            }
        }

        return activeCount;
    }

    /// <summary>
    /// Checks if a block has any active source in its transitive upstream dependencies.
    /// Uses cached lookup table for O(1) access.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: Cache is only updated at cycle boundaries, not mid-cycle.
    /// This ensures stable values during a shipment cycle even as sources exhaust.
    /// </remarks>
    public bool HasActiveUpstreamSource(IBlock block)
    {
        // Blocked blocks are considered to have no active sources
        if (IsBlocked(block))
            return false;

        if (_hasActiveUpstreamCache.TryGetValue(block, out var cached))
            return cached;

        // Fallback for blocks not in cache (should only happen during initialization)
        var result = ComputeHasActiveUpstreamSource(block);
        _hasActiveUpstreamCache[block] = result;
        return result;
    }

    /// <summary>
    /// Computes whether a block has active upstream sources using BFS.
    /// </summary>
    private bool ComputeHasActiveUpstreamSource(IBlock block)
    {
        lock (ActiveSourcesLock)
        {
            // Direct check: is this block itself an active source?
            if (ActiveSources.Contains(block))
                return true;
        }

        // BFS to find active sources upstream
        var visited = new HashSet<IBlock>();
        var queue = new Queue<IBlock>();
        queue.Enqueue(block);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            var upstreamBlocks = Graph.Connections
                .Where(c => c.Target == current)
                .Select(c => c.Source);

            foreach (var upstream in upstreamBlocks)
            {
                lock (ActiveSourcesLock)
                {
                    if (ActiveSources.Contains(upstream))
                        return true;
                }
                queue.Enqueue(upstream);
            }
        }

        return false;
    }

    /// <summary>
    /// Marks a source block as active and updates the downstream cache.
    /// Call this when a source begins emitting shipments.
    /// </summary>
    public void MarkSourceActive(IBlock sourceBlock)
    {
        lock (ActiveSourcesLock)
        {
            if (!ActiveSources.Add(sourceBlock))
                return; // Already active, no update needed
        }

        // Propagate active status downstream
        PropagateActiveStatusDownstream(sourceBlock, isActive: true);
    }

    /// <summary>
    /// Marks a source block as inactive.
    /// Call this when a source is exhausted.
    /// </summary>
    /// <remarks>
    /// Cache update is deferred until next shipment cycle boundary.
    /// </remarks>
    public void MarkSourceInactive(IBlock sourceBlock)
    {
        lock (ActiveSourcesLock)
        {
            ActiveSources.Remove(sourceBlock);
        }
    }

    /// <summary>
    /// Propagates active status downstream from a newly activated source.
    /// </summary>
    private void PropagateActiveStatusDownstream(IBlock sourceBlock, bool isActive)
    {
        // Mark the source itself
        _hasActiveUpstreamCache[sourceBlock] = true;

        // BFS to propagate to all downstream blocks
        var visited = new HashSet<IBlock>();
        var queue = new Queue<IBlock>();
        queue.Enqueue(sourceBlock);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            var downstreamBlocks = Graph.Connections
                .Where(c => c.Source == current)
                .Select(c => c.Target);

            foreach (var downstream in downstreamBlocks)
            {
                _hasActiveUpstreamCache[downstream] = true;
                queue.Enqueue(downstream);
            }
        }
    }


    /// <summary>
    /// Removes a block from the upstream cache when it's blocked.
    /// Blocked blocks will never execute, so they don't need cache entries.
    /// </summary>
    public void InvalidateCacheForBlockedBlock(IBlock block)
    {
        _hasActiveUpstreamCache.TryRemove(block, out _);
    }

    /// <summary>
    /// Initializes the upstream source cache for all blocks.
    /// Should be called after ActiveSources is initially populated.
    /// </summary>
    public void InitializeUpstreamCache()
    {
        _hasActiveUpstreamCache.Clear();

        foreach (var block in Graph.Blocks)
        {
            // Skip blocked blocks - they won't execute
            if (!IsBlocked(block))
            {
                var hasActive = ComputeHasActiveUpstreamSource(block);
                _hasActiveUpstreamCache[block] = hasActive;
            }
        }
    }

    /// <summary>
    /// Resets execution state for the next shipment cycle.
    /// </summary>
    public void ResetForNextShipment()
    {
        // Clear warehouses and barriers
        Warehouses.Clear();
        Barriers.Clear();

        // Reset all block states to Pending (except blocked and active sources)
        foreach (var block in Graph.Blocks)
        {
            if (!IsBlocked(block))
            {
                BlockStates[block] = BlockExecutionState.Pending;
            }
        }

        // Re-mark active sources as Ready
        lock (ActiveSourcesLock)
        {
            foreach (var source in ActiveSources)
            {
                if (!IsBlocked(source))
                {
                    BlockStates[source] = BlockExecutionState.Ready;
                }
            }
        }

        // Rebuild the upstream source cache for the new cycle
        InitializeUpstreamCache();
    }
}
