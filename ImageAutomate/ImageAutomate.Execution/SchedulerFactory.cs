using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Execution;

/// <summary>
/// Factory for creating scheduler instances based on execution mode.
/// Uses the global scheduler registry for lookups.
/// </summary>
internal static class SchedulerFactory
{
    private static readonly SchedulerRegistry _registry = new();

    /// <summary>
    /// Gets the global scheduler registry for registration.
    /// </summary>
    public static SchedulerRegistry Registry => _registry;

    /// <summary>
    /// Creates a scheduler for the specified mode.
    /// </summary>
    /// <param name="mode">The execution mode or custom scheduler name.
    /// Built-in modes: "SimpleDfs", "Adaptive" (not implemented), "AdaptiveBatched" (not implemented).
    /// Custom: Use registered scheduler name from plugins.</param>
    /// <returns>A scheduler instance.</returns>
    /// <exception cref="ArgumentException">Thrown for unknown modes.</exception>
    public static IScheduler CreateScheduler(string mode)
    {
        return mode switch
        {
            "SimpleDfs" => new SimpleDfsScheduler(),
            _ => _registry.CreateScheduler(mode)
        };
    }
}
