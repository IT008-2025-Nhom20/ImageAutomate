using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Execution;

/// <summary>
/// Factory for creating scheduler instances based on execution mode.
/// </summary>
internal static class SchedulerFactory
{
    /// <summary>
    /// Creates a scheduler for the specified execution mode.
    /// </summary>
    /// <param name="mode">The execution mode.</param>
    /// <returns>A scheduler instance.</returns>
    /// <exception cref="NotImplementedException">Thrown when Adaptive Mode is selected.</exception>
    /// <exception cref="ArgumentException">Thrown for unknown execution modes.</exception>
    public static IScheduler CreateScheduler(ExecutionMode mode)
    {
        return mode switch
        {
            ExecutionMode.SimpleDfs => new SimpleDfsScheduler(),
            
            // Adaptive and AdaptiveBatched modes commented out until Mode B implementation
            // ExecutionMode.Adaptive => throw new NotImplementedException(...),
            // ExecutionMode.AdaptiveBatched => throw new NotImplementedException(...),
            
            _ => throw new ArgumentException($"Unknown execution mode: {mode}", nameof(mode))
        };
    }
}
