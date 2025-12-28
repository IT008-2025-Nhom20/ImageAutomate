using ImageAutomate.Core;

namespace ImageAutomate.Infrastructure;

/// <summary>
/// Thread-safe registry for image format strategies.
/// </summary>
public sealed class ImageFormatRegistry : IImageFormatRegistry
{
    /// <summary>
    /// Gets the shared singleton instance of the registry.
    /// </summary>
    public static ImageFormatRegistry Instance { get; } = new();

    private readonly Dictionary<string, IImageFormatStrategy> _strategies = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    /// <summary>
    /// Registers a format strategy.
    /// </summary>
    /// <param name="formatName">Unique format name (case-insensitive).</param>
    /// <param name="strategy">Format strategy instance.</param>
    /// <exception cref="ArgumentException">Thrown when format name is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when strategy is null.</exception>
    public void RegisterFormat(string formatName, IImageFormatStrategy strategy)
    {
        if (string.IsNullOrEmpty(formatName))
        {
            throw new ArgumentException("Format name cannot be null or empty.", nameof(formatName));
        }
        if (strategy == null)
        {
            throw new ArgumentNullException(nameof(strategy));
        }

        lock (_lock)
        {
            _strategies[formatName] = strategy;
        }
    }

    /// <summary>
    /// Unregisters a format.
    /// </summary>
    /// <param name="formatName">Format name to unregister.</param>
    /// <returns>True if unregistered, false if not found.</returns>
    public bool UnregisterFormat(string formatName)
    {
        lock (_lock)
        {
            return _strategies.Remove(formatName);
        }
    }

    /// <summary>
    /// Gets a format strategy by name.
    /// </summary>
    /// <param name="formatName">Format name to look up.</param>
    /// <returns>Format strategy, or null if not found.</returns>
    public IImageFormatStrategy? GetFormat(string formatName)
    {
        lock (_lock)
        {
            return _strategies.GetValueOrDefault(formatName);
        }
    }

    /// <summary>
    /// Gets all registered format names.
    /// </summary>
    /// <returns>List of registered format names.</returns>
    public IReadOnlyList<string> GetRegisteredFormats()
    {
        lock (_lock)
        {
            return _strategies.Keys.ToList();
        }
    }

    /// <summary>
    /// Checks if a format is registered.
    /// </summary>
    /// <param name="formatName">Format name to check.</param>
    /// <returns>True if registered, false otherwise.</returns>
    public bool IsFormatRegistered(string formatName)
    {
        lock (_lock)
        {
            return _strategies.ContainsKey(formatName);
        }
    }
}
