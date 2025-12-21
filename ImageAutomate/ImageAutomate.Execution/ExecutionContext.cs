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

    #region Graph Topology (Immutable after initialization)

    /// <summary>
    /// In-degree (number of incoming connections) for each block.
    /// </summary>
    internal IReadOnlyDictionary<IBlock, int> InDegree => _inDegree;
    private readonly Dictionary<IBlock, int> _inDegree = [];

    /// <summary>
    /// Out-degree (number of outgoing connections) for each block.
    /// </summary>
    internal IReadOnlyDictionary<IBlock, int> OutDegree => _outDegree;
    private readonly Dictionary<IBlock, int> _outDegree = [];

    /// <summary>
    /// Precomputed downstream blocks for each block (adjacency list).
    /// </summary>
    internal IReadOnlyDictionary<IBlock, HashSet<IBlock>> DownstreamBlocks => _downstreamBlocks;
    private readonly Dictionary<IBlock, HashSet<IBlock>> _downstreamBlocks = [];

    /// <summary>
    /// Precomputed upstream blocks for each block (reverse adjacency list).
    /// </summary>
    internal IReadOnlyDictionary<IBlock, HashSet<IBlock>> UpstreamBlocks => _upstreamBlocks;
    private readonly Dictionary<IBlock, HashSet<IBlock>> _upstreamBlocks = [];

    /// <summary>
    /// Precomputed connections grouped by target block and target socket.
    /// Maps: TargetBlock -> TargetSocket -> List of source blocks.
    /// </summary>
    internal IReadOnlyDictionary<IBlock, Dictionary<Socket, List<IBlock>>> SocketSources => _socketSources;
    private readonly Dictionary<IBlock, Dictionary<Socket, List<IBlock>>> _socketSources = [];

    #endregion

    #region Runtime State (Mutable, accessed via methods)

    /// <summary>
    /// Warehouses indexed by producer block.
    /// </summary>
    private readonly ConcurrentDictionary<IBlock, Lazy<Warehouse>> _warehouses = new();

    /// <summary>
    /// Dependency barriers indexed by consumer block.
    /// </summary>
    private readonly ConcurrentDictionary<IBlock, Lazy<DependencyBarrier>> _barriers = new();

    /// <summary>
    /// Active incoming connections for each block.
    /// Only contains connections from blocks with active upstream sources.
    /// </summary>
    private readonly ConcurrentDictionary<IBlock, IReadOnlyList<Connection>> _activeIncomingConnections = new();

    /// <summary>
    /// Cached active in-degree for each block.
    /// </summary>
    private readonly ConcurrentDictionary<IBlock, int> _activeInDegree = new();

    /// <summary>
    /// Current execution state of each block.
    /// </summary>
    private readonly ConcurrentDictionary<IBlock, BlockExecutionState> _blockStates = new();

    /// <summary>
    /// Exceptions encountered during execution.
    /// </summary>
    private readonly ConcurrentBag<Exception> _exceptions = [];

    /// <summary>
    /// Set of source blocks that can still emit more shipments.
    /// </summary>
    private readonly HashSet<IBlock> _activeSources = [];

    /// <summary>
    /// Lock for ActiveSources access.
    /// </summary>
    private readonly Lock _activeSourcesLock = new();

    #endregion

    #region Counters and Progress

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
        private set => Interlocked.Exchange(ref _lastProgressTicks, value.Ticks);
    }

    /// <summary>
    /// Returns true if there are any active sources remaining.
    /// </summary>
    public bool HasActiveSources
    {
        get
        {
            lock (_activeSourcesLock)
            {
                return _activeSources.Count > 0;
            }
        }
    }

    /// <summary>
    /// Returns true if any exceptions were recorded.
    /// </summary>
    public bool HasExceptions => !_exceptions.IsEmpty;

    private int _activeBlockCount;
    private int _processedShipmentCount;
    private long _lastProgressTicks;

    #endregion

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
            _inDegree[block] = 0;
            _outDegree[block] = 0;
            _blockStates[block] = BlockExecutionState.Pending;
            _downstreamBlocks[block] = [];
            _upstreamBlocks[block] = [];
            _socketSources[block] = [];
            _activeIncomingConnections[block] = Array.Empty<Connection>();
            _activeInDegree[block] = 0;
        }

        // Build adjacency lists and count degrees from connections
        foreach (var connection in Graph.Connections)
        {
            _inDegree[connection.Target]++;
            _outDegree[connection.Source]++;

            // Add to downstream adjacency
            _downstreamBlocks[connection.Source].Add(connection.Target);

            // Add to upstream adjacency
            _upstreamBlocks[connection.Target].Add(connection.Source);

            // Note: ActiveIncomingConnections will be populated during InitializeActiveConnections
            // after ActiveSources is set

            // Add to socket sources map
            if (!_socketSources[connection.Target].TryGetValue(connection.TargetSocket, out var sources))
            {
                sources = [];
                _socketSources[connection.Target][connection.TargetSocket] = sources;
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

    #region Warehouse Access

    /// <summary>
    /// Gets or creates a warehouse for the specified block.
    /// </summary>
    public Warehouse GetOrCreateWarehouse(IBlock block)
    {
        var lazy = _warehouses.GetOrAdd(
            block,
            b => new Lazy<Warehouse>(() => new Warehouse(_outDegree[b])));
        return lazy.Value;
    }

    /// <summary>
    /// Tries to get an existing warehouse for the specified block.
    /// </summary>
    public bool TryGetWarehouse(IBlock block, out Warehouse? warehouse)
    {
        if (_warehouses.TryGetValue(block, out var lazy))
        {
            warehouse = lazy.Value;
            return true;
        }
        warehouse = null;
        return false;
    }

    #endregion

    #region Barrier Access

    /// <summary>
    /// Gets or creates a dependency barrier for the specified block with the given dependency count.
    /// </summary>
    public DependencyBarrier GetOrCreateBarrier(IBlock block, int dependencyCount)
    {
        var lazy = _barriers.GetOrAdd(
            block,
            _ => new Lazy<DependencyBarrier>(() => new DependencyBarrier(block, dependencyCount)));
        return lazy.Value;
    }

    #endregion

    #region Block State Access

    /// <summary>
    /// Gets the current state of a block.
    /// </summary>
    public BlockExecutionState GetBlockState(IBlock block)
    {
        return _blockStates.TryGetValue(block, out var state)
            ? state
            : BlockExecutionState.Pending;
    }

    /// <summary>
    /// Sets the state of a block.
    /// </summary>
    public void SetBlockState(IBlock block, BlockExecutionState state)
    {
        _blockStates[block] = state;
    }

    #endregion

    #region Exception Handling

    /// <summary>
    /// Records an exception that occurred during execution.
    /// </summary>
    public void RecordException(Exception exception)
    {
        _exceptions.Add(exception);
    }

    /// <summary>
    /// Gets all recorded exceptions.
    /// </summary>
    public IEnumerable<Exception> GetExceptions()
    {
        return _exceptions;
    }

    #endregion

    #region Block State Checks

    /// <summary>
    /// Marks a block as blocked.
    /// </summary>
    public void MarkBlocked(IBlock block)
    {
        _blockStates[block] = BlockExecutionState.Blocked;
    }

    /// <summary>
    /// Marks a block as failed.
    /// (won't reset, can block downstream) but retain their Failed state.
    /// </summary>
    /// <remarks>
    /// Failed blocks are "poisonous" - they behave like blocked blocks
    /// </remarks>
    public void MarkFailed(IBlock block)
    {
        _blockStates[block] = BlockExecutionState.Failed;
    }

    /// <summary>
    /// Checks if a block has failed.
    /// </summary>
    public bool IsFailed(IBlock block)
    {
        return _blockStates.TryGetValue(block, out var state)
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
        return _blockStates.TryGetValue(block, out var state)
            && (state == BlockExecutionState.Blocked || state == BlockExecutionState.Failed);
    }

    public bool IsRunning(IBlock block)
    {
        return _blockStates.TryGetValue(block, out var state)
            && state == BlockExecutionState.Running;
    }

    #endregion

    #region Active Connection Access

    /// <summary>
    /// Gets the active in-degree for a block (number of active incoming connections).
    /// </summary>
    /// <remarks>
    /// This is an O(1) thread-safe lookup using pre-computed ActiveInDegree.
    /// Only counts connections from blocks that have active upstream sources.
    /// </remarks>
    public int GetActiveInDegree(IBlock block)
    {
        return _activeInDegree.TryGetValue(block, out var degree) ? degree : 0;
    }

    /// <summary>
    /// Gets the active incoming connections for a block.
    /// </summary>
    /// <remarks>
    /// Returns only connections from blocks with active upstream sources.
    /// Thread-safe; list is immutable after retrieval.
    /// </remarks>
    public IReadOnlyList<Connection> GetActiveIncomingConnections(IBlock block)
    {
        return _activeIncomingConnections.TryGetValue(block, out var connections)
            ? connections
            : Array.Empty<Connection>();
    }

    /// <summary>
    /// Checks if a block has any active source in its transitive upstream dependencies.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: This is updated at cycle boundaries, not mid-cycle.
    /// This ensures stable values during a shipment cycle even as sources exhaust.
    /// A block has active upstream sources if it has any active incoming connections
    /// or if it's an active source itself.
    /// </remarks>
    public bool HasActiveUpstreamSource(IBlock block)
    {
        // Blocked blocks are considered to have no active sources
        if (IsBlocked(block))
            return false;

        // Check if it's an active source
        lock (_activeSourcesLock)
        {
            if (_activeSources.Contains(block))
                return true;
        }

        // Check if it has any active incoming connections
        return _activeInDegree.TryGetValue(block, out var degree) && degree > 0;
    }

    #endregion

    #region Source Management

    /// <summary>
    /// Marks a source block as active.
    /// Call this when a source begins emitting shipments.
    /// </summary>
    /// <remarks>
    /// Active connections are rebuilt at cycle boundaries to ensure consistency.
    /// </remarks>
    public void MarkSourceActive(IBlock sourceBlock)
    {
        lock (_activeSourcesLock)
        {
            _activeSources.Add(sourceBlock);
        }
    }

    /// <summary>
    /// Marks a source block as inactive.
    /// Call this when a source is exhausted.
    /// </summary>
    /// <remarks>
    /// Active connection rebuild is deferred until next shipment cycle boundary.
    /// </remarks>
    public void MarkSourceInactive(IBlock sourceBlock)
    {
        lock (_activeSourcesLock)
        {
            _activeSources.Remove(sourceBlock);
        }
    }

    /// <summary>
    /// Checks if a block is currently an active source.
    /// </summary>
    public bool IsActiveSource(IBlock block)
    {
        lock (_activeSourcesLock)
        {
            return _activeSources.Contains(block);
        }
    }

    /// <summary>
    /// Executes an action for each active source in a thread-safe manner.
    /// </summary>
    public void ForEachActiveSource(Action<IBlock> action)
    {
        lock (_activeSourcesLock)
        {
            foreach (var source in _activeSources)
            {
                action(source);
            }
        }
    }

    #endregion

    #region Cycle Management

    /// <summary>
    /// Initializes active incoming connections and active in-degree for all blocks.
    /// Should be called after ActiveSources is initially populated or when cycle resets.
    /// </summary>
    /// <remarks>
    /// Builds new connection lists first, then atomically replaces them.
    /// This ensures thread-safe reads during execution.
    /// </remarks>
    public void InitializeActiveConnections()
    {
        // Compute which blocks have active upstream sources
        var blocksWithActiveUpstream = ComputeBlocksWithActiveUpstream();

        // Build new connection lists (not yet visible to readers)
        var newConnections = new Dictionary<IBlock, List<Connection>>();
        foreach (var block in Graph.Blocks)
        {
            newConnections[block] = [];
        }

        // Populate new connection lists based on active sources
        foreach (var connection in Graph.Connections)
        {
            // Only add connection if source has active upstream (or is active source itself)
            if (blocksWithActiveUpstream.Contains(connection.Source))
            {
                newConnections[connection.Target].Add(connection);
            }
        }

        // Atomically replace all lists and update degrees
        foreach (var block in Graph.Blocks)
        {
            var connections = newConnections[block];
            _activeIncomingConnections[block] = connections;
            _activeInDegree[block] = connections.Count;
        }
    }

    /// <summary>
    /// Resets execution state for the next shipment cycle.
    /// </summary>
    public void ResetForNextShipment()
    {
        // Clear warehouses and barriers
        _warehouses.Clear();
        _barriers.Clear();

        // Reset all block states to Pending (except blocked and active sources)
        foreach (var block in Graph.Blocks)
        {
            if (!IsBlocked(block))
            {
                _blockStates[block] = BlockExecutionState.Pending;
            }
        }

        // Re-mark active sources as Ready
        lock (_activeSourcesLock)
        {
            foreach (var source in _activeSources)
            {
                if (!IsBlocked(source))
                {
                    _blockStates[source] = BlockExecutionState.Ready;
                }
            }
        }

        // Rebuild active connections for the new cycle
        InitializeActiveConnections();
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Computes the set of blocks that have active upstream sources using BFS.
    /// Returns a set of all blocks reachable from active sources.
    /// </summary>
    private HashSet<IBlock> ComputeBlocksWithActiveUpstream()
    {
        var blocksWithActiveUpstream = new HashSet<IBlock>();
        var queue = new Queue<IBlock>();

        // Start from all active sources
        lock (_activeSourcesLock)
        {
            foreach (var source in _activeSources)
            {
                if (!IsBlocked(source))
                {
                    blocksWithActiveUpstream.Add(source);
                    queue.Enqueue(source);
                }
            }
        }

        // BFS to mark all downstream blocks
        var visited = new HashSet<IBlock>();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            if (_downstreamBlocks.TryGetValue(current, out var downstreamSet))
            {
                foreach (var downstream in downstreamSet)
                {
                    if (!IsBlocked(downstream))
                    {
                        blocksWithActiveUpstream.Add(downstream);
                        queue.Enqueue(downstream);
                    }
                }
            }
        }

        return blocksWithActiveUpstream;
    }

    #endregion
}
