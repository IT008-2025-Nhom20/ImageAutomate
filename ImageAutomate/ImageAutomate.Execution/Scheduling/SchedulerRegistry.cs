using System.Collections.Concurrent;

namespace ImageAutomate.Execution.Scheduling;

/// <summary>
/// Thread-safe singleton registry for task schedulers.
/// </summary>
public sealed class SchedulerRegistry
{
    /// <summary>
    /// Gets the shared singleton instance of the registry.
    /// </summary>
    public static SchedulerRegistry Instance { get; } = new();

    private readonly ConcurrentDictionary<string, Func<IScheduler>> _factories = new(StringComparer.OrdinalIgnoreCase);

    private SchedulerRegistry()
    {
        // Register default schedulers
        RegisterScheduler("simpledfs", () => new SimpleDfsScheduler());
        RegisterScheduler("adaptive", () => new AdaptiveScheduler());
    }

    /// <summary>
    /// Registers a scheduler factory.
    /// </summary>
    /// <param name="name">Unique scheduler name (case-insensitive).</param>
    /// <param name="factory">Function that creates a new scheduler instance.</param>
    public void RegisterScheduler(string name, Func<IScheduler> factory)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Scheduler name cannot be null or empty.", nameof(name));
        }
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _factories[name] = factory;
    }

    /// <summary>
    /// Creates a scheduler instance by name.
    /// </summary>
    /// <param name="name">Name of the scheduler to create.</param>
    /// <returns>A new scheduler instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the scheduler is not registered.</exception>
    public IScheduler CreateScheduler(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            // Default to simpledfs if name is null/empty, matching historical behavior.
            name = "simpledfs";
        }

        if (_factories.TryGetValue(name, out var factory))
        {
            return factory();
        }

        throw new KeyNotFoundException($"Scheduler '{name}' is not registered.");
    }

    /// <summary>
    /// Gets all registered scheduler names.
    /// </summary>
    public IReadOnlyList<string> GetRegisteredSchedulers()
    {
        return _factories.Keys.ToList();
    }
}
