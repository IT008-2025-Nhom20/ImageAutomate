namespace ImageAutomate.Execution;

/// <summary>
/// Configuration options for the pipeline executor.
/// </summary>
public class ExecutorConfiguration
{
    /// <summary>
    /// Gets or sets the execution mode or custom scheduler name.
    /// Built-in modes: "SimpleDfs", "Adaptive" (not implemented), "AdaptiveBatched" (not implemented).
    /// Custom: Use registered scheduler name from plugins.
    /// Default: "SimpleDfs".
    /// </summary>
    public string Mode { get; set; } = "SimpleDfs";

    /// <summary>
    /// Gets or sets the maximum degree of parallelism (concurrent block executions).
    /// Default: Number of logical processors.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the watchdog timeout for deadlock detection.
    /// If no progress occurs within this duration, a PipelineDeadlockException is thrown.
    /// Default: 30 seconds.
    /// </summary>
    /// <remarks>
    /// With a batch-based execution model, 30 seconds was decided to be a reasonable default.
    /// However, a more robust deadlock detection mechanism should be implemented in the future.
    /// </remarks>
    public TimeSpan WatchdogTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether GC throttling is enabled.
    /// When enabled, the engine pauses dispatches if GC frequency exceeds 10/second.
    /// Default: true.
    /// </summary>
    public bool EnableGcThrottling { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of work items to process per shipment.
    /// Used for batching work from shipment sources (e.g., LoadBlock).
    /// Higher values increase memory pressure but reduce overhead.
    /// Default: 64 work items.
    /// </summary>
    public int MaxShipmentSize { get; set; } = 64;

    #region Adaptive Mode Configuration

    /// <summary>
    /// Gets or sets the profiling window size for cost estimation (Adaptive Mode only).
    /// Default: 20 samples.
    /// </summary>
    public int ProfilingWindowSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the exponential moving average alpha for cost profiling (Adaptive Mode only).
    /// Default: 0.2 (emphasizes recent behavior).
    /// </summary>
    public double CostEmaAlpha { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the critical path recomputation interval in blocks (Adaptive Mode only).
    /// Default: Every 10 blocks.
    /// </summary>
    public int CriticalPathRecomputeInterval { get; set; } = 10;

    /// <summary>
    /// Gets or sets the batch size for grouped scheduling (AdaptiveBatched mode only).
    /// Default: 5 blocks.
    /// </summary>
    public int BatchSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the critical path boost multiplier (Adaptive Mode only).
    /// Blocks on the critical path receive priority × this multiplier.
    /// Default: 1.5.
    /// </summary>
    public double CriticalPathBoost { get; set; } = 1.5;

    #endregion
}
