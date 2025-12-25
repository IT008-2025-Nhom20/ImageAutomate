namespace ImageAutomate.Execution.Scheduling;

/// <summary>
/// Thread-safe registry for scheduler factories.
/// Allows plugins to register custom schedulers by name.
/// </summary>
public sealed class SchedulerRegistry
{
    private readonly Dictionary<string, Func<IScheduler>> _factories = new();
    private readonly object _lock = new();

    /// <summary>
    /// Registers a scheduler factory with the specified name.
    /// </summary>
    /// <param name="name">Unique name for the scheduler (case-sensitive).</param>
    /// <param name="factory">Factory function to create scheduler instances.</param>
    /// <returns>True if registered, false if name already exists.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
    public bool RegisterScheduler(string name, Func<IScheduler> factory)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        lock (_lock)
        {
            if (_factories.ContainsKey(name))
            {
                return false;
            }

            _factories[name] = factory;
            return true;
        }
    }

    /// <summary>
    /// Unregisters a scheduler by name.
    /// </summary>
    /// <param name="name">Name of the scheduler to unregister.</param>
    /// <returns>True if unregistered, false if name was not found.</returns>
    public bool UnregisterScheduler(string name)
    {
        lock (_lock)
        {
            return _factories.Remove(name);
        }
    }

    /// <summary>
    /// Creates a scheduler instance by name.
    /// </summary>
    /// <param name="name">Name of the registered scheduler.</param>
    /// <returns>A scheduler instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when name is not registered.</exception>
    public IScheduler CreateScheduler(string name)
    {
        lock (_lock)
        {
            if (!_factories.TryGetValue(name, out var factory))
            {
                throw new KeyNotFoundException($"Scheduler '{name}' is not registered.");
            }

            return factory();
        }
    }

    /// <summary>
    /// Gets all registered scheduler names.
    /// </summary>
    /// <returns>Read-only list of registered scheduler names.</returns>
    public IReadOnlyList<string> GetRegisteredNames()
    {
        lock (_lock)
        {
            return _factories.Keys.ToList();
        }
    }

    /// <summary>
    /// Checks if a scheduler is registered.
    /// </summary>
    /// <param name="name">Name of the scheduler to check.</param>
    /// <returns>True if registered, false otherwise.</returns>
    public bool IsRegistered(string name)
    {
        lock (_lock)
        {
            return _factories.ContainsKey(name);
        }
    }
}
