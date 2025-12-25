namespace ImageAutomate.Execution;

/// <summary>
/// Specifies the execution mode for the pipeline engine.
/// </summary>
[Obsolete("Use string-based scheduler names instead. This enum exists for backward compatibility.")]
public enum ExecutionMode
{
    /// <summary>
    /// Simple Depth-First Search (DFS) scheduling with Completion Pressure priority.
    /// Default mode. No profiling or critical path analysis.
    /// </summary>
    SimpleDfs = 0,

    /// <summary>
    /// Adaptive scheduling with live cost profiling and critical path analysis.
    /// Experimental.
    /// </summary>
    /// <remarks>
    /// Throws NotImplementedException until implemented.
    /// </remarks>
    // Adaptive = 1,

    /// <summary>
    /// Adaptive scheduling with batch-based critical path recomputation.
    /// Adaptive Mode variant - Experimental.
    /// </summary>
    /// <remarks
    /// Throws NotImplementedException until implemented.
    /// </remarks>
    // AdaptiveBatched = 2
}
