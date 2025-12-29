namespace ImageAutomate.Execution;

/// <summary>
/// Represents the execution state of a block in the pipeline.
/// </summary>
/// <remarks>
/// State transitions:
/// - Pending → Ready (when all dependencies satisfied via barrier signal)
/// - Ready → Running (when dequeued and execution starts)
/// - Running → Completed (successful execution)
/// - Running → Failed (execution threw exception)
/// - Any → Blocked (transitive propagation from failed upstream blocks)
/// - Any → Cancelled (user cancellation)
/// </remarks>
public enum BlockExecutionState
{
    /// <summary>
    /// Block is pending execution (waiting for dependencies to be satisfied).
    /// This is the initial state for all blocks.
    /// </summary>
    Pending,

    /// <summary>
    /// Block is ready to execute (all dependencies satisfied, enqueued in scheduler).
    /// </summary>
    Ready,

    /// <summary>
    /// Block is currently executing on a worker thread.
    /// </summary>
    Running,

    /// <summary>
    /// Block has completed execution successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Block execution threw an exception.
    /// </summary>
    Failed,

    /// <summary>
    /// Block is downstream of a failed block and will be skipped.
    /// </summary>
    Blocked,

    /// <summary>
    /// Block execution was cancelled by the user.
    /// </summary>
    Cancelled
}
