# ExecutionContext

`ExecutionContext` is a large sealed class that maintains the runtime state of a pipeline execution. While not a public API, it is central to the engine's operation.

## Responsibilities

*   **Topology Analysis**: Precomputes in-degrees, out-degrees, and adjacency lists for efficient graph traversal.
*   **State Management**: Tracks the execution state of each block (`Pending`, `Ready`, `Running`, `Completed`, `Failed`, `Blocked`, `Cancelled`).
*   **Synchronization**: Manages `Warehouse` (data buffers) and `DependencyBarrier` (synchronization primitives) instances.
*   **Progress Tracking**: Counters for active blocks, processed shipments, and last progress time.
*   **Source Management**: Tracks active source blocks to handle graph termination conditions.
*   **Exception Handling**: Records exceptions that occur during execution.

## Key Properties

### Core Properties
*   **`Graph`**: The `PipelineGraph` being executed.
*   **`Configuration`**: The `ExecutorConfiguration` in use.
*   **`Scheduler`**: The `IScheduler` strategy.
*   **`CancellationToken`**: Token for cancellation.

### Progress Tracking
*   **`ProcessedShipmentCount`**: Total number of completed execution units.
*   **`ActiveBlockCount`**: Current number of running blocks.
*   **`LastProgress`**: `TimeSpan` since last progress update.
*   **`HasActiveSources`**: Whether any source blocks are still active.
*   **`HasExceptions`**: Whether any exceptions have been recorded.

### Graph Topology (computed)
*   **`UpstreamBlocks`**: `IReadOnlyDictionary<IBlock, IReadOnlyList<IBlock>>` - upstream dependencies.
*   **`DownstreamBlocks`**: `IReadOnlyDictionary<IBlock, IReadOnlyList<IBlock>>` - downstream consumers.
*   **`InDegree`**: `IReadOnlyDictionary<IBlock, int>` - number of incoming dependencies.
*   **`OutDegree`**: `IReadOnlyDictionary<IBlock, int>` - number of outgoing edges.

### Socket Mapping
*   **`SocketSources`**: `IReadOnlyDictionary<Socket, IShipmentSource>` - maps sockets to their data sources.

## Methods (Internal)

### State Management
*   **`GetBlockState(IBlock block)`**: Returns the current `BlockExecutionState` of a block.
*   **`SetBlockState(IBlock block, BlockExecutionState state)`**: Sets the execution state of a block.
*   **`IsPending(IBlock block)`**: Checks if block is in `Pending` state.
*   **`IsRunning(IBlock block)`**: Checks if block is in `Running` state.
*   **`IsBlocked(IBlock block)`**: Checks if block is in `Blocked` state.
*   **`IsFailed(IBlock block)`**: Checks if block is in `Failed` state.

### Warehouse/Barrier Access
*   **`GetOrCreateWarehouse(IBlock block, int consumerCount)`**: Gets or creates a `Warehouse` for a producer block.
*   **`TryGetWarehouse(IBlock block)`**: Attempts to get an existing `Warehouse` (returns `null` if not found).
*   **`GetOrCreateBarrier(IBlock block, int inDegree)`**: Gets or creates a `DependencyBarrier` for a consumer block.

### Source Tracking
*   **`IsActiveSource(IBlock block)`**: Checks if a block is an active source.
*   **`MarkSourceActive(IBlock block)`**: Marks a block as an active source.
*   **`MarkSourceInactive(IBlock block)`**: Marks a block as inactive source.
*   **`ForEachActiveSource(Action<IBlock> action)`**: Executes action for each active source.

### Exception Handling
*   **`RecordException(Exception exception)`**: Records an exception that occurred during execution.
*   **`GetExceptions()`**: Returns `IReadOnlyList<Exception>` of all recorded exceptions.

## BlockExecutionState Enum

```csharp
public enum BlockExecutionState
{
    Pending,
    Ready,
    Running,
    Completed,
    Failed,
    Blocked,
    Cancelled
}
```
