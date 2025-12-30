using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Execution;

/// <summary>
/// Factory for creating scheduler instances based on execution mode.
/// Uses the global scheduler registry for all lookups.
/// </summary>
public static class SchedulerFactory
{
    private static readonly SchedulerRegistry _registry = new();

    /// <summary>
    /// Gets the global scheduler registry for registration.
    /// </summary>
    public static SchedulerRegistry Registry => _registry;

    /// <summary>
    /// Static constructor - registers built-in schedulers.
    /// </summary>
    static SchedulerFactory()
    {
        _registry.RegisterScheduler("SimpleDfs", () => new SimpleDfsScheduler());
    }

    /// <summary>
    /// Creates a scheduler for the specified mode.
    /// </summary>
    /// <param name="mode">The execution mode or custom scheduler name.
    /// Built-in modes: "SimpleDfs", "Adaptive" (not implemented), "AdaptiveBatched" (not implemented).
    /// Custom: Use registered scheduler name from plugins.</param>
    /// <returns>A scheduler instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when mode is not registered.</exception>
    public static IScheduler CreateScheduler(string mode)
    {
        return _registry.CreateScheduler(mode);
    }
}
