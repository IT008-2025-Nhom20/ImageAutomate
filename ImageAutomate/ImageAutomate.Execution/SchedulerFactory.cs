using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Execution;

/// <summary>
/// Factory for creating scheduler instances.
/// Delegates to the SchedulerRegistry.
/// </summary>
public static class SchedulerFactory
{
    public static IScheduler CreateScheduler(string schedulerType)
    {
        // Delegate to the singleton registry
        return SchedulerRegistry.Instance.CreateScheduler(schedulerType);
    }
}
