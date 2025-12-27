namespace ImageAutomate.Core;

/// <summary>
/// Marker interface for blocks that produce work items in batches (shipments).
/// </summary>
/// <remarks>
/// Shipment sources (typically LoadBlock) execute multiple times, producing a limited
/// number of work items per execution to control memory pressure.
/// 
/// A block implementing this interface must also implement IBlock.
/// 
/// Execution flow:
/// 1. Executor calls Execute() on the block
/// 2. Block returns up to MaxShipmentSize work items
/// 3. If output count &lt; MaxShipmentSize, block is exhausted
/// 4. If output count == MaxShipmentSize, block may have more shipments
/// 5. Executor re-enqueues the block until exhausted
/// 
/// This is transparent to downstream blocks - they simply process whatever arrives.
/// </remarks>
public interface IShipmentSource
{
    /// <summary>
    /// Gets or sets the maximum number of work items to produce per execution.
    /// </summary>
    /// <remarks>
    /// This should be set by the executor during initialization.
    /// Default recommendation: 64 work items per shipment.
    /// </remarks>
    int MaxShipmentSize { get; set; }

    /// <summary>
    /// Gets or sets transient data for the current shipment cycle.
    /// </summary>
    /// <remarks>
    /// For LoadBlock, this contains the file paths to load in this batch.
    /// Set by ExecutionContext before Execute(), cleared after.
    /// This property should not be serialized or persisted.
    /// </remarks>
    IReadOnlyList<string>? ShipmentData { get; set; }

    /// <summary>
    /// Scans and returns all targets for shipment processing.
    /// </summary>
    /// <remarks>
    /// Called once during execution bootstrap by the ExecutionContext.
    /// For LoadBlock, this scans the directory, validates images, applies sorting,
    /// and returns the list of file paths to be processed.
    /// </remarks>
    /// <returns>Read-only list of targets (e.g., file paths) to process in shipments.</returns>
    IReadOnlyList<string> GetShipmentTargets();
}
