# Using GraphExecutor in Production

This document outlines the recommended pattern for initializing and using `GraphExecutor` in the UI layer (`EditorView`) to ensure responsiveness, stability, and proper resource management.

## Key Principles

1.  **Offload to Background Thread**: The `GraphExecutor.ExecuteAsync` method contains synchronous preamble logic (Validation and File Scanning). To prevent UI freezes, the entire execution call must be wrapped in `Task.Run`.
2.  **Handle Cancellation**: Always provide a `CancellationToken` to allow users to abort long-running pipelines.
3.  **Manage UI State**: Disable relevant UI controls (e.g., "Execute" button) during execution to prevent re-entrancy.
4.  **Error Handling**: Catch `AggregateException` (execution errors) and `PipelineValidationException` (validation errors) separately.

## Recommended Pattern

### 1. ViewModel / Code-Behind Implementation

```csharp
private CancellationTokenSource? _cancellationSource;

private async void OnExecuteMenuItemClick(object sender, EventArgs e)
{
    // 1. UI State Management
    SetExecutionUIState(isExecuting: true);
    
    // 2. Cancellation Setup
    _cancellationSource?.Dispose();
    _cancellationSource = new CancellationTokenSource();
    var token = _cancellationSource.Token;

    // 3. Executor Initialization
    // Create validator and executor instances (stateless or fresh instances preferred)
    IGraphValidator validator = new GraphValidator();
    IGraphExecutor executor = new GraphExecutor(validator);

    // Configuration (Optional)
    var config = new ExecutorConfiguration 
    { 
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        MaxShipmentSize = 64
    };

    try
    {
        // 4. Execution (Offloaded to ThreadPool)
        // We use Task.Run to ensure the synchronous parts of ExecuteAsync 
        // (Validation + Directory Scanning) do not block the UI thread.
        await Task.Run(async () => 
        {
            await executor.ExecuteAsync(graph, config, token);
        }, token);

        MessageBox.Show("Pipeline completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (OperationCanceledException)
    {
        MessageBox.Show("Execution cancelled by user.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    catch (PipelineValidationException ex)
    {
        // Handle validation errors (e.g., disconnected sockets, cycles)
        MessageBox.Show($"Validation failed:\n{ex.Message}", "Invalid Graph", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    catch (AggregateException aggEx)
    {
        // Handle execution errors (e.g., file not found, corrupt image)
        var messages = string.Join("\n", aggEx.InnerExceptions.Select(e => e.Message));
        MessageBox.Show($"Execution failed:\n{messages}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        // 5. Cleanup
        SetExecutionUIState(isExecuting: false);
        _cancellationSource.Dispose();
        _cancellationSource = null;
    }
}

private void OnCancelMenuItemClick(object sender, EventArgs e)
{
    _cancellationSource?.Cancel();
}

private void SetExecutionUIState(bool isExecuting)
{
    ExecuteMenuItem.Enabled = !isExecuting;
    CancelMenuItem.Enabled = isExecuting;
    // Optionally show/hide progress bar
}
```

## Why this solves the bottlenecks

1.  **Validation Freeze**: By wrapping `executor.ExecuteAsync` in `Task.Run`, the synchronous `GraphValidator.Validate` call executes on a ThreadPool thread, leaving the UI responsive.
2.  **Initialization Freeze**: The `InitializeShipmentSource` method (which scans directories) also runs on the ThreadPool thread.
3.  **Responsiveness**: The `await` keyword ensures the UI thread is freed up to process events (window movement, repainting) while the pipeline runs.
