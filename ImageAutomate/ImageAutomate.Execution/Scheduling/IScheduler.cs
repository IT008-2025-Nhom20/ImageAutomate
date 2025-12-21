using ImageAutomate.Core;

namespace ImageAutomate.Execution.Scheduling;

/// <summary>
/// Interface for pipeline block schedulers.
/// </summary>
/// <remarks>
/// Schedulers are responsible for:
/// <list type="bullet">
/// <item>Tracking block readiness via dependency barriers</item>
/// <item>Prioritizing block execution order</item>
/// <item>Handling blocked block propagation</item>
/// <item>Managing shipment cycle transitions</item>
/// </list>
/// 
/// The executor only asks two questions:
/// <list type="number">
/// <item>Is there pending work? (<see cref="HasPendingWork"/>)</item>
/// <item>What's the next block? (<see cref="TryDequeue"/>)</item>
/// </list>
/// 
/// And notifies outcomes:
/// <list type="bullet">
/// <item>Block completed (<see cref="NotifyCompleted"/>)</item>
/// <item>Block blocked/failed (<see cref="NotifyBlocked"/>)</item>
/// </list>
/// </remarks>
internal interface IScheduler
{
    /// <summary>
    /// Gets whether the scheduler has pending work (queued or could be queued).
    /// </summary>
    bool HasPendingWork { get; }

    /// <summary>
    /// Initializes the scheduler with source blocks from the context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    void Initialize(ExecutionContext context);

    /// <summary>
    /// Attempts to dequeue the next block ready for execution.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The next block to execute, or null if no ready blocks available.</returns>
    IBlock? TryDequeue(ExecutionContext context);

    /// <summary>
    /// Notifies the scheduler that a block has completed execution.
    /// </summary>
    /// <param name="completedBlock">The block that completed.</param>
    /// <param name="context">The execution context.</param>
    /// <remarks>
    /// The scheduler will internally signal barriers and enqueue ready downstream blocks.
    /// </remarks>
    void NotifyCompleted(IBlock completedBlock, ExecutionContext context);

    /// <summary>
    /// Notifies the scheduler that a block is blocked and cannot execute.
    /// </summary>
    /// <param name="blockedBlock">The block that is blocked.</param>
    /// <param name="context">The execution context.</param>
    /// <remarks>
    /// The scheduler will handle warehouse cleanup and signal downstream barriers.
    /// </remarks>
    void NotifyBlocked(IBlock blockedBlock, ExecutionContext context);

    /// <summary>
    /// Begins the next shipment cycle.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <remarks>
    /// Called when the current cycle completes but sources still have data.
    /// The scheduler will re-enqueue active sources.
    /// </remarks>
    void BeginNextShipmentCycle(ExecutionContext context);
}
