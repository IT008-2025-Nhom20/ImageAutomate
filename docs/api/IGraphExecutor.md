# IGraphExecutor

`IGraphExecutor` defines the interface for the execution engine responsible for processing the `PipelineGraph`.

## Definition

```csharp
public interface IGraphExecutor
{
    void Execute(PipelineGraph graph);

    Task ExecuteAsync(
        PipelineGraph graph,
        ExecutorConfiguration? configuration = null,
        CancellationToken cancellationToken = default);
}
```

## Methods

### `Execute`
Synchronously executes the provided pipeline graph.
*   **Parameters**: `graph` - The `PipelineGraph` to execute.

### `ExecuteAsync`
Asynchronously executes the pipeline graph, allowing for configuration and cancellation.
*   **Parameters**:
    *   `graph`: The `PipelineGraph` to execute.
    *   `configuration`: Optional `ExecutorConfiguration` to tune execution parameters.
    *   `cancellationToken`: Token to cancel the execution operation.
*   **Returns**: A `Task` representing the asynchronous execution.