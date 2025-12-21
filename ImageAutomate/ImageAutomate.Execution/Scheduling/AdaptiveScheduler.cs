using ImageAutomate.Core;

namespace ImageAutomate.Execution.Scheduling;

/// <summary>
/// Adaptive scheduler with live cost profiling and critical path analysis.
/// Experimental. Not yet implemented.
/// </summary>
/// <remarks>
/// Reserved for future implementation. All methods throw NotImplementedException.
/// </remarks>
internal sealed class AdaptiveScheduler : IScheduler
{
    #region Reserved for Adaptive Mode Implementation

    #endregion

    public bool IsEmpty => throw new NotImplementedException(
        "Adaptive Mode is not yet implemented. Use ExecutionMode.SimpleDfs instead.");

    public void Enqueue(IBlock block, ExecutionContext context)
    {
        throw new NotImplementedException(
            "Adaptive Mode is not yet implemented. Use ExecutionMode.SimpleDfs instead.");
    }

    public IBlock? TryDequeue(ExecutionContext context)
    {
        throw new NotImplementedException(
            "Adaptive Mode is not yet implemented. Use ExecutionMode.SimpleDfs instead.");
    }
}
