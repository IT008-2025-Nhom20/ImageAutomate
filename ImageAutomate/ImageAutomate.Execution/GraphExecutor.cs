using System.Diagnostics;
using ImageAutomate.Core;
using ImageAutomate.Execution.Exceptions;

namespace ImageAutomate.Execution;

/// <summary>
/// Executes pipeline graphs using a dataflow-driven execution model.
/// </summary>
public class GraphExecutor : IGraphExecutor
{
    private readonly IGraphValidator _validator;

    public GraphExecutor(IGraphValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Executes the pipeline graph synchronously using default configuration.
    /// </summary>
    public void Execute(PipelineGraph graph)
    {
        ExecuteAsync(graph, new ExecutorConfiguration(), CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Executes the pipeline graph asynchronously with specified configuration.
    /// </summary>
    /// <param name="graph">The pipeline graph to execute.</param>
    /// <param name="configuration">Execution configuration (optional).</param>
    /// <param name="cancellationToken">Cancellation token (optional).</param>
    public async Task ExecuteAsync(
        PipelineGraph graph,
        ExecutorConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        configuration ??= new ExecutorConfiguration();

        // Phase 1: Static Validation
        if (!_validator.Validate(graph))
            throw new PipelineValidationException("Graph validation failed.");

        // Phase 2: Initialization
        var scheduler = SchedulerFactory.CreateScheduler(configuration.Mode);
        var context = new ExecutionContext(graph, configuration, scheduler, cancellationToken);

        // Initialize shipment sources and track them as active
        foreach (var block in graph.Blocks)
        {
            if (block is IShipmentSource shipmentSource)
            {
                shipmentSource.MaxShipmentSize = configuration.MaxShipmentSize;
                context.MarkSourceActive(block);
            }
        }

        // Initialize upstream cache after all sources are marked active
        context.InitializeUpstreamCache();

        // Let scheduler discover and enqueue source blocks
        scheduler.Initialize(context);

        // Phase 3: Runtime Loop
        await ExecuteRuntimeLoopAsync(context);

        // Phase 4: Error Propagation
        if (!context.Exceptions.IsEmpty)
        {
            throw new AggregateException(
                "Pipeline execution failed with one or more errors.",
                context.Exceptions);
        }
    }

    /// <summary>
    /// Main execution loop: dispatch blocks, execute, and signal dependencies.
    /// </summary>
    private async Task ExecuteRuntimeLoopAsync(ExecutionContext context)
    {
        var watchdogTimer = Stopwatch.StartNew();
        var activeTasks = new List<Task>();

        // Engine should break out when finished
        while (true)
        {
            // Check cancellation
            if (context.CancellationToken.IsCancellationRequested)
            {
                throw new PipelineCancelledException("Pipeline execution was cancelled.");
            }

            // Watchdog: Check for deadlock
            if (watchdogTimer.Elapsed > context.Configuration.WatchdogTimeout)
            {
                var timeSinceProgress = DateTime.UtcNow - context.LastProgress;
                if (timeSinceProgress > context.Configuration.WatchdogTimeout)
                {
                    throw new PipelineDeadlockException(
                        $"No progress detected for {timeSinceProgress.TotalSeconds:F1} seconds. " +
                        $"Active blocks: {context.ActiveBlockCount}, Pending work: {context.Scheduler.HasPendingWork}");
                }
                watchdogTimer.Restart();
            }

            // Dispatch blocks while under parallelism limit
            while (context.Scheduler.HasPendingWork
                && context.ActiveBlockCount < context.Configuration.MaxDegreeOfParallelism)
            {
                var block = context.Scheduler.TryDequeue(context);
                if (block == null)
                    break; // No blocks ready

                // Check if blocked (skip execution via scheduler)
                if (context.IsBlocked(block))
                {
                    context.Scheduler.NotifyBlocked(block, context);
                    continue;
                }

                // Dispatch block execution
                context.IncrementActiveBlocks();
                var task = Task.Run(() => ExecuteBlockAsync(block, context), context.CancellationToken);
                activeTasks.Add(task);
            }

            // Wait for at least one task to complete
            if (activeTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(activeTasks);
                activeTasks.Remove(completedTask);

                // Propagate exceptions from completed task
                try
                {
                    await completedTask;
                }
                catch (Exception ex)
                {
                    context.Exceptions.Add(ex);
                }
            }
            else if (!context.Scheduler.HasPendingWork && context.ActiveBlockCount == 0)
            {
                // Shipment cycle complete - check if we should start next cycle
                lock (context.ActiveSourcesLock)
                {
                    if (context.ActiveSources.Count > 0)
                    {
                        // Sources still have shipments - reset and start next cycle
                        context.ResetForNextShipment();
                        context.Scheduler.BeginNextShipmentCycle(context);
                    }
                    else
                    {
                        // No more active sources - execution complete
                        break;
                    }
                }
            }
            else
            {
                // Wait briefly to avoid busy-waiting
                await Task.Delay(20, context.CancellationToken);
            }
        }

        // Wait for all remaining tasks
        if (activeTasks.Count > 0)
        {
            await Task.WhenAll(activeTasks);
        }
    }

    /// <summary>
    /// Executes a single block and handles result propagation.
    /// </summary>
    private async Task ExecuteBlockAsync(IBlock block, ExecutionContext context)
    {
        try
        {
            // Mark as running
            context.BlockStates[block] = BlockExecutionState.Running;

            // Gather inputs from upstream warehouses
            var inputs = GatherInputs(block, context);

            // Execute the block
            var outputs = await Task.Run(() => block.Execute(inputs), context.CancellationToken);

            // Commit outputs to warehouse
            ExportOutputs(block, outputs, context);

            // Dispose consumed inputs immediately
            DisposeInputs(inputs);

            // Check if this is a shipment source that has more shipments
            bool hasMoreShipments = ShouldReEnqueueShipment(block, outputs, context);

            // Notify scheduler of completion FIRST (while source is still in ActiveSources)
            // This allows barriers to properly see this source when checking dependencies
            context.Scheduler.NotifyCompleted(block, context);

            lock (context.ActiveSourcesLock)
            {
                if (context.ActiveSources.Contains(block))
                {
                    if (hasMoreShipments)
                    {
                        // Source still has shipments - keep in ActiveSources
                        // Will be re-enqueued after shipment cycle completes
                        context.BlockStates[block] = BlockExecutionState.Ready;
                    }
                    else
                    {
                        // Source exhausted - mark inactive and update cache
                        context.MarkSourceInactive(block);
                        context.BlockStates[block] = BlockExecutionState.Completed;
                        // Natural flow will stop blocks when warehouses are empty
                    }
                }
                else
                {
                    // Non-source block - mark completed
                    context.BlockStates[block] = BlockExecutionState.Completed;
                }
            }

            // Track progress
            context.IncrementProcessedShipments();
        }
        catch (Exception ex)
        {
            HandleBlockFailure(block, ex, context);
        }
        finally
        {
            context.DecrementActiveBlocks();
        }
    }

    /// <summary>
    /// Determines if a shipment source should be re-enqueued for another execution.
    /// </summary>
    /// <remarks>
    /// A block is re-enqueued if:
    /// 1. It implements IShipmentSource
    /// 2. It has no incoming connections (is a source block)
    /// 3. Its output count equals MaxShipmentSize (indicating more may be available)
    /// 
    /// When output count &lt; MaxShipmentSize, the source is exhausted.
    /// </remarks>
    private bool ShouldReEnqueueShipment(
        IBlock block,
        IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> outputs,
        ExecutionContext context)
    {
        // Only shipment sources can be re-enqueued
        if (block is not IShipmentSource shipmentSource)
            return false;

        // Only source blocks (in-degree == 0) are shipment sources
        if (context.InDegree[block] != 0)
            return false;

        // Check if output count indicates more shipments available
        int totalOutputCount = outputs.Values.Sum(list => list.Count);
        
        // If output count equals max shipment size, assume more shipments exist
        // If output count < max shipment size, source is exhausted
        return totalOutputCount >= shipmentSource.MaxShipmentSize;
    }

    /// <summary>
    /// Gathers inputs for a block from upstream warehouses (implements JIT cloning).
    /// </summary>
    private IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> GatherInputs(IBlock block, ExecutionContext context)
    {
        var inputs = new Dictionary<Socket, List<IBasicWorkItem>>();

        // Find all incoming connections
        var incomingConnections = context.Graph.Connections
            .Where(c => c.Target == block)
            .ToList();

        foreach (var connection in incomingConnections)
        {
            var sourceBlock = connection.Source;
            var targetSocket = connection.TargetSocket;

            // Get warehouse for source block
            if (context.Warehouses.TryGetValue(sourceBlock, out var lazyWarehouse))
            {
                var warehouse = lazyWarehouse.Value;
                // defensive cloning is done by warehouse
                var warehouseOutputs = warehouse.GetInventory();

                // Map source socket to target socket, merging items if multiple connections to same socket
                if (warehouseOutputs.TryGetValue(connection.SourceSocket, out var items))
                {
                    if (!inputs.ContainsKey(targetSocket))
                    {
                        inputs[targetSocket] = new List<IBasicWorkItem>();
                    }
                    inputs[targetSocket].AddRange(items);
                }
            }
        }

        // Convert to readonly dictionary
        return inputs.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<IBasicWorkItem>)kvp.Value);
    }

    /// <summary>
    /// Commits block outputs to its warehouse.
    /// </summary>
    private void ExportOutputs(
        IBlock block,
        IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> outputs,
        ExecutionContext context)
    {
        if (outputs == null || outputs.Count == 0)
            return;

        // Create new warehouse for this execution
        var newWarehouse = new Lazy<Warehouse>(
            () => new Warehouse(context.OutDegree[block]));
        
        context.Warehouses[block] = newWarehouse;
        newWarehouse.Value.Import(outputs);
    }

    /// <summary>
    /// Disposes consumed input work items.
    /// </summary>
    private void DisposeInputs(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        foreach (var items in inputs.Values)
        {
            foreach (var item in items)
            {
                item.Dispose();
            }
        }
    }

    /// <summary>
    /// Blocks downstream blocks that can no longer execute due to a failed block.
    /// </summary>
    /// <remarks>
    /// When a block fails, downstream blocks that require its sockets but have no
    /// alternative active sources for those sockets cannot execute and should be blocked.
    /// 
    /// NOTE: This is only called for failures, not for natural source exhaustion.
    /// Exhausted sources let their final batch flow through naturally.
    /// </remarks>
    private void BlockDownstreamOnFailure(IBlock failedBlock, ExecutionContext context)
    {
        // BFS to find and block all downstream blocks that can't execute
        var visited = new HashSet<IBlock>();
        var queue = new Queue<IBlock>();
        queue.Enqueue(failedBlock);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            // Use precomputed adjacency list instead of LINQ query
            if (!context.DownstreamBlocks.TryGetValue(current, out var downstreamBlocks))
                continue;

            foreach (var downstreamBlock in downstreamBlocks)
            {
                // Skip if already blocked
                if (context.IsBlocked(downstreamBlock))
                    continue;

                // Check if this block has active upstream sources for ALL its required sockets
                // Use precomputed socket sources map
                if (!context.SocketSources.TryGetValue(downstreamBlock, out var socketSourcesMap))
                    continue;

                bool hasAllSocketsCovered = true;
                foreach (var (socket, sources) in socketSourcesMap)
                {
                    // Check if this socket has at least one active upstream source
                    bool socketHasActiveSource = sources.Any(source => 
                        !context.IsBlocked(source) && context.HasActiveUpstreamSource(source));

                    if (!socketHasActiveSource)
                    {
                        hasAllSocketsCovered = false;
                        break;
                    }
                }

                if (!hasAllSocketsCovered)
                {
                    // This block can't execute - block it and continue traversal
                    context.MarkBlocked(downstreamBlock);
                    queue.Enqueue(downstreamBlock);
                }
            }
        }
    }

    /// <summary>
    /// Handles block execution failure: marks blocked, propagates downstream.
    /// </summary>
    private void HandleBlockFailure(IBlock block, Exception exception, ExecutionContext context)
    {
        // Mark as failed (failed blocks are "poisonous" - they behave like blocked
        // blocks but retain their Failed state for diagnostics)
        context.MarkFailed(block);
        
        context.Exceptions.Add(new PipelineExecutionException(
            $"Block '{block.Name}' failed: {exception.Message}", exception));

        // Propagate failure to downstream blocks that can no longer execute
        // Only block blocks that have no alternative active sources for their required sockets
        BlockDownstreamOnFailure(block, context);

        // Notify scheduler (it will signal barriers so blocked blocks can be skipped)
        context.Scheduler.NotifyCompleted(block, context);
    }

}