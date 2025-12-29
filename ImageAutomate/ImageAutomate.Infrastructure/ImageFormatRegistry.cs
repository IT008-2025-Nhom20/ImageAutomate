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
    private volatile IReadOnlyList<string>? _cachedFormats;

    private ImageFormatRegistry()
    {
    }

    /// <summary>
    /// Registers a format strategy.
    /// </summary>
    /// <param name="formatName">Unique format name (case-insensitive).</param>
    /// <param name="strategy">Format strategy instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when strategy is null.</exception>
    public void RegisterFormat(string formatName, IImageFormatStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(formatName, nameof(formatName));
        ArgumentNullException.ThrowIfNull(strategy, nameof(strategy));

        lock (_lock)
        {
            _strategies[formatName] = strategy;
            _cachedFormats = null; // Invalidate cache
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
            bool removed = _strategies.Remove(formatName);
            if (removed)
            {
                _cachedFormats = null; // Invalidate cache
            }
            return removed;
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
        // Check cache first (double-check locking pattern)
        if (_cachedFormats != null)
        {
            return _cachedFormats;
        }

        lock (_lock)
        {
            // Check again inside lock
            if (_cachedFormats == null)
            {
                _cachedFormats = _strategies.Keys.ToList();
            }
            return _cachedFormats;
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
